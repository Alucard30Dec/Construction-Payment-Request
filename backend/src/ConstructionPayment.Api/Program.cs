using System.Text.Json.Serialization;
using ConstructionPayment.Application;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Infrastructure;
using ConstructionPayment.Infrastructure.Persistence;
using ConstructionPayment.Infrastructure.Seed;
using ConstructionPayment.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Cho phép ghi đè cấu hình local (không commit) để chạy Development an toàn.
builder.Configuration
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ConstructionPayment.Application.Validators.LoginRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Construction Payment Request API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [securityScheme] = Array.Empty<string>()
    });
});

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Trong Development, cho phép localhost/127.0.0.1 mọi port
            // để tránh lỗi CORS khi Vite tự chuyển port (5173 -> 5174...).
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    {
                        return false;
                    }

                    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                        || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();
var bootstrapDatabaseOnly = args.Any(x => string.Equals(x, "--bootstrap-db", StringComparison.OrdinalIgnoreCase));

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

static async Task ResetMySqlSchemaAsync(AppDbContext dbContext)
{
    // Drop theo thứ tự phụ thuộc để tránh lỗi FK.
    var dropStatements = new[]
    {
        "DROP TABLE IF EXISTS `PaymentRequestAttachments`;",
        "DROP TABLE IF EXISTS `PaymentRequestApprovalHistories`;",
        "DROP TABLE IF EXISTS `PaymentConfirmations`;",
        "DROP TABLE IF EXISTS `PaymentRequests`;",
        "DROP TABLE IF EXISTS `ApprovalMatrices`;",
        "DROP TABLE IF EXISTS `Contracts`;",
        "DROP TABLE IF EXISTS `AuditLogs`;",
        "DROP TABLE IF EXISTS `Suppliers`;",
        "DROP TABLE IF EXISTS `Projects`;",
        "DROP TABLE IF EXISTS `Users`;",
        "DROP TABLE IF EXISTS `RolePermissionGrants`;",
        "DROP TABLE IF EXISTS `RoleProfiles`;",
        "DROP TABLE IF EXISTS `__EFMigrationsHistory`;"
    };

    foreach (var sql in dropStatements)
    {
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }
}

static async Task ApplySchemaAsync(
    AppDbContext dbContext,
    ILogger logger,
    bool isDevelopment,
    bool isMySql,
    bool isSqlite)
{
    var discoveredMigrations = dbContext.Database.GetMigrations().ToList();
    logger.LogInformation("Tìm thấy {MigrationCount} migration trong assembly runtime.", discoveredMigrations.Count);

    if (isSqlite && isDevelopment)
    {
        // SQLite không hỗ trợ đầy đủ các thao tác migration phức tạp (ví dụ AlterColumn).
        // Trong Development fallback, dùng EnsureCreated để đảm bảo schema khớp model hiện tại.
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("SQLite Development mode: sử dụng EnsureCreated() thay cho Migrate().");
        return;
    }

    if (discoveredMigrations.Count == 0)
    {
        logger.LogWarning(
            "Không phát hiện EF Core migration trong assembly runtime. Sử dụng EnsureCreated() để khởi tạo schema.");
        await dbContext.Database.EnsureCreatedAsync();
        return;
    }

    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex) when (isDevelopment && !isMySql)
    {
        logger.LogWarning(ex,
            "MigrateAsync thất bại trong Development. Thử EnsureCreatedAsync để khởi tạo schema code-first.");
        await dbContext.Database.EnsureCreatedAsync();
    }
}

static async Task<bool> IsCoreSchemaReadyAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
{
    try
    {
        _ = await dbContext.Users.AsNoTracking().AnyAsync(cancellationToken);
        _ = await dbContext.RoleProfiles.AsNoTracking().AnyAsync(cancellationToken);
        return true;
    }
    catch
    {
        return false;
    }
}

