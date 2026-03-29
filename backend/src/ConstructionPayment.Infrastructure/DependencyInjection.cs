using System.Collections;
using System.Text;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Infrastructure.Audit;
using ConstructionPayment.Infrastructure.Options;
using ConstructionPayment.Infrastructure.Persistence;
using ConstructionPayment.Infrastructure.Security;
using ConstructionPayment.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ConstructionPayment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        var databaseProvider = configuration["DatabaseProvider"]?.Trim();
        var isDevelopment = hostEnvironment.IsDevelopment();
        var allowSqliteFallbackInDevelopment =
            configuration.GetValue<bool?>("Database:AllowSqliteFallbackInDevelopment") ?? true;

        services.AddDbContextPool<AppDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureSqlServer(options, configuration);
            }
            else if (string.Equals(databaseProvider, "MySql", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ConfigureMySql(options, configuration);
                }
                catch (Exception ex) when (isDevelopment && allowSqliteFallbackInDevelopment)
                {
                    Console.WriteLine(
                        $"[CPMS][WARN] Khong the khoi tao MySQL/TiDB ({ex.GetType().Name}: {ex.Message}). " +
                        "Fallback sang SQLite trong Development de he thong van dang nhap duoc.");
                    ConfigureSqlite(options, configuration);
                }
            }
            else
            {
                ConfigureSqlite(options, configuration);
            }
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        ConfigureAuthentication(services, configuration);
        ConfigurePermissionAuthorization(services);

        return services;
    }

    private static void ConfigureSqlServer(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var sqlServerConnection = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException("Missing SQL Server connection string.");

        options.UseSqlServer(sqlServerConnection, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });
    }

    private static void ConfigureMySql(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var mysqlConnection = ResolveMySqlConnectionString(configuration);
        var serverVersion = ServerVersion.AutoDetect(mysqlConnection);

        options.UseMySql(mysqlConnection, serverVersion, mySqlOptions =>
        {
            mySqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
        });
    }

    private static void ConfigureSqlite(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var sqliteConnection = configuration.GetConnectionString("SqliteConnection")
            ?? throw new InvalidOperationException("Missing SQLite connection string.");

        options.UseSqlite(sqliteConnection, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });
    }

    private static string ResolveMySqlConnectionString(IConfiguration configuration)
    {
        var candidates = new (string? Value, string Source)[]
        {
            (configuration.GetConnectionString("MySqlConnection"), "ConnectionStrings:MySqlConnection"),
            (configuration["ConnectionStrings:MySqlConnection"], "ConnectionStrings:MySqlConnection"),
            (configuration["ConnectionStrings__MySqlConnection"], "ConnectionStrings__MySqlConnection"),
            (configuration["MySqlConnection"], "MySqlConnection"),
            (configuration["DATABASE_URL"], "DATABASE_URL"),
            (configuration["MYSQL_URL"], "MYSQL_URL"),
            (configuration["TIDB_URL"], "TIDB_URL")
        };

        var errors = new List<string>();

        foreach (var (value, source) in candidates)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            try
            {
                var normalized = NormalizeMySqlConnectionString(value);
                normalized = ExpandEnvironmentVariableAlias(normalized);

                if (normalized.Contains("<PASSWORD>", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("still contains <PASSWORD> placeholder.");
                }

                if (normalized.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
                {
                    return ConvertMySqlUrlToAdoConnectionString(normalized);
                }

                ValidateAdoConnectionString(normalized);
                return normalized;
            }
            catch (Exception ex)
            {
                errors.Add($"{source}: {ex.Message}");
            }
        }

        if (errors.Count == 0)
        {
            throw new InvalidOperationException(
                "Missing MySQL connection string. Set `ConnectionStrings__MySqlConnection` " +
                "(or `DATABASE_URL`) in environment variables.");
        }

        throw new InvalidOperationException(
            "MySQL connection string is invalid. " +
            string.Join(" | ", errors));
    }

    private static string NormalizeMySqlConnectionString(string raw)
    {
        var normalized = raw.Trim().Trim('"').Trim();

        if (TryStripPrefixedAssignment(normalized, out var stripped))
        {
            normalized = stripped;
        }

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("is empty.");
        }

        if (LooksLikeConnectionStringKeyOnly(normalized))
        {
            throw new InvalidOperationException(
                $"value looks like an environment variable key (`{normalized}`) instead of a real connection string.");
        }

        return normalized;
    }

    private static string ExpandEnvironmentVariableAlias(string value)
    {
        // Hỗ trợ cấu hình dạng alias: DATABASE_URL, $DATABASE_URL, ${DATABASE_URL}.
        if (!TryGetEnvironmentVariableAlias(value, out var alias))
        {
            return value;
        }

        var aliasValue = FindEnvironmentVariableValue(alias);

        if (string.IsNullOrWhiteSpace(aliasValue))
        {
            return value;
        }

        var normalizedAliasValue = aliasValue.Trim().Trim('"').Trim();

        // Tránh tự tham chiếu vòng lặp kiểu `ConnectionStrings__MySqlConnection=ConnectionStrings__MySqlConnection`.
        if (string.Equals(normalizedAliasValue, value, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return normalizedAliasValue;
    }

    private static string? FindEnvironmentVariableValue(string alias)
    {
        var direct = Environment.GetEnvironmentVariable(alias);
        if (!string.IsNullOrWhiteSpace(direct))
        {
            return direct;
        }

        var allVars = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry entry in allVars)
        {
            if (entry.Key is not string key || entry.Value is not string val)
            {
                continue;
            }

            if (key.Equals(alias, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(val))
            {
                return val;
            }
        }

        return null;
    }

    private static bool TryGetEnvironmentVariableAlias(string value, out string alias)
    {
        alias = string.Empty;

        var candidate = value.Trim();
        if (candidate.StartsWith("${", StringComparison.Ordinal) && candidate.EndsWith('}'))
        {
            candidate = candidate[2..^1].Trim();
        }
        else if (candidate.StartsWith('$'))
        {
            candidate = candidate[1..].Trim();
        }

        if (string.IsNullOrWhiteSpace(candidate)
            || candidate.Contains(';')
            || candidate.Contains("://", StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var ch in candidate)
        {
            if (!(char.IsLetterOrDigit(ch) || ch == '_' || ch == ':'))
            {
                return false;
            }
        }

        alias = candidate;
        return true;
    }

    private static bool LooksLikeConnectionStringKeyOnly(string value)
    {
        return value.Equals("ConnectionStrings__MySqlConnection", StringComparison.OrdinalIgnoreCase)
            || value.Equals("ConnectionStrings:MySqlConnection", StringComparison.OrdinalIgnoreCase)
            || value.Equals("MySqlConnection", StringComparison.OrdinalIgnoreCase)
            || value.Equals("DATABASE_URL", StringComparison.OrdinalIgnoreCase)
            || value.Equals("MYSQL_URL", StringComparison.OrdinalIgnoreCase)
            || value.Equals("TIDB_URL", StringComparison.OrdinalIgnoreCase);
    }

    private static string ConvertMySqlUrlToAdoConnectionString(string mysqlUrl)
    {
        if (!Uri.TryCreate(mysqlUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("invalid mysql:// URL format.");
        }

        var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 3306;

        if (string.IsNullOrWhiteSpace(uri.Host)
            || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("mysql:// URL must include host, username and database.");
        }

        return $"Server={uri.Host};Port={port};Database={database};User Id={username};Password={password};SslMode=Required;AllowPublicKeyRetrieval=true;";
    }

    private static void ValidateAdoConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Server))
        {
            throw new InvalidOperationException("missing Server in ADO.NET connection string.");
        }

        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            throw new InvalidOperationException("missing Database in ADO.NET connection string.");
        }
    }

    private static bool TryStripPrefixedAssignment(string value, out string stripped)
    {
        stripped = string.Empty;

        var equalsIndex = value.IndexOf('=');
        if (equalsIndex <= 0 || equalsIndex >= value.Length - 1)
        {
            return false;
        }

        var key = value[..equalsIndex].Trim();
        if (!key.Equals("ConnectionStrings__MySqlConnection", StringComparison.OrdinalIgnoreCase)
            && !key.Equals("ConnectionStrings:MySqlConnection", StringComparison.OrdinalIgnoreCase)
            && !key.Equals("MySqlConnection", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        stripped = value[(equalsIndex + 1)..].Trim();
        return true;
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing JWT configuration.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
        {
            throw new InvalidOperationException("JWT key is required.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    private static void ConfigurePermissionAuthorization(IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }
}
