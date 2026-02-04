using AtikDonusum.Data;
using AtikDonusum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Adým: Servisleri (Hizmetleri) Konteyner'a Ekleme ---

// Veritabaný Baðlantýsýný (ConnectionString) appsettings.json dosyasýndan oku
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext (Veritabaný) ekleme servisi
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity (Kimlik) servisini ekleme (Rolleri de içerecek þekilde)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    // Þifre politikalarýný geliþtirmek için gevþetebilirsin (Ýsteðe baðlý)
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC (Controller + View) servislerini ekle
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// --- KÜLTÜR AYARI (DECIMAL HATASI ÝÇÝN) ---
var supportedCultures = new[] { new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// --- KRÝTÝK DÜZELTME: LOGIN SORUNUNU ÇÖZEN COOKIE AYARLARI ---
builder.Services.ConfigureApplicationCookie(options =>
{
    // Yönlendirme Yollarý
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.LogoutPath = "/Auth/Logout";

    // Cookie (Çerez) Özellikleri
    options.Cookie.Name = "AtikDonusumAuth"; // Çerez ismini sabitledik
    options.Cookie.HttpOnly = true; // Güvenlik için js eriþimini kapatýr
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 60 dakika oturum açýk kalsýn
    options.SlidingExpiration = true; // Ýþlem yaptýkça süreyi uzat

    // Localhost'ta ve sunucuda sorunsuz çalýþmasý için:
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.IsEssential = true;
});

// --- 2. Adým: Uygulamayý Oluþturma ---
var app = builder.Build();

// --- 3. Adým: HTTP Request Pipeline (Ýstek Akýþýný) Yapýlandýrma ---

if (app.Environment.IsDevelopment())
{
    // Geliþtirme ortamý
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kültür ayarýný uygula
app.UseRequestLocalization();

// Authentication ve Authorization SIRALAMASI ÇOK ÖNEMLÝDÝR
app.UseAuthentication(); // 1. Kimlik Doðrulama
app.UseAuthorization();  // 2. Yetkilendirme

// Varsayýlan Rota
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- DBSEEDER ÇAÐIRMA ---
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await AtikDonusum.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Veritabaný seed iþlemi sýrasýnda bir hata oluþtu.");
}

app.Run();