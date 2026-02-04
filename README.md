
# Yapay Zeka Destekli Akıllı Dijital Atık Toplama Sistemi

Bu proje, kentsel atık yönetimini optimize etmek amacıyla geliştirilmiş, yapay zeka destekli bir web platformudur. Sistem, ASP.NET Core (MVC) mimarisi ve Python (Flask) tabanlı tahminleme algoritmaları kullanılarak geliştirilmiştir.

## 🚀 Özellikler

- **Kullanıcı Paneli:** Atık türü ve miktar girişi, Google Maps entegrasyonu ile konum bildirme.
- **Yönetici Paneli:** Anlık talep takibi, rota optimizasyonu ve istatistiksel raporlar.
- **Yapay Zeka Modülü:** Linear Regression algoritması ile geçmiş verilere dayalı gelecek atık miktarı tahmini.
- **Veritabanı:** MS SQL Server üzerinde ilişkisel veri tabanı yapısı.

## 🛠 Kullanılan Teknolojiler

- **Backend:** C# / ASP.NET Core 6.0
- **AI Service:** Python / Flask / Scikit-Learn / Pandas
- **Frontend:** HTML5, CSS3, Bootstrap, JavaScript
- **Database:** Microsoft SQL Server

## ⚙️ Kurulum

1. **WebUI:** `WebUI` klasöründeki çözüm dosyasını (.sln) Visual Studio ile açıp çalıştırın.
2. **Yapay Zeka Servisi:** `AI_Service` klasöründeki `app.py` dosyasını terminalden `python app.py` komutu ile başlatın.
