using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

// ADR-006: onay iş mantığı business object (SatinAlmaTalebiM) veya controller'da değil, servis
// katmanında yaşar — SatinAlmaTalebiM.Durum'u YALNIZCA bu servis değiştirir.
public interface ISatinAlmaOnayServisi
{
    void Gonder(SatinAlmaTalebiM talep);                                          // Taslak → OnayBekliyor
    void Onayla(SatinAlmaTalebiM talep, ApplicationUser onaylayan);               // OnayBekliyor → Onaylandi
    void Reddet(SatinAlmaTalebiM talep, ApplicationUser reddeden, string neden);  // OnayBekliyor → Reddedildi
    void TaslagaDondur(SatinAlmaTalebiM talep);                                   // Reddedildi → Taslak (düzeltip yeniden gönderebilmek için)
}
