using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AtikDonusum.Data;
using AtikDonusum.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;

namespace AtikDonusum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WastePointsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WastePointsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WastePoints
        public async Task<IActionResult> Index()
        {
            return View(await _context.WastePoints.ToListAsync());
        }

        // GET: WastePoints/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var wastePoint = await _context.WastePoints
                .FirstOrDefaultAsync(m => m.Id == id);

            if (wastePoint == null) return NotFound();

            return View(wastePoint);
        }

        // GET: WastePoints/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WastePoints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WastePoint wastePoint)
        {
            ModelState.Remove("Enlem");
            ModelState.Remove("Boylam");

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(wastePoint.Adres))
                {
                    // DÜZELTME: Metot artık double dönüyor
                    var koordinatlar = await GetCoordinatesFromAddressAsync(wastePoint.Adres);

                    if (koordinatlar != null)
                    {
                        wastePoint.Enlem = koordinatlar.Value.lat; // double = double (Hata yok)
                        wastePoint.Boylam = koordinatlar.Value.lon; // double = double (Hata yok)
                    }
                    else
                    {
                        ModelState.AddModelError("Adres", "Girilen adres haritada bulunamadı, lütfen daha açık yazın.");
                        return View(wastePoint);
                    }
                }

                _context.Add(wastePoint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wastePoint);
        }

        // GET: WastePoints/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var wastePoint = await _context.WastePoints.FindAsync(id);
            if (wastePoint == null) return NotFound();
            return View(wastePoint);
        }

        // POST: WastePoints/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WastePoint wastePoint)
        {
            if (id != wastePoint.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wastePoint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WastePointExists(wastePoint.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(wastePoint);
        }

        // GET: WastePoints/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var wastePoint = await _context.WastePoints
                .FirstOrDefaultAsync(m => m.Id == id);
            if (wastePoint == null) return NotFound();

            return View(wastePoint);
        }

        // POST: WastePoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wastePoint = await _context.WastePoints.FindAsync(id);
            if (wastePoint != null) _context.WastePoints.Remove(wastePoint);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WastePointExists(int id)
        {
            return _context.WastePoints.Any(e => e.Id == id);
        }

        // ---------------------------------------------------------
        // DÜZELTME: Return tipi (double, double) yapıldı
        // ---------------------------------------------------------
        private async Task<(double lat, double lon)?> GetCoordinatesFromAddressAsync(string address)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "AtikDonusumProjesi/1.0");
                    var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}&limit=1";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(jsonString))
                        {
                            var root = doc.RootElement;
                            if (root.GetArrayLength() > 0)
                            {
                                var element = root[0];

                                string latStr = element.GetProperty("lat").GetString();
                                string lonStr = element.GetProperty("lon").GetString();

                                // DÜZELTME: decimal.Parse yerine double.Parse
                                double lat = double.Parse(latStr, CultureInfo.InvariantCulture);
                                double lon = double.Parse(lonStr, CultureInfo.InvariantCulture);

                                return (lat, lon);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Koordinat bulma hatası: " + ex.Message);
            }

            return null;
        }
    }
}