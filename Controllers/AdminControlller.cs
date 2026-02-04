using AtikDonusum.Data;
using AtikDonusum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace AtikDonusum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var deliveries = await _context.Deliveries.OrderByDescending(d => d.TeslimatTarihi).ToListAsync();
                var vehicles = await _context.Vehicles.ToListAsync();
                var routes = await _context.CollectionRoutes.OrderByDescending(r => r.PlanlamaTarihi).ToListAsync();
                var users = await _userManager.GetUsersInRoleAsync("Kullanici");

                // TARİH: Bu Pazartesi ve Geçen Pazartesi
                DateTime today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime thisWeekStart = today.AddDays(-1 * diff).Date;
                DateTime lastWeekStart = thisWeekStart.AddDays(-7);

                // MASTER DATA: Tüm verileri burada toplayacağız
                var masterData = new Dictionary<string, object>();

                // Pasta Grafiği İçin Toplamlar
                var pieRealTotals = new Dictionary<string, double>();
                var pieFutureTotals = new Dictionary<string, double>();

                // Genel Toplamlar (Hepsini toplamak için)
                var generalLast = new double[7];
                var generalThis = new double[7];
                var generalNext = new double[7];

                // Atık Türlerini Belirle
                var uniqueTypes = deliveries.Select(d => CleanWasteName(d.AtikTuru)).Distinct().Where(t => t != "Diğer").ToList();
                uniqueTypes.Add("Diğer"); // Diğer'i en sona ekle

                foreach (var type in uniqueTypes)
                {
                    var typeData = deliveries.Where(d => CleanWasteName(d.AtikTuru) == type).ToList();

                    // A) GERÇEK VERİLER (Geçen ve Bu Hafta)
                    var lastWeekArr = new double[7];
                    var thisWeekArr = new double[7];
                    for (int i = 0; i < 7; i++)
                    {
                        double v1 = (double)typeData.Where(d => d.TeslimatTarihi.Date == lastWeekStart.AddDays(i)).Sum(x => x.MiktarKG);
                        double v2 = (double)typeData.Where(d => d.TeslimatTarihi.Date == thisWeekStart.AddDays(i)).Sum(x => x.MiktarKG);
                        lastWeekArr[i] = v1;
                        thisWeekArr[i] = v2;
                        generalLast[i] += v1;
                        generalThis[i] += v2;
                    }

                    // B) GELECEK HAFTA (YAPAY ZEKA)
                    var nextWeekArr = new double[7];
                    bool aiSuccess = false;

                    // Python'a Bağlan
                    if (typeData.Any())
                    {
                        try
                        {
                            var egitimData = typeData.Select(d => new { Tarih = d.TeslimatTarihi.ToString("yyyy-MM-dd"), Miktar = (double)d.MiktarKG, Tur = type }).ToList();
                            var content = new StringContent(JsonSerializer.Serialize(egitimData), Encoding.UTF8, "application/json");
                            var response = await _httpClient.PostAsync("http://127.0.0.1:5000/tahmin_et", content);
                            if (response.IsSuccessStatusCode)
                            {
                                using (JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
                                {
                                    var arr = doc.RootElement.GetProperty("tahminler");
                                    int idx = 0;
                                    foreach (var t in arr.EnumerateArray())
                                    {
                                        if (idx < 7)
                                        {
                                            double val = t.GetProperty("TahminKG").GetDouble();
                                            nextWeekArr[idx] = val;
                                            generalNext[idx] += val; // Genele Ekle
                                            idx++;
                                        }
                                    }
                                    aiSuccess = true;
                                }
                            }
                        }
                        catch { }
                    }

                    // Python Yoksa Simülasyon
                    if (!aiSuccess)
                    {
                        double avg = lastWeekArr.Average() > 0 ? lastWeekArr.Average() : 10;
                        Random rnd = new Random();
                        for (int i = 0; i < 7; i++)
                        {
                            double val = Math.Round(avg * (0.8 + rnd.NextDouble() * 0.4), 1);
                            nextWeekArr[i] = val;
                            generalNext[i] += val;
                        }
                    }

                    // C) TOPLAMLAR (PASTA GRAFİĞİ İÇİN)
                    double tReal = Math.Round(typeData.Sum(x => (double)x.MiktarKG), 1);
                    double tFuture = Math.Round(nextWeekArr.Sum(), 1);

                    pieRealTotals[type] = tReal;
                    pieFutureTotals[type] = tFuture;

                    // D) BÖLGESEL VERİ (MAHALLELER)
                    var regions = PrepareRegions(typeData, tFuture, tReal);
                    masterData.Add(type, new { Daily = new { Last = lastWeekArr, This = thisWeekArr, Next = nextWeekArr }, Regions = regions });
                }

                // GENEL VERİYİ OLUŞTUR
                double grandTotalReal = deliveries.Sum(x => (double)x.MiktarKG);
                double grandTotalForecast = generalNext.Sum();
                var generalRegions = PrepareRegions(deliveries, grandTotalForecast, grandTotalReal);

                masterData.Add("Genel", new { Daily = new { Last = generalLast, This = generalThis, Next = generalNext }, Regions = generalRegions });

                // VIEW'A GÖNDER
                ViewBag.MasterData = JsonSerializer.Serialize(masterData);
                ViewBag.PieReal = JsonSerializer.Serialize(pieRealTotals);
                ViewBag.PieFuture = JsonSerializer.Serialize(pieFutureTotals);

                // KART BİLGİLERİ (İSTEDİĞİN GELECEK TOPLAMI BURADA)
                ViewBag.TotalKgReal = grandTotalReal.ToString("0.0");
                ViewBag.TotalKgForecast = grandTotalForecast.ToString("0.0"); // <-- GELECEK HAFTA TOPLAMI
                ViewBag.TotalDeliveries = deliveries.Count;

                // Diğer Bilgiler
                ViewBag.VehicleStats = vehicles.Select(v => new { Plaka = v.Plaka, Model = v.Model ?? "-", Kapasite = v.KapasiteKG, Durum = v.Durum }).ToList();
                ViewBag.Announcements = await _context.Announcements.OrderByDescending(a => a.Tarih).ToListAsync();
                ViewBag.DeliveriesList = deliveries;
                ViewBag.AvailableVehicles = vehicles.Where(v => v.Durum == "Aktif").ToList();
                ViewBag.VehiclesList = vehicles;
                ViewBag.RoutesList = routes;
                ViewBag.UsersList = users;
                ViewBag.PendingDeliveries = deliveries.Where(d => d.Durum == "Beklemede" || d.Durum == "Onaylandı").ToList();

                // Fiyat
                double diesel = 45.50;
                ViewBag.DieselPrice = diesel.ToString("0.00");
                ViewBag.DailyFuelCost = (vehicles.Count * 45 * 0.12 * diesel).ToString("0.00");

                return View();
            }
            catch { return View(); }
        }

        // BÖLGESEL HESAPLAMA METODU
        private List<object> PrepareRegions(List<Delivery> data, double totalForecast, double totalReal)
        {
            return data.GroupBy(d => ExtractNeighborhood(d.LokasyonBilgisi)).Select(g => {
                double pKg = (double)g.Sum(x => x.MiktarKG);
                double ratio = totalReal > 0 ? pKg / totalReal : 0;
                double fKg = Math.Round(totalForecast * ratio, 1);

                string rec = fKg > pKg * 1.3 && fKg > 10 ? "Büyük Araç" : (fKg < pKg * 0.7 ? "Ertele" : "Normal");
                string cls = fKg > pKg * 1.3 && fKg > 10 ? "text-danger fw-bold" : "text-muted";

                return new { Mahalle = g.Key, Gecmis = pKg.ToString("0.0"), Gelecek = fKg.ToString("0.0"), Oneri = rec, OneriClass = cls };
            }).OrderByDescending(x => double.Parse(x.Gelecek)).Take(5).ToList<object>();
        }

        private string CleanWasteName(string s) { if (string.IsNullOrEmpty(s)) return "Diğer"; s = s.ToLower(); if (s.Contains("kag") || s.Contains("kağ")) return "Kağıt"; if (s.Contains("pla")) return "Plastik"; if (s.Contains("cam")) return "Cam"; if (s.Contains("met")) return "Metal"; return "Diğer"; }
        private string ExtractNeighborhood(string s) { if (string.IsNullOrWhiteSpace(s)) return "Belirsiz"; var w = s.Split(' '); for (int i = 0; i < w.Length; i++) if (w[i].Contains("Mah")) return w[i]; return w[0]; }

        // CRUD İŞLEMLERİ (AYNI)
        [HttpPost] public async Task<IActionResult> CreateAnnouncement(string Baslik, string Icerik) { if (!string.IsNullOrEmpty(Baslik)) { _context.Announcements.Add(new Announcement { Baslik = Baslik, Icerik = Icerik, Tarih = DateTime.Now }); await _context.SaveChangesAsync(); } return Redirect("/Admin/Index#duyurular"); }
        [HttpPost] public async Task<IActionResult> DeleteAnnouncement(int id) { var a = await _context.Announcements.FindAsync(id); if (a != null) { _context.Announcements.Remove(a); await _context.SaveChangesAsync(); } return Redirect("/Admin/Index#duyurular"); }
        [HttpPost] public async Task<IActionResult> CreateVehicle(Vehicle v) { v.Durum = "Aktif"; _context.Vehicles.Add(v); await _context.SaveChangesAsync(); return Redirect("/Admin/Index#rotalar"); }
        [HttpPost] public async Task<IActionResult> DeleteVehicle(int id) { var v = await _context.Vehicles.FindAsync(id); if (v != null) { _context.Remove(v); await _context.SaveChangesAsync(); } return Redirect("/Admin/Index#rotalar"); }
        [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> CreateDelivery(string KullaniciId, string AtikTuru, string MiktarKG, string LokasyonBilgisi, string Enlem, string Boylam) { try { double lat = 0, lng = 0; decimal m = 0; if (!string.IsNullOrEmpty(Enlem)) double.TryParse(Enlem.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out lat); if (!string.IsNullOrEmpty(Boylam)) double.TryParse(Boylam.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out lng); if (!string.IsNullOrEmpty(MiktarKG)) decimal.TryParse(MiktarKG.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out m); _context.Deliveries.Add(new Delivery { KullaniciId = KullaniciId ?? "admin", AtikTuru = AtikTuru, MiktarKG = m, LokasyonBilgisi = LokasyonBilgisi, Enlem = lat, Boylam = lng, TeslimatTarihi = DateTime.Now, Durum = "Beklemede" }); await _context.SaveChangesAsync(); } catch { } return Redirect("/Admin/Index#teslimatlar"); }
        [HttpPost] public async Task<IActionResult> UpdateDeliveryLocation(int id, string lat, string lng) { var d = await _context.Deliveries.FindAsync(id); if (d != null && double.TryParse(lat?.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out double la) && double.TryParse(lng?.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out double lo)) { d.Enlem = la; d.Boylam = lo; await _context.SaveChangesAsync(); return Json(new { success = true }); } return Json(new { success = false }); }
        [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> CreateRoute(string RotaAdi, int AracId, string Bolge, List<int> TeslimatIdleri) { if (string.IsNullOrWhiteSpace(RotaAdi) || TeslimatIdleri == null || !TeslimatIdleri.Any()) return Redirect("/Admin/Index#rotalar"); var items = await _context.Deliveries.Where(x => TeslimatIdleri.Contains(x.Id)).ToListAsync(); double cLat = 37.2153, cLng = 28.3636; var sorted = items.OrderBy(x => Math.Pow((double)x.Enlem - cLat, 2) + Math.Pow((double)x.Boylam - cLng, 2)).ToList(); var r = new CollectionRoute { RotaAdi = RotaAdi, AracId = AracId, Bolge = Bolge ?? "Genel", PlanlamaTarihi = DateTime.Now, Durum = "Planlandı", ToplanacakNoktalarJson = JsonSerializer.Serialize(sorted.Select(i => i.Id).ToList()) }; _context.CollectionRoutes.Add(r); var arac = await _context.Vehicles.FindAsync(AracId); if (arac != null) { arac.Durum = "Meşgul"; } await _context.SaveChangesAsync(); foreach (var i in items) { i.Durum = "Rota Planlandı"; i.CollectionRouteId = r.Id; } await _context.SaveChangesAsync(); return Redirect("/Admin/Index#rotalar"); }
        [HttpGet] public async Task<IActionResult> GetRouteDetails(int id) { var r = await _context.CollectionRoutes.FindAsync(id); if (r == null) return Json(new { success = false }); var ids = JsonSerializer.Deserialize<List<int>>(r.ToplanacakNoktalarJson ?? "[]"); var all = await _context.Deliveries.Where(x => ids.Contains(x.Id)).ToListAsync(); var s = new List<object>(); foreach (var pid in ids) { var f = all.FirstOrDefault(x => x.Id == pid); if (f != null) s.Add(new { f.Id, f.AtikTuru, f.LokasyonBilgisi, Enlem = (double)f.Enlem, Boylam = (double)f.Boylam }); } return Json(new { success = true, data = s }); }
        [HttpPost] public async Task<IActionResult> EditDelivery(int Id, string Durum) { var d = await _context.Deliveries.FindAsync(Id); if (d != null) { d.Durum = Durum; await _context.SaveChangesAsync(); } return Redirect("/Admin/Index#teslimatlar"); }
        [HttpPost] public async Task<IActionResult> DeleteDelivery(int id) { var d = await _context.Deliveries.FindAsync(id); if (d != null) { _context.Remove(d); await _context.SaveChangesAsync(); } return Redirect("/Admin/Index#teslimatlar"); }
        [HttpPost] public async Task<IActionResult> DeleteRoute(int id) { var r = await _context.CollectionRoutes.FindAsync(id); if (r != null) { var arac = await _context.Vehicles.FindAsync(r.AracId); if (arac != null) { arac.Durum = "Aktif"; } var deliveries = await _context.Deliveries.Where(d => d.CollectionRouteId == id).ToListAsync(); foreach (var d in deliveries) { d.Durum = "Beklemede"; d.CollectionRouteId = null; } _context.Remove(r); await _context.SaveChangesAsync(); } return Redirect("/Admin/Index#rotalar"); }
        [HttpPost] public async Task<IActionResult> CreateUser(string UserName, string Email, string Sifre) { var u = new ApplicationUser { UserName = UserName, Email = Email, EmailConfirmed = true }; var r = await _userManager.CreateAsync(u, Sifre); if (r.Succeeded) await _userManager.AddToRoleAsync(u, "Kullanici"); return Redirect("/Admin/Index#users"); }
        [HttpPost] public async Task<IActionResult> DeleteUser(string id) { var u = await _userManager.FindByIdAsync(id); if (u != null) await _userManager.DeleteAsync(u); return Redirect("/Admin/Index#users"); }
    }
}