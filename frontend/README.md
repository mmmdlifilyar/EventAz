# EventAz React interfeysi

EventAz WebAPI-dən ayrıca işləyən React + TypeScript interfeysi.

## İşə salmaq

1. EventAz WebAPI-ni `https://localhost:7012` ünvanında başladın.
2. `frontend` qovluğunda asılılıqları quraşdırın və Vite-i başladın:

   ```bash
   pnpm.cmd install
   pnpm.cmd dev
   ```

3. Brauzerdə `http://localhost:5173` səhifəsini açın.

Vite `/api` sorğularını lokal API-yə ötürür. Başqa API ünvanı işlədilirsə, `.env.example` faylını `.env` adı ilə köçürüb `VITE_API_BASE_URL` dəyərini yazın. Bu halda API həmin frontend ünvanına CORS icazəsi verməlidir.

## Hazır funksiyalar

- Tədbirləri axtarmaq, kateqoriya, məkan, tarix və qiymətə görə süzmək
- Nəticələri tarixə və bilet qiymətinə görə sıralamaq və səhifələmək
- Tədbir detallarına baxmaq
- Hesab yaratmaq və JWT ilə daxil olmaq
- Tədbiri favoritlərə əlavə etmək və favorit siyahısını görmək
- Promo kodla bilet almaq və QR biletini yükləmək
- Alınmış biletləri görmək və QR kodlarını yenidən yükləmək
- Tədbirlərə rəy oxumaq və yazmaq
- Təşkilatçı panelindən tədbir yaratmaq və bileti yoxlamaq
- Admin panelindən tədbir yayımlamaq, promo kod və kateqoriya idarə etmək, statistikanı görmək

