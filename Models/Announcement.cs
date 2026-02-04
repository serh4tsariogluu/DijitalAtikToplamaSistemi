using System;
using System.ComponentModel.DataAnnotations;

namespace AtikDonusum.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Icerik { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}