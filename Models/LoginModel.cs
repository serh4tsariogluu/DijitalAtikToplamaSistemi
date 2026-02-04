using System.ComponentModel.DataAnnotations;

namespace AtikDonusum.Models
{
    // Bu sınıf, Login.cshtml view'i tarafından kullanılan form verilerini temsil eder.
    public class LoginModel
    {
        [Required(ErrorMessage = "Kullanıcı adı alanı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
    }
}
