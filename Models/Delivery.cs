using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtikDonusum.Models
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        public string? KullaniciId { get; set; }

        [ForeignKey("KullaniciId")]
        public virtual ApplicationUser? Kullanici { get; set; }

        public string? AtikTuru { get; set; }

        // Bu decimal kalabilir (KG hesabı)
        public decimal MiktarKG { get; set; }

        public string? LokasyonBilgisi { get; set; }

        // HATA BURADAYDI: Bunları kesinlikle double yapıyoruz.
        // Eğer veritabanında decimal ise, Controller'da (double) diye cast edeceğiz.
        public double Enlem { get; set; }
        public double Boylam { get; set; }

        public DateTime TeslimatTarihi { get; set; } = DateTime.Now;

        public string? Not { get; set; }

        public string Durum { get; set; } = "Beklemede";

        public int? CollectionRouteId { get; set; }
    }
}