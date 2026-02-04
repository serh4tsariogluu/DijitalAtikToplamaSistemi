// Dosya Adı: Controllers/AuthController.cs

using AtikDonusum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AtikDonusum.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // --- GİRİŞ (LOGIN) BÖLÜMÜ ---
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer zaten giriş yapmışsa, 'Home'a değil, 
            // rolüne göre doğru yere yönlendirmeyi dene (veya direk Home'a yolla)
            // Şimdilik en temizi: Giriş yapmışsa Home'a gitsin.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.KullaniciAdi,
                    model.Sifre,
                    isPersistent: false,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    // --- HATA BURADAYDI, DÜZELTİLDİ ---

                    // 1. Giriş yapan kullanıcıyı bul
                    var user = await _userManager.FindByNameAsync(model.KullaniciAdi);

                    // 2. Kullanıcının rolünü kontrol et
                    if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        // EĞER ROLÜ "Admin" İSE: Admin Paneline yönlendir
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        // EĞER ROLÜ "Admin" DEĞİLSE (yani "Kullanici" ise):
                        // Kullanıcı Paneline (Dashboard) yönlendir
                        return RedirectToAction("Index", "Dashboard");
                    }
                    // --- DÜZELTME SONU ---
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                    return View(model);
                                 }
            }
            return View(model);
        }

        // --- ÇIKIŞ (LOGOUT) BÖLÜMÜ ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Ana sayfaya yönlendir
        }

        // --- KAYIT (REGISTER) BÖLÜMÜ ---
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.KullaniciAdi,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,

                    // "Giriş yapamıyorum" hatasının çözümü:
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Sifre);

                if (result.Succeeded)
                {
                    // Kullanıcıyı 'Kullanici' rolüne ata
                    await _userManager.AddToRoleAsync(user, "Kullanici");

                    // Kullanıcıyı hemen giriş yapmış say
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Kullanıcıyı Paneline yönlendir
                    return RedirectToAction("Index", "Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // --- ERİŞİM REDDEDİLDİ (ACCESS DENIED) BÖLÜMÜ ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}