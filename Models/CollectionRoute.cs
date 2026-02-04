using System;
using System.ComponentModel.DataAnnotations;

namespace AtikDonusum.Models
{
    public class CollectionRoute
    {
        [Key]
        public int Id { get; set; }
        public string? RotaAdi { get; set; }
        public int AracId { get; set; }
        public string? Bolge { get; set; }
        public string? ToplanacakNoktalarJson { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public DateTime PlanlamaTarihi { get; set; } = DateTime.Now;
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        public string? Durum { get; set; }
    }
}