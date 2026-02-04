using System.ComponentModel.DataAnnotations;

namespace AtikDonusum.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }
        public string? Plaka { get; set; }

        // YENİ EKLENDİ: Araç Modeli (Ford, Fiat vb.)
        public string? Model { get; set; }

        public string? SurucuIsmi { get; set; }
        public decimal KapasiteKG { get; set; }
        public string? Durum { get; set; }
    }
}