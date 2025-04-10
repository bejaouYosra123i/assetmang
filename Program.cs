using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement1.Data;

var builder = WebApplication.CreateBuilder(args);

// Ajout des services à l'application
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configuration de la base de données avec Entity Framework Core et SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Ajout des outils pour les développeurs en cas d'erreur de migration
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuration d'ASP.NET Core Identity avec gestion des utilisateurs et rôles
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

// Ajout des contrôleurs avec vues
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Initialisation des données (rôles et utilisateurs par défaut)
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}
var cultureInfo = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
app.MapControllerRoute(
    name: "requestCreate",
    pattern: "Request/Create",
    defaults: new { controller = "InvestmentRequests", action = "Create" });

app.Run();