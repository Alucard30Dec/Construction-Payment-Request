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

        services.AddDbContext<AppDbContext>(options =>
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
        var raw = configuration.GetConnectionString("MySqlConnection")
            ?? throw new InvalidOperationException("Missing MySQL connection string.");

        raw = raw.Trim();

        if (raw.Contains("<PASSWORD>", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("MySQL connection string still contains <PASSWORD>. Please replace with your real password.");
        }

        if (!raw.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
        {
            return raw;
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Invalid mysql:// URL format.");
        }

        var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 3306;

        if (string.IsNullOrWhiteSpace(uri.Host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("mysql:// URL must include host, username and database.");
        }

        return $"Server={uri.Host};Port={port};Database={database};User Id={username};Password={password};SslMode=Required;AllowPublicKeyRetrieval=true;";
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
