using System;
using System.ComponentModel.DataAnnotations;

namespace AtikDonusum.Models
{
    public class WastePoint
    {
        [Key]
        public int Id { get; set; }
        public string? Ad { get; set; }
        public string? Adres { get; set; }
        public string? KabulEdilenAtikTipleri { get; set; }

        // Hatalar: Enlem, Boylam, AktifMi eksikti
        public double Enlem { get; set; }
        public double Boylam { get; set; }
        public bool AktifMi { get; set; } = true;
    }
}