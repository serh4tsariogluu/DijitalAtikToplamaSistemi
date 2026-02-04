using Microsoft.AspNetCore.Identity;
using System;

namespace AtikDonusum.Models
{
    public class ApplicationUser : IdentityUser
    {
        // "Null atanamaz" hataları için string? yapıldı
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Name { get; set; } // Eğer ayrıca Name tutuyorsan
        public string? Type { get; set; }
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}