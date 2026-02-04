using AtikDonusum.Data;
using AtikDonusum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace AtikDonusum.Controllers
{
    [Authorize] // Sadece giriş yapanlar görebilir
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Kullanıcı Bilgisi
            ViewBag.CurrentUser = user;

            // Duyurular (Son 5)
            ViewBag.AnnouncementsList = await _context.Announcements
                .OrderByDescending(a => a.Tarih)
                .Take(5)
                .ToListAsync();

            // Geçmiş Teslimatlar
            var deliveries = await _context.Deliveries
                .Where(d => d.KullaniciId == user.Id)
                .OrderByDescending(d => d.TeslimatTarihi)
                .ToListAsync();

            ViewBag.UserDeliveries = deliveries;

            // İstatistikler (Profil Sayfası İçin)
            ViewBag.TotalWaste = deliveries.Sum(x => x.MiktarKG).ToString("0.0", CultureInfo.InvariantCulture);
            ViewBag.TotalCount = deliveries.Count;
            ViewBag.PendingCount = deliveries.Count(d => d.Durum == "Beklemede");

            return View();
        }

        // --- 1. ATIK EKLEME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDelivery(string AtikTuru, string MiktarKG, string LokasyonBilgisi, string Enlem, string Boylam)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                double lat = 0, lng = 0; decimal m = 0;
                // Nokta/Virgül dönüşümü yaparak parse et
                if (!string.IsNullOrEmpty(Enlem)) double.TryParse(Enlem.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out lat);
                if (!string.IsNullOrEmpty(Boylam)) double.TryParse(Boylam.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out lng);
                if (!string.IsNullOrEmpty(MiktarKG)) decimal.TryParse(MiktarKG.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out m);

                var delivery = new Delivery
                {
                    KullaniciId = user.Id,
                    AtikTuru = AtikTuru,
                    MiktarKG = m,
                    LokasyonBilgisi = LokasyonBilgisi,
                    Enlem = lat,
                    Boylam = lng,
                    TeslimatTarihi = DateTime.Now,
                    Durum = "Beklemede"
                };

                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Atık talebiniz başarıyla oluşturuldu.";
            }
            return RedirectToAction("Index");
        }

        // --- 2. İPTAL İŞLEMİ ---
        [HttpPost]
        public async Task<IActionResult> CancelDelivery(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            // Sadece kendi bekleyen talebini silebilir
            var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == id && d.KullaniciId == user.Id);

            if (delivery != null && delivery.Durum == "Beklemede")
            {
                _context.Deliveries.Remove(delivery); // Veritabanından siler
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Talep başarıyla iptal edildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bu talep iptal edilemez (Araç yola çıkmış olabilir).";
            }
            return RedirectToAction("Index");
        }

        // --- 3. ŞİFRE DEĞİŞTİRME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Yeni şifreler birbiriyle uyuşmuyor!";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var result = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
            }
            else
            {
                // Hata mesajlarını birleştirip göster
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = "Hata: " + errors;
            }

            return RedirectToAction("Index");
        }
    }
}