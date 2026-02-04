using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AtikDonusum.Controllers
{
    public class DynamicRouteController : Controller
    {
        // Rota verisi için modelimiz
        public class RouteRequest
        {
            public string Address1 { get; set; } // Senin adresin
            public string Address2 { get; set; } // Benim adresim
        }

        public class Waypoint
        {
            public string Name { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
            public string Type { get; set; } // Depot, Pickup
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate(RouteRequest request)
        {
            // 1. ARAÇ MERKEZİ (Depot) - Burası sabit, araç buradan çıkıyor.
            // Örnek: Muğla Menteşe Belediyesi önü
            var depot = new Waypoint { Name = "Araç Merkezi (Depo)", Lat = 37.2154, Lng = 28.3636, Type = "Depot" };

            // 2. GİRİLEN ADRESLERİ KOORDİNATA ÇEVİRME (Simülasyon)
            // Normalde burada Google Geocoding API kullanılır.
            // Test edebilmen için basit bir "Demo Çevirici" yazdım.
            var point1 = MockGeocoding(request.Address1, 37.2180, 28.3650); // Varsayılan: Biraz kuzey
            var point2 = MockGeocoding(request.Address2, 37.2130, 28.3590); // Varsayılan: Biraz güneybatı

            // 3. ROTA LİSTESİ OLUŞTURMA
            var routePoints = new List<Waypoint>();

            // Sıralama: Depo -> Adres 1 -> Adres 2 -> Depo (Dönüş)
            routePoints.Add(depot);  // Başlangıç
            routePoints.Add(point1); // Senin Adresin
            routePoints.Add(point2); // Benim Adresim
            routePoints.Add(depot);  // Bitiş (Geri Dönüş)

            return View("Result", routePoints);
        }

        // Sahte Adres Çözücü (Gerçek API Key olmadan test etmen için)
        private Waypoint MockGeocoding(string address, double defaultLat, double defaultLng)
        {
            // Sen formda bu isimleri yazarsan özel koordinatlara gider
            if (address.ToLower().Contains("üniversite"))
                return new Waypoint { Name = address, Lat = 37.1697, Lng = 28.3712, Type = "Pickup" };

            if (address.ToLower().Contains("avm"))
                return new Waypoint { Name = address, Lat = 37.2050, Lng = 28.3550, Type = "Pickup" };

            // Bilinmeyen bir adres girersen, haritada görebilmen için varsayılan yakın bir koordinat atar
            return new Waypoint { Name = address, Lat = defaultLat, Lng = defaultLng, Type = "Pickup" };
        }
    }
}