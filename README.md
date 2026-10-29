# EventAz — Tədbir və bilet platforması

ASP.NET Core Web API kurs layihəsi. Layihədə Clean Architecture yanaşması, CQRS/MediatR, ASP.NET Core Identity, JWT və Entity Framework Core istifadə olunur.

## Layihənin qatları

- `src/Core/EventAz.Domain` — tədbir, bilet, rəy, favorit və promo kod modelləri.
- `src/Core/EventAz.Application` — CQRS sorğuları və əmrləri, validator-lar, DTO-lar və interfeyslər.
- `src/Infrastructure/EventAz.Persistence` — EF Core `DbContext`, SQL Server, migration-lar və repository-lər.
- `src/Infrastructure/EventAz.Infrastructure` — JWT və SMTP email xidmətləri.
- `src/Presentation/EventAz.WebAPI` — controller-lər, autentifikasiya və DI qurulumu.

## Tələblər

- .NET 10 SDK
- SQL Server
- EF Core CLI (`dotnet-ef`)

## Layihəni lokal işə salmaq

1. `src/Presentation/EventAz.WebAPI/appsettings.json` faylındakı `ConnectionStrings:DefaultConnection` dəyərini SQL Server bağlantınıza uyğunlaşdırın.
2. WebAPI layihəsinin User Secrets bölməsində JWT gizli açarını təyin edin:

   ```powershell
   dotnet user-secrets set "JwtSettings:SecretKey" "uzun-tesadufi-gizli-acar" --project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
   ```

3. EF Core CLI quraşdırılmayıbsa, quraşdırın:

   ```powershell
   dotnet tool install --global dotnet-ef
   ```

4. Migration-ları verilənlər bazasına tətbiq edin:

   ```powershell
   dotnet ef database update --project src/Infrastructure/EventAz.Persistence/EventAz.Persistence.csproj --startup-project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
   ```

5. API-ni başladın:

   ```powershell
   dotnet run --project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
   ```

İnkişaf mühitində Swagger UI açılır. Solution-u build etmək üçün:

```powershell
dotnet build EventAz.sln
```

## SMTP qurulumu

`appsettings.json` faylında `EmailSettings` üçün Host, Port, UserName, FromEmail, FromName və UseSsl sahələri var. Şifrəni config faylına əlavə etməyin; User Secrets-də saxlayın:

```powershell
dotnet user-secrets set "EmailSettings:Host" "smtp.example.com" --project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
dotnet user-secrets set "EmailSettings:UserName" "smtp-user" --project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
dotnet user-secrets set "EmailSettings:Password" "smtp-password" --project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
dotnet user-secrets set "EmailSettings:FromEmail" "tickets@example.com" --project src/Presentation/EventAz.WebAPI/EventAz.WebAPI.csproj
```

Mühit dəyişənlərindən də istifadə edə bilərsiniz. `UseSsl=false` olduqda SMTP STARTTLS, `UseSsl=true` olduqda SSL bağlantısı işlədilir. SMTP qurulmayıbsa və ya email göndərilməsi alınmırsa, bilet alışı ləğv olunmur və səbəb tətbiqin jurnalında qeyd edilir.

## Əsas endpoint-lər

| Əməliyyat | Endpoint | İcazə |
| --- | --- | --- |
| Qeydiyyatdan keçmək | `POST /api/auth/register` | Hamı üçün açıq |
| Daxil olmaq | `POST /api/auth/login` | Hamı üçün açıq |
| Yayımlanmış tədbirlərin siyahısı | `GET /api/events` | Hamı üçün açıq |
| Tədbir detalları | `GET /api/events/{id}` | Hamı üçün açıq |
| Tədbir yaratmaq | `POST /api/events` | Admin, Organizer |
| Tədbiri yayımlamaq | `POST /api/events/{id}/publish` | Admin |
| Bilet almaq | `POST /api/tickets` | Daxil olmuş istifadəçi |
| Öz biletinin QR şəklini almaq | `GET /api/tickets/{ticketId}/qr` | Bilet sahibi |
| Bileti yoxlamaq və istifadə edilmiş kimi qeyd etmək | `POST /api/tickets/validate` | Admin və ya tədbirin Organizer-i |
| Öz favoritlərini görmək | `GET /api/favorites` | Daxil olmuş istifadəçi |
| Favoritə əlavə etmək və silmək | `PUT/DELETE /api/favorites/{eventId}` | Daxil olmuş istifadəçi |
| Promo kod yaratmaq | `POST /api/promo-codes` | Admin |
| Tədbirin promo kodlarını görmək | `GET /api/promo-codes/by-event/{eventId}` | Admin |
| Promo kodu deaktiv etmək | `DELETE /api/promo-codes/{id}` | Admin |
| Tədbir rəylərini görmək | `GET /api/reviews/event/{eventId}` | Hamı üçün açıq |
| Rəy yazmaq | `POST /api/reviews` | Daxil olmuş, bilet almış istifadəçi |
| Admin statistikası | `GET /api/admin/statistics` | Admin |

## Swagger-dan tədbir yaratmaq

1. `POST /api/auth/register` vasitəsilə `Role` dəyəri `Organizer` olan hesab yaradın. Mövcud hesabınız varsa, `POST /api/auth/login` ilə token alın.
2. Cavabdakı tokeni Swagger səhifəsinin yuxarısındakı **Authorize** düyməsinə daxil edin. `Bearer` sxemi tokeni sorğulara avtomatik əlavə edir.
3. `GET /api/categories` ilə mövcud kateqoriyanı tapın və onun həqiqi `id` dəyərini tədbir sorğusunda `categoryId` kimi istifadə edin.
4. `POST /api/events` sorğusunda gələcək `startDate` və ondan sonrakı `endDate` göndərin. Tədbir uğurla yaradılanda `Draft` statusunda olur; ictimai siyahıda görünməsi üçün Admin `POST /api/events/{id}/publish` sorğusu göndərməlidir.

`401 Unauthorized` tokenin olmadığını və ya etibarsız olduğunu bildirir; belə sorğu yadda saxlama mərhələsinə çatmır. Token düzgündür, amma istifadəçinin rolu `Admin` və ya `Organizer` deyilsə, `403 Forbidden` qaytarılır.

## Xəta cavabları

Gözlənilməyən xətalar və CQRS yoxlama xətaları `application/problem+json` formatında qaytarılır. Cavabda HTTP statusu, başlıq, izah və sorğunun `traceId` dəyəri olur. Yoxlama xətalarında əlavə olaraq `errors` sahəsi verilir. Serverin 500 xətalarında daxili xəta məlumatı istifadəçiyə göstərilmir.

## Vacib biznes qaydaları

- Bilet ehtiyatı verilənlər bazasında atomik yenilənir; beləliklə, artıq satışın qarşısı alınır.
- Hər istifadəçi bir tədbirə yalnız bir rəy yaza bilər; unikal indeks bu qaydanı qoruyur.
- İstifadəçi eyni tədbiri favoritlərə yalnız bir dəfə əlavə edə bilər.
- Bilet yalnız tədbir başlayandan sonra və bitməmişdən əvvəl yoxlanıla bilər; əvvəl istifadə edilmiş bilet qəbul edilmir.
- Promo kodun istifadə limiti bilet alışı transaction-ı daxilində yoxlanılır.
