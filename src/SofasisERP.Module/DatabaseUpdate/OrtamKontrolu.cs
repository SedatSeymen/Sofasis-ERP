/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : OrtamKontrolu.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : 4-ajanlı testte (2026-08-22, yazılım mühendisi bulgusu) tespit
 *                    edildi: DatabaseSeeder/Updater, ortam ayrımı olmadan her
 *                    --updateDatabase çalışmasında boş şifreli Admin kullanıcısı ve
 *                    rastgele finansal test verisi üretiyordu — production'a kazayla
 *                    sızma riski. SofasisERP.Module projesi ASP.NET Core hosting
 *                    paketlerine bağımlı DEĞİL (yalnızca Blazor.Server'da mevcut) —
 *                    bu yüzden IHostEnvironment enjekte etmek yerine, ASP.NET Core'un
 *                    IsDevelopment() içeride zaten okuduğu ASPNETCORE_ENVIRONMENT
 *                    değişkeni doğrudan okunur (işlevsel olarak birebir eşdeğer).
 * ****************************************************************************
 */

using System;

namespace SofasisERP.Module.DatabaseUpdate;

static class OrtamKontrolu
{
    public static bool GelistirmeOrtamiMi() =>
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
}
