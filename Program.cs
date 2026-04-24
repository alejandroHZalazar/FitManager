using FitManager.Data;
using FitManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(3)));

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie auth ───────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();

// ── App Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<FitManager.Services.IMenuService, FitManager.Services.MenuService>();
builder.Services.AddScoped<FitManager.Services.IMemberService, FitManager.Services.MemberService>();
builder.Services.AddScoped<FitManager.Services.IPaymentService, FitManager.Services.PaymentService>();
builder.Services.AddScoped<FitManager.Services.IPlanService, FitManager.Services.PlanService>();
builder.Services.AddScoped<FitManager.Services.ICashRegisterService, FitManager.Services.CashRegisterService>();
builder.Services.AddScoped<FitManager.Services.IClassService,        FitManager.Services.ClassService>();
builder.Services.AddScoped<FitManager.Services.IReportService,       FitManager.Services.ReportService>();
builder.Services.AddScoped<FitManager.Services.ICompanyService,      FitManager.Services.CompanyService>();
builder.Services.AddScoped<FitManager.Services.IRoutineService,      FitManager.Services.RoutineService>();
builder.Services.AddScoped<FitManager.Services.INutritionService,    FitManager.Services.NutritionService>();

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Seed ──────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos.");
    }
}

app.Run();