static async Task RecreateDevelopmentSchemaAsync(
    AppDbContext dbContext,
    ILogger logger,
    bool isSqlite,
    bool isMySql)
{
    if (isSqlite)
    {
        await dbContext.Database.EnsureDeletedAsync();
    }
    else if (isMySql)
    {
        await ResetMySqlSchemaAsync(dbContext);
    }
    else
    {
        throw new InvalidOperationException("Chỉ hỗ trợ tự khôi phục schema development cho SQLite/MySQL.");
    }

    if (isMySql)
    {
        // TiDB (MySQL-compatible) có thể không hỗ trợ một số collation do EnsureCreated sinh ra.
        // Vì vậy luôn dùng migration SQL khi khôi phục MySQL/TiDB.
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Đã dựng lại schema development bằng Migrate() cho MySQL/TiDB.");
        return;
    }

    // SQLite development fallback.
    await dbContext.Database.EnsureCreatedAsync();
    logger.LogInformation("Đã dựng lại schema development bằng EnsureCreated().");
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupDatabase");
    var providerName = dbContext.Database.ProviderName ?? string.Empty;
    var isSqlite = providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
    var isSqlServer = providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
    var isMySql = providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase);
    var shouldAutoMigrate = app.Environment.IsDevelopment() || isSqlite || isSqlServer || isMySql;
    var enableDemoSeed = builder.Configuration.GetValue<bool?>("Database:SeedDemoData")
        ?? (app.Environment.IsDevelopment() || isSqlite || isMySql);

    logger.LogInformation("Database provider hiện tại: {ProviderName}", providerName);

    if (shouldAutoMigrate)
    {
        await ApplySchemaAsync(dbContext, logger, app.Environment.IsDevelopment(), isMySql, isSqlite);
    }

    var canConnect = await dbContext.Database.CanConnectAsync();
    if (!canConnect)
    {
        throw new InvalidOperationException($"Không kết nối được database (provider: {providerName}).");
    }

    // Kiểm tra schema cốt lõi để tránh app chạy khi DB chưa có bảng quan trọng.
    if (!await IsCoreSchemaReadyAsync(dbContext))
    {
        logger.LogError("Schema database chưa sẵn sàng (không truy cập được bảng Users/RoleProfiles).");

        if (!app.Environment.IsDevelopment() || (!isSqlite && !isMySql))
        {
            throw new InvalidOperationException("Schema database chưa sẵn sàng. Vui lòng kiểm tra migration/code-first setup.");
        }

        logger.LogWarning(
            "Phát hiện schema lỗi trong Development (provider: {ProviderName}). Bắt đầu tự động khôi phục schema.",
            providerName);

        try
        {
            await RecreateDevelopmentSchemaAsync(dbContext, logger, isSqlite, isMySql);

            if (!await IsCoreSchemaReadyAsync(dbContext))
            {
                throw new InvalidOperationException("Dựng lại schema xong nhưng vẫn chưa có bảng cốt lõi.");
            }

            logger.LogInformation("Khôi phục schema development thành công.");
        }
        catch (Exception repairEx)
        {
            logger.LogError(repairEx, "Tự động khôi phục schema thất bại.");
            throw new InvalidOperationException("Schema database chưa sẵn sàng. Vui lòng kiểm tra migration/code-first setup.", repairEx);
        }
    }

    var hasAnyUser = await dbContext.Users.AsNoTracking().AnyAsync();
    var hasAnyRoleProfile = await dbContext.RoleProfiles.AsNoTracking().AnyAsync();
    var shouldSeedDemoData = enableDemoSeed && (bootstrapDatabaseOnly || !hasAnyUser || !hasAnyRoleProfile);

    if (!shouldSeedDemoData)
    {
        logger.LogInformation(
            "Bỏ qua seed dữ liệu mẫu. EnableDemoSeed={EnableDemoSeed}, Bootstrap={Bootstrap}, HasAnyUser={HasAnyUser}, HasAnyRoleProfile={HasAnyRoleProfile}.",
            enableDemoSeed,
            bootstrapDatabaseOnly,
            hasAnyUser,
            hasAnyRoleProfile);
    }
    else
    {
        try
        {
            await DbSeeder.SeedAsync(dbContext, passwordHasher);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seeding dữ liệu mẫu thất bại. Thử reset database development và seed lại.");

            if (!app.Environment.IsDevelopment() || (!isSqlite && !isMySql))
            {
                logger.LogError(
                    "Không tự động reset DB vì môi trường không phải Development hoặc provider không hỗ trợ reset. Dừng khởi động để tránh mất dữ liệu.");
                throw;
            }
            else
            {
                try
                {
                    await RecreateDevelopmentSchemaAsync(dbContext, logger, isSqlite, isMySql);
                    await DbSeeder.SeedAsync(dbContext, passwordHasher);
                    logger.LogInformation("Đã reset database development và seed dữ liệu mẫu thành công.");
                }
                catch (Exception retryEx)
                {
                    logger.LogError(retryEx, "Seed lại sau khi reset database vẫn thất bại.");
                    throw;
                }
            }
        }
    }
}

if (bootstrapDatabaseOnly)
{
    Console.WriteLine("[CPMS] Database bootstrap completed: schema sync + seed demo data.");
    return;
}

app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));
app.MapGet("/health/db", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var configuredProvider = builder.Configuration["DatabaseProvider"] ?? "not-set";
    var provider = dbContext.Database.ProviderName ?? "unknown";
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    if (!canConnect)
    {
        return Results.Json(new
        {
            status = "unreachable",
            configuredProvider,
            provider,
            utc = DateTime.UtcNow
        }, statusCode: 503);
    }

    try
    {
        var userCount = await dbContext.Users.CountAsync(cancellationToken);
        return Results.Ok(new
        {
            status = "ok",
            configuredProvider,
            provider,
            userCount,
            utc = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "schema_error",
            configuredProvider,
            provider,
            message = ex.Message,
            utc = DateTime.UtcNow
        }, statusCode: 500);
    }
});

app.Run();
