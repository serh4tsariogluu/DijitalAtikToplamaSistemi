using AtikDonusum.Data;
using AtikDonusum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AtikDonusum.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // --- 1. Adım: Rolleri Oluştur ---
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Kullanici"))
            {
                await roleManager.CreateAsync(new IdentityRole("Kullanici"));
            }

            // --- 2. Adım: Varsayılan Admin Kullanıcısını Oluştur ---
            string adminEmail = "admin@proje.com";
            string adminKullaniciAdi = "admin";
            string adminSifre = "Admin123*";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminKullaniciAdi,
                    Email = adminEmail,
                    Ad = "Admin",
                    Soyad = "Proje",
                    EmailConfirmed = true,
                    // DÜZELTME: Hata almamak için zorunlu alanlara boş değer atadık
                    Address1 = "-",
                    Address2 = "-",
                    Name = "System Admin",
                    Type = "Admin"
                };
                var result = await userManager.CreateAsync(adminUser, adminSifre);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // --- 3. Adım: Test Duyurularını Ekle ---
            if (!await context.Announcements.AnyAsync())
            {
                await context.Announcements.AddRangeAsync(
                    new Announcement
                    {
                        Baslik = "Mobil Uygulama Yayında!",
                        Icerik = "Artık atık teslimlerini mobil uygulamamızla takip edebilirsiniz.",
                        Tarih = DateTime.Now.AddDays(-5)
                    },
                    new Announcement
                    {
                        Baslik = "Yeni Rota Eklendi",
                        Icerik = "Yeni Fethiye bölgesi geri dönüşüm rotası aktif edilmiştir.",
                        Tarih = DateTime.Now.AddDays(-1)
                    }
                );
            }

            // --- 4. Adım: Test Atık Noktalarını Ekle ---
            if (!await context.WastePoints.AnyAsync())
            {
                await context.WastePoints.AddRangeAsync(
                    new WastePoint
                    {
                        Ad = "Menteşe Belediyesi Atık Getirme Merkezi",
                        Adres = "Karamehmet Mah. 123. Sokak No: 5, Menteşe/Muğla",
                        // DÜZELTME: 'm' harfi silindi. Artık double.
                        Enlem = 37.2154,
                        Boylam = 28.3636,
                        KabulEdilenAtikTipleri = "Plastik, Kağıt, Cam, Metal",
                        AktifMi = true
                    },
                    new WastePoint
                    {
                        Ad = "Fethiye Elektronik Atık Toplama",
                        Adres = "Cumhuriyet Mah. Çarşı Cad. No: 102, Fethiye/Muğla",
                        // DÜZELTME: 'm' harfi silindi. Artık double.
                        Enlem = 36.6574,
                        Boylam = 29.1156,
                        KabulEdilenAtikTipleri = "Elektronik Atık, Pil",
                        AktifMi = true
                    }
                );
            }

            // --- 5. Adım: Değişiklikleri Kaydet ---
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}