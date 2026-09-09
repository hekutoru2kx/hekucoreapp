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
using Hekucoreapp.Domain.Catalogs;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<HekucoreappDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

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


app.UseRequestLocalization(localizationOptions);
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Hekucoreapp.Api.Middleware.UserLanguageMiddleware>();
app.UseMiddleware<Hekucoreapp.Api.Middleware.ActiveUserMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
    var db = scope.ServiceProvider.GetRequiredService<HekucoreappDbContext>();

    // Ensure the singleton AppSettings row exists before anything reads it.
    await AppSettingsSeeder.SeedAsync(db);

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

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
            var bootstrapEmail = builder.Configuration["BootstrapAdminEmail"];
            if (!string.IsNullOrEmpty(bootstrapEmail))
            {
                var bootstrapUser = await userManager.FindByEmailAsync(bootstrapEmail);
                if (bootstrapUser != null)
                    await userRoleRepository.GrantRoleAsync(bootstrapUser.Id, "Admin");
            }
        }
    }
}

app.Run();