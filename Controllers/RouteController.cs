using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtikDonusum.Data;
using AtikDonusum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Route = AtikDonusum.Models.CollectionRoute;

namespace AtikDonusum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RouteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RouteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bekleyenTalepler = await _context.Deliveries
                                            .Include(d => d.Kullanici)
                                            .Where(d => d.Durum == "Beklemede" && d.Enlem != 0 && d.Boylam != 0)
                                            .OrderByDescending(d => d.TeslimatTarihi)
                                            .ToListAsync();
            return View(bekleyenTalepler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoute(string RotaAdi, int AracId, int[] TeslimatIdleri)
        {
            if (TeslimatIdleri == null || TeslimatIdleri.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen rotaya eklemek için en az bir talep seçin.";
                return RedirectToAction("Index", "Admin");
            }

            var selectedDeliveries = await _context.Deliveries
                                            .Where(d => TeslimatIdleri.Contains(d.Id))
                                            .ToListAsync();

            var startPoint = new RoutePoint
            {
                Id = 0,
                Name = "MERKEZ",
                Lat = 37.2154,
                Lng = 28.3636,
                Order = 1
            };

            var pointsToSort = new List<RoutePoint> { startPoint };
            foreach (var d in selectedDeliveries)
            {
                pointsToSort.Add(new RoutePoint
                {
                    Id = d.Id,
                    Lat = (double)d.Enlem,
                    Lng = (double)d.Boylam
                });
            }

            var optimizedRoutePoints = OptimizeRoute(pointsToSort);

            string mapsUrl = "http://googleusercontent.com/maps.google.com/dir";
            foreach (var point in optimizedRoutePoints)
            {
                string latStr = point.Lat.ToString(CultureInfo.InvariantCulture);
                string lngStr = point.Lng.ToString(CultureInfo.InvariantCulture);
                mapsUrl += $"/{latStr},{lngStr}";
            }

            var newRoute = new AtikDonusum.Models.CollectionRoute
            {
                RotaAdi = RotaAdi,
                AracId = AracId,
                OlusturulmaTarihi = DateTime.Now,
                PlanlamaTarihi = DateTime.Now,
                Durum = "Aktif",
                GoogleMapsUrl = mapsUrl,
                Bolge = "Genel",
                ToplanacakNoktalarJson = "[]"
            };

            _context.CollectionRoutes.Add(newRoute);
            await _context.SaveChangesAsync();

            foreach (var delivery in selectedDeliveries)
            {
                delivery.CollectionRouteId = newRoute.Id;
                delivery.Durum = "Rota Planlandı";
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rota başarıyla oluşturuldu!";
            return RedirectToAction("Index", "Admin");
        }

        private List<RoutePoint> OptimizeRoute(List<RoutePoint> allPoints)
        {
            var route = new List<RoutePoint>();
            var remaining = new List<RoutePoint>(allPoints);
            var current = remaining.First(p => p.Id == 0);
            route.Add(current);
            remaining.Remove(current);

            while (remaining.Count > 0)
            {
                RoutePoint nearest = null;
                double minDistance = double.MaxValue;
                foreach (var target in remaining)
                {
                    double dist = GetDistance(current.Lat, current.Lng, target.Lat, target.Lng);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = target;
                    }
                }
                if (nearest != null)
                {
                    route.Add(nearest);
                    remaining.Remove(nearest);
                    current = nearest;
                }
            }
            return route;
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle) { return Math.PI * angle / 180.0; }

        public class RoutePoint
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty; // EKLENDİ
            public double Lat { get; set; }
            public double Lng { get; set; }
            public int Order { get; set; }
        }
    }
}