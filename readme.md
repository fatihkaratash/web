# FitMind AI - Spor Salonu Yönetim Sistemi

![.NET](https://img.shields.io/badge/.NET-7.0-512BD4?style=flat&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-336791?style=flat&logo=postgresql)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap)
![License](https://img.shields.io/badge/license-MIT-green)

**FitMind AI**, ASP.NET Core MVC ile geliştirilmiş, AI destekli akıllı spor salonu yönetim ve randevu sistemidir. Sakarya Üniversitesi Web Programlama dersi kapsamında geliştirilmiştir.

## 🎯 Özellikler

### ✅ Temel Özellikler
- 🏋️ **Multi-Gym Desteği**: Birden fazla spor salonu yönetimi
- 👤 **Rol Tabanlı Yetkilendirme**: Admin ve Member rolleri
- 📅 **Akıllı Randevu Sistemi**: Çakışma kontrolü ve onay mekanizması
- 🤖 **AI Egzersiz Önerileri**: OpenAI entegrasyonu ile kişiselleştirilmiş programlar
- 📊 **Admin Dashboard**: İstatistikler ve raporlama
- 🔒 **Güvenli Authentication**: ASP.NET Core Identity

### 🚀 Teknik Özellikler
- RESTful API endpoint'leri
- LINQ sorguları ile veri yönetimi
- Service Layer pattern
- ViewModel kullanımı
- Client & Server-side validation
- Responsive Bootstrap 5 arayüzü

## 🛠️ Teknolojiler

- **Backend**: ASP.NET Core 7.0 MVC
- **Database**: PostgreSQL 14+
- **ORM**: Entity Framework Core 7.0
- **Authentication**: ASP.NET Core Identity
- **UI Framework**: Bootstrap 5.3
- **AI Integration**: OpenAI API (GPT-3.5-turbo)
- **Version Control**: Git & GitHub

## 📋 Gereksinimler

- .NET 7.0 SDK veya üzeri
- PostgreSQL 14+ (lokal kurulum)
- Visual Studio 2022 / VS Code / Rider
- Git

## 🚀 Kurulum

### 1. Repository'yi Klonlayın
```bash
git clone https://github.com/fatihkaratash/web.git
cd web/FitMindAI
```

### 2. PostgreSQL Veritabanını Hazırlayın
```bash
# PostgreSQL'de database oluşturun (opsiyonel, EF otomatik oluşturur)
createdb fitminddb
```

### 3. Connection String'i Ayarlayın
`appsettings.Development.json` dosyasını düzenleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=fitminddb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 4. Migration'ları Uygulayın
```bash
dotnet ef database update
```

### 5. Uygulamayı Çalıştırın
```bash
dotnet run
```

Tarayıcınızda `https://localhost:5001` adresine gidin.

## 👤 Varsayılan Kullanıcılar

### Admin Hesabı
- **Email**: `ogrencinumarasi@sakarya.edu.tr`
- **Şifre**: `sau`

### Member Hesabı
Kayıt ol sayfasından yeni üye oluşturabilirsiniz.

## 📁 Proje Yapısı

```
FitMindAI/
├── Areas/Admin/          # Admin paneli
├── Controllers/          # MVC Controllers
├── Data/                # DbContext ve Initializer
├── Models/              # Domain entities
├── Services/            # Business logic
├── ViewModels/          # View-specific models
├── Views/               # Razor views
├── wwwroot/             # Static files
└── Migrations/          # EF Core migrations
```

## 📊 Veritabanı Şeması

### Core Tables
- **Gym**: Spor salonu bilgileri
- **ServiceType**: Hizmet türleri (Fitness, Yoga, Pilates)
- **Trainer**: Antrenör bilgileri
- **Member**: Üye profilleri
- **TrainerService**: Antrenör-Hizmet ilişkisi (M:M)
- **TrainerAvailability**: Antrenör müsaitlik saatleri
- **Appointment**: Randevular
- **AiRecommendation**: AI öneri logları

## 🎓 Kullanım Senaryoları

### Admin İşlemleri
1. Spor salonları ekleyin/düzenleyin
2. Hizmet türlerini tanımlayın (Fitness, Pilates, Yoga)
3. Antrenörleri ekleyin ve salona atayın
4. Antrenörlerin müsaitlik saatlerini belirleyin
5. Randevuları onaylayın/reddedin
6. Dashboard'dan istatistikleri görüntüleyin

### Üye İşlemleri
1. Kayıt olun ve giriş yapın
2. Spor salonu ve antrenör seçin
3. Müsait saatleri görüntüleyin
4. Randevu oluşturun
5. Kendi randevularınızı görüntüleyin
6. AI'dan egzersiz önerisi alın

## 🔌 API Endpoints

```
GET  /api/trainers                    # Tüm aktif antrenörler
GET  /api/trainers/available          # Müsait antrenörler
GET  /api/members/{id}/appointments   # Üye randevuları
```

## 🤝 Katkıda Bulunma

Bu proje eğitim amaçlıdır. Önerileriniz için issue açabilirsiniz.

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 👨‍💻 Geliştirici

**Fatih Karataş**  
Sakarya Üniversitesi - Web Programlama Dersi Projesi

- GitHub: [@fatihkaratash](https://github.com/fatihkaratash)
- Proje Repo: [github.com/fatihkaratash/web](https://github.com/fatihkaratash/web)

## 📚 Dokümantasyon

- [Development Plan](../DEVELOPMENT_PLAN.md) - Detaylı geliştirme planı
- [Enhancements](../ENHANCEMENTS.md) - Bonus özellikler ve geliştirme fikirleri
- [Project Rules](../PROJECT_RULES.md) - Proje kuralları ve best practices

---

**Not**: Bu proje aktif geliştirme aşamasındadır. Ekran görüntüleri ve ek özellikler eklenecektir.

**Son Güncelleme**: Kasım 2025
