using ElectronicShopMVC.DataAccess.Data;
using ElectronicShopMVC.DataAccess.Repository;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Services;
using ElectronicShopMVC.Utility;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(
        CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate();

var cultureInfo = new CultureInfo("vi-VN");
cultureInfo.NumberFormat.CurrencySymbol = "₫";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddDistributedMemoryCache(); // Lưu Session trong bộ nhớ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn Session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



builder.Services.AddSingleton<VNPayService>();

// Configure SQL Server Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
}, ServiceLifetime.Scoped);


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    o.InvalidModelStateResponseFactory = actionContext =>
    {

        List<Error> error = actionContext.ModelState
                    .Where(modelError => modelError.Value!.Errors.Count > 0)
                    .Select(modelError => new Error
                    {
                        ErrorField = modelError.Key,
                        ErrorDescription = modelError.Value!.Errors.FirstOrDefault()!.ErrorMessage
                    }).ToList();

        return new BadRequestObjectResult(error);
    };
});
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddSingleton<IImageService, ImageService>();

var app = builder.Build();

// Test database connection on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var canConnect = context.Database.CanConnect();
            
            if (canConnect)
            {
                var databaseName = context.Database.GetDbConnection().Database;
                logger.LogInformation("✓ Database connection successful. Database: {Database}", databaseName);
            }
            else
            {
                logger.LogWarning("✗ Cannot connect to database. Please check your connection string in appsettings.json");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ An error occurred while testing database connection.");
            logger.LogError("Please check your connection string in appsettings.json");
            
            // Extract server name from connection string for debugging (safe info only)
            var serverMatch = System.Text.RegularExpressions.Regex.Match(connectionString ?? "", @"Server=([^;]+)");
            if (serverMatch.Success)
            {
                logger.LogError("Connection attempted to server: {Server}", serverMatch.Groups[1].Value);
            }
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "✗ Failed to test database connection during startup.");
}

// Seed roles and default admin user (if configured in appsettings.json under AdminUser)
try
{
    Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Ensure required roles exist
        var roles = new[] { StaticDetails.Role_Cust, StaticDetails.Role_Admin };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role {Role}", role);
            }
        }

        // Read admin credentials from configuration (optional). If missing, fall back to defaults.
        var adminEmail = config["AdminUser:Email"] ?? "admin@example.com";
        var adminPassword = config["AdminUser:Password"] ?? "Admin@12345";

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Name = "Administrator"
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, StaticDetails.Role_Admin);
                    logger.LogInformation("Created admin user {Email}", adminEmail);
                }
                else
                {
                    logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, StaticDetails.Role_Admin))
                {
                    await userManager.AddToRoleAsync(admin, StaticDetails.Role_Admin);
                    logger.LogInformation("Assigned Admin role to existing user {Email}", adminEmail);
                }
            }
        }
        else
        {
            logger.LogInformation("AdminUser not found in configuration; skipped creating default admin user.");
        }
    }).GetAwaiter().GetResult();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding roles/admin user.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseStatusCodePagesWithRedirects("/Error/{0}");
app.Run();
