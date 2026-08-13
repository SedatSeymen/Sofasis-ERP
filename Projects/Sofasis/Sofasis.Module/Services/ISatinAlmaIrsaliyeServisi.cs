using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

public interface ISatinAlmaIrsaliyeServisi
{
    // Sipariş'in kalan (henüz irsaliye edilmemiş) satırlarından, HENÜZ KAYDEDİLMEMİŞ bir İrsaliye
    // taslağı (+ bire-bir bağlı StokHareketleriM/D taslağı) oluşturur. ADR-006 gereği tüm iş mantığı
    // burada; controller yalnızca popup'ı açar.
    IrsaliyeM IrsaliyeTaslagiOlustur(IObjectSpace objectSpace, SatinAlmaSiparisiM siparis);

    // İrsaliye gerçekten kaydedilirken (Committing) çağrılır: Sipariş satırlarının
    // TeslimEdilenMiktar'ını artırır, Sipariş Durum'unu günceller.
    void TeslimatiIsle(IrsaliyeM irsaliye);
}
