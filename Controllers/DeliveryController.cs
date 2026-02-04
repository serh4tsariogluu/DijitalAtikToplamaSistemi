using AtikDonusum.Data;
using AtikDonusum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization; // Kültür ayarı için eklendi
using System.Threading.Tasks;

namespace AtikDonusum.Controllers
{
    [Authorize]
    public class DeliveryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeliveryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Parametreleri string olarak alıyoruz, içeride double'a çevireceğiz
        public async Task<IActionResult> Create(string AtikTuru, decimal MiktarKG, string LokasyonBilgisi, string Enlem, string Boylam)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            try
            {
                // DÜZELTME: Veritabanı 'double' bekliyor, bu yüzden double değişkenler tanımladık.
                double lat = 0;
                double lng = 0;

                // DÜZELTME: decimal.TryParse yerine double.TryParse kullandık.
                if (!string.IsNullOrEmpty(Enlem))
                    double.TryParse(Enlem.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out lat);

                if (!string.IsNullOrEmpty(Boylam))
                    double.TryParse(Boylam.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out lng);

                var teslimat = new Delivery
                {
                    KullaniciId = user.Id,
                    AtikTuru = AtikTuru,
                    MiktarKG = MiktarKG, // Burası decimal kalabilir (Modelde decimal ise)
                    LokasyonBilgisi = LokasyonBilgisi,
                    TeslimatTarihi = DateTime.Now,
                    Durum = "Beklemede",
                    Enlem = lat, // Artık double -> double (Hata yok)
                    Boylam = lng // Artık double -> double (Hata yok)
                };

                _context.Deliveries.Add(teslimat);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kurye talebi başarıyla oluşturuldu!";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = "Hata: " + ex.Message; }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}