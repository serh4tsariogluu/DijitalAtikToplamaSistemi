using System.ComponentModel.DataAnnotations;

namespace AtikDonusum.Models
{
    /// <summary>
    /// /Auth/Register sayfasındaki kayıt formunu temsil eden View Model.
    /// </summary>
    public class RegisterModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Adınız")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyadınız")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi girdiniz.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)] // Formda karakterlerin gizlenmesi için
        [Display(Name = "Şifre")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalı.", MinimumLength = 6)]
        public string Sifre { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre (Tekrar)")]
        [Compare("Sifre", ErrorMessage = "Şifreler uyuşmuyor. Lütfen kontrol edin.")]
        public string SifreTekrar { get; set; }
    }
}

