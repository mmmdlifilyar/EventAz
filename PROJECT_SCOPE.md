# EventAz kurs layihəsi — əhatə dairəsi

ASP.NET Core Web API backend-i Clean Architecture qatlarında hazırlanır: Domain, Application, Infrastructure/Persistence və WebAPI. Application qatında CQRS/MediatR, Identity və JWT autentifikasiyası, EF Core/MSSQL persistence istifadə olunur.

## Tələb olunan imkanlar

- Qeydiyyat, giriş, JWT və Admin/User əsaslı avtorizasiya
- Tədbir yaratmaq, yayımlamaq, siyahılamaq, ətraflı baxmaq, filtr və pagination
- VIP/Standard kimi çoxsəviyyəli biletlər və bilet alışı
- QR kodlu bilet
- Favorit tədbirlər
- Promo kodlar
- Tədbir review və rating
- Admin statistikası
- Email bildirişləri

## Hazırkı vəziyyət

- [x] Auth və kateqoriya üçün ilkin CQRS/API
- [x] Tədbir yaratmaq və ticket tier-ləri əlavə etmək; başlanğıc statusu Draft-dır
- [x] Admin üçün tədbiri Published etmək
- [x] Published tədbirlər üçün search, category/date filter və pagination
- [x] Published tədbirin detail məlumatı
- [ ] Auth/role axınını Admin/User layihə tələbinə uyğun yekunlaşdırmaq
- [x] Bilet alışı, atomic satış limiti, unikal QR payload-u, PNG QR və yoxlama endpoint-i
- [x] Favoritlər: əlavə et, sil və öz siyahısına bax
- [x] Promo kodlar: Admin yaratma, siyahılama, deaktiv etmə və alışda endirim/limit yoxlaması
- [x] Review/rating: verified ticket required, one review per user and event, public review list
- [x] Admin statistikası: istifadəçilər, tədbirlər, satış gəliri və ən çox satılan tədbirlər
- [x] Email bildirişləri: uğurlu bilet alışından sonra konfiqurasiya olunmuş SMTP ilə təsdiq email-i
- [x] API xətalarının vahid Problem Details cavab formatı
- [x] README-də quraşdırma, konfiqurasiya və endpoint sənədləşməsi

## Hazırkı API axını

1. Admin kateqoriya yaradır: `POST /api/categories`.
2. Organizer/Admin tədbir və tier-ləri yaradır: `POST /api/events`.
3. Admin tədbiri yayımlayır: `POST /api/events/{id}/publish`.
4. İstifadəçilər tədbirləri oxuyur: `GET /api/events` və `GET /api/events/{id}`.

Tədbir siyahısı `page`, `pageSize`, `search`, `categoryId`, `fromDate` və `toDate` query parametrlərini qəbul edir.
