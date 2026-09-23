using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Application.Services;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Infrastructure.Data;
using Hekucoreapp.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Hekucoreapp.Infrastructure.Email;
using Hekucoreapp.Infrastructure.Repositories;
using Hekucoreapp.Infrastructure.BackgroundServices;
using Hekucoreapp.Infrastructure.Content;
using Hekucoreapp.Api.ContentAccess;
using Hekucoreapp.Domain.Catalogs;
using Hekucoreapp.Infrastructure.Logging;
using Serilog;
using Serilog.Events;

// Bootstrap logger: active only while the host is being built, so a failure during configuration
// (e.g. a bad connection string) is still captured somewhere. Replaced by the fully-configured
// Serilog pipeline below once the DI container exists.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Logging foundation — see Hekucoreapp.Infrastructure/Logging. CategoryLevelSwitches is the live,
// admin-adjustable minimum level per LogCategory (Http/Database/Security/Integration/Business/
// System); ICategoryLogger is what the rest of the app calls to emit a categorized line. Gating
// happens inside CategoryLogger itself, before anything reaches Serilog's pipeline — so the
// pipeline's own MinimumLevel below must stay permissive (Verbose) for categorized events to ever
// have a chance; framework/library noise that never goes through ICategoryLogger is governed by
// the Override rules instead.
var categoryLevelSwitches = new CategoryLevelSwitches();
builder.Services.AddSingleton(categoryLevelSwitches);
builder.Services.AddSingleton<ICategoryLogger, CategoryLogger>();
builder.Services.AddSingleton<RequestContextEnricher>();

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Hekucoreapp", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.With(services.GetRequiredService<RequestContextEnricher>())
        .Destructure.With<SensitiveDataDestructuringPolicy>()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(AppContext.BaseDirectory, "Logs", "hekucoreapp-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30)
        // Primary, queryable sink — never the only sink (see file/console above): if Postgres
        // itself is the thing that's down, those two still capture events.
        .WriteTo.SystemLogPostgres(context.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Localization
var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

builder.Services.AddLocalization();

// Database
builder.Services.AddDbContext<HekucoreappDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
    options.AddInterceptors(new QueryLoggingInterceptor(
        serviceProvider.GetRequiredService<ICategoryLogger>(),
        serviceProvider.GetRequiredService<IHostEnvironment>()));
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<HekucoreappDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

//User Config
builder.Services.AddScoped<IUserService, UserService>();

//Email Service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<EmailTemplates>();

// DI registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<IRoleManagementRepository, RoleManagementRepository>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
//Logging settings (admin-only, six category rows + one retention row) + its refresh/retention-sweep hosted service
builder.Services.AddScoped<ILoggingSettingsService, LoggingSettingsService>();
builder.Services.AddScoped<ILoggingSettingsRepository, LoggingSettingsRepository>();
builder.Services.AddHostedService<LoggingSettingsRefreshHostedService>();
//System log viewer (admin-only, reads system_logs directly — gated by LoggingSettingsPermission.Read)
builder.Services.AddScoped<ISystemLogsService, SystemLogsService>();
builder.Services.AddScoped<ISystemLogsRepository, SystemLogsRepository>();

// Content / attachments — generic polymorphic layer (see TASKS.md / the design doc). Provider
// defaults to LocalDisk so a fresh clone works with no Azure account; set
// ContentStorage:Provider = "AzureBlob" + the connection string/container to use a real account.
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IContentPartitionResolver, GlobalContentPartitionResolver>();
builder.Services.AddScoped<IContentAccessPolicy, PersonContentAccessPolicy>();
if (string.Equals(builder.Configuration["ContentStorage:Provider"], "AzureBlob", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IContentStorage, AzureBlobContentStorage>();
else
    builder.Services.AddScoped<IContentStorage, LocalDiskContentStorage>();

//Accessor for HttpContext to get the current user in DbContext
builder.Services.AddHttpContextAccessor();

//Geography Service and Repository
builder.Services.AddScoped<IGeographyService, GeographyService>();
builder.Services.AddScoped<IGeographyRepository, GeographyRepository>();
//Person Service and Repository
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Policy-based authorization, generated from PermissionCatalog's registered modules
builder.Services.AddAuthorization(options =>
{
    foreach (var (module, enumType) in PermissionCatalog.Modules)
        foreach (var action in Enum.GetNames(enumType))
            options.AddPolicy($"{module}.{action}", policy => policy.RequireClaim(module, action));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<Hekucoreapp.Api.Middleware.HttpRequestLoggingMiddleware>();

app.UseRequestLocalization(localizationOptions);
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Hekucoreapp.Api.Middleware.UserLanguageMiddleware>();
app.UseMiddleware<Hekucoreapp.Api.Middleware.ActiveUserMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

// Angular client-side routing: a request for a route like /profile or /admin/roles only exists
// in the SPA's router, not as a server-side file or endpoint. The initial load (or any in-app
// navigation) works without this because the SPA's own JS handles it, but a hard refresh or a
// direct link sends that path straight to the server — which needs to fall back to index.html
// (rather than 404) so Angular's router boots and takes over from there.
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
    var roleManagementRepository = scope.ServiceProvider.GetRequiredService<IRoleManagementRepository>();
    var db = scope.ServiceProvider.GetRequiredService<HekucoreappDbContext>();

    // Ensure the singleton AppSettings row exists before anything reads it.
    await AppSettingsSeeder.SeedAsync(db);

    // Logging category rows (one per LogCategory, sensible default minimum levels) + the
    // retention singleton — read/updated by the admin-only admin page and polled live by
    // LoggingSettingsRefreshHostedService.
    await LoggingSettingsSeeder.SeedAsync(db);

    // Bulk-load reference geography (countries/states/cities) from the shipped CSVs on a
    // fresh database. No-ops once each table is populated.
    await GeographySeeder.SeedAsync(db);

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // Bring any missing DefaultRoleCatalog role into existence with its catalog claims. Roles
    // that already exist are left alone — a full reconcile stays behind the admin
    // restore-defaults endpoint. Without this, catalog roles only ever appear when an admin
    // presses that button, so granting one on a fresh database throws RoleNotFound.
    await roleManagementRepository.EnsureDefaultRolesExistAsync();

    var bootstrapEmail = builder.Configuration["BootstrapAdminEmail"];

    // First run against an empty database: create the bootstrap admin account so there is
    // a first way in. Guarded on AspNetUsers being *entirely empty* — never merely "no
    // active admin" — so it can never fire on an established install. The break-glass
    // block below then grants this user the Admin role in the same startup pass.
    if (!string.IsNullOrEmpty(bootstrapEmail) && !await userManager.Users.AnyAsync())
    {
        var configuredPassword = builder.Configuration["BootstrapAdminPassword"];
        var password = string.IsNullOrEmpty(configuredPassword) ? GenerateBootstrapPassword() : configuredPassword;

        var bootstrapUser = new ApplicationUser
        {
            Email = bootstrapEmail,
            UserName = bootstrapEmail,
            EmailConfirmed = true,
            MustChangePassword = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(bootstrapUser, password);
        if (createResult.Succeeded)
        {
            if (string.IsNullOrEmpty(configuredPassword))
                // Deliberately Console.WriteLine, not app.Logger — the generated password must
                // never enter the structured logging pipeline (console/file/DB sinks), since that
                // persists and gets queried later. This is the one-time console line an operator
                // watching startup sees; it is not retained anywhere logging is.
                Console.WriteLine(
                    $"Created bootstrap admin {bootstrapEmail} on the empty database. Temporary password: {password} — sign in and change it now (this is printed only once, and is not logged).");
            else
                app.Logger.LogWarning(
                    "Created bootstrap admin {Email} on the empty database using the configured BootstrapAdminPassword — sign in and change it now.",
                    bootstrapEmail);
        }
        else
        {
            app.Logger.LogWarning(
                "Could not create bootstrap admin {Email}: {Errors}",
                bootstrapEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }

    // Admin always has every registered permission, so it can never lock itself
    // out of a module after that module switches from role checks to policy checks.
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole != null)
    {
        var adminClaims = await roleManager.GetClaimsAsync(adminRole);

        foreach (var (module, enumType) in PermissionCatalog.Modules)
        {
            foreach (var action in Enum.GetNames(enumType))
            {
                if (!adminClaims.Any(c => c.Type == module && c.Value == action))
                    await roleManager.AddClaimAsync(adminRole, new Claim(module, action));
            }
        }

        // Break-glass recovery: if the Admin role has no members (e.g. it was
        // deleted and got recreated above, or the last admin was removed from it),
        // restore a known bootstrap admin so the app can never become unmanageable.
        var hasActiveAdmin = await db.UserRoleAssignments.AnyAsync(ur => ur.RoleId == adminRole.Id && ur.RevokedAt == null);
        if (!hasActiveAdmin)
        {
            if (!string.IsNullOrEmpty(bootstrapEmail))
            {
                var bootstrapUser = await userManager.FindByEmailAsync(bootstrapEmail);
                if (bootstrapUser != null)
                    await userRoleRepository.GrantRoleAsync(bootstrapUser.Id, "Admin");
            }
        }
    }
}

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

// Random password for a generated bootstrap admin: 20 chars from a CSPRNG over an
// unambiguous alphabet, plus one of each required class so it always clears the
// default Identity complexity rules. Only used when BootstrapAdminPassword is unset.
static string GenerateBootstrapPassword()
{
    const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
    return System.Security.Cryptography.RandomNumberGenerator.GetString(alphabet, 20) + "Aa1!";
}