/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TurkiyeZamani.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Sunucu saati (UtcNow) yerine Türkiye yerel saatine göre
 *                    "bugün"ü döndüren tek kaynak. Denetim raporu K4: 21:00-24:00
 *                    (UTC+3) arası girilen kayıtlar DateTime.UtcNow.Date ile bir
 *                    önceki güne tarihleniyordu (fiş numarası, günlük kur tarihi,
 *                    fiş/vade/belge tarihleri). Türkiye 2016'dan beri yaz saati
 *                    uygulamadığından sabit UTC+3'tür; yine de işletim sistemi
 *                    saat dilimi veritabanı (Windows: "Turkey Standard Time",
 *                    Linux: "Europe/Istanbul") öncelikli denenir, ikisi de
 *                    bulunamazsa sabit +3 saate düşülür.
 * ****************************************************************************
 */

namespace SofasisERP.Module.Services;

public static class TurkiyeZamani
{
    static readonly TimeZoneInfo saatDilimi = SaatDilimiBul();

    public static DateTime Simdi => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, saatDilimi);

    public static DateTime Bugun => Simdi.Date;

    static TimeZoneInfo SaatDilimiBul()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.CreateCustomTimeZone("Turkiye-Sabit-UTC+3", TimeSpan.FromHours(3), "Türkiye (UTC+3)", "Türkiye (UTC+3)");
    }
}
