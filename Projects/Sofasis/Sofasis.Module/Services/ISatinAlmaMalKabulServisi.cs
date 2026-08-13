using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

public interface ISatinAlmaMalKabulServisi
{
    // Siparişin KalanMiktar>0 olan satırlarının TAMAMI için bir StokHareketleriM (fiş türü
    // STSAGR) taslağı + karşılık gelen StokHareketleriD satırlarını oluşturur — KAYDETMEZ.
    // Kullanıcı dönen taslağı ekranda görüp (Depo seçer, dilerse Miktar'ı azaltır/satır siler —
    // kısmi teslimat) normal "Kaydet" ile commit eder.
    StokHareketleriM MalKabulTaslagiOlustur(DevExpress.ExpressApp.IObjectSpace objectSpace, SatinAlmaSiparisiM siparis);

    // StokHareketleriM (Mal Kabul fişi) KAYDEDİLDİKTEN SONRA çağrılır — KaynakBelgeTipi'i
    // SatinAlmaSiparisiD olan her satır için TeslimEdilenMiktar'ı GERÇEKTEN kaydedilen Miktar
    // kadar artırır (taslakta ne vardı değil, kullanıcının kaydettiği ne ise) ve
    // SatinAlmaSiparisiM.Durum'u günceller (tüm satırlar tamamlandıysa MalKabulYapildi,
    // aksi halde KismiTeslimAlindi).
    void TeslimatiIsle(StokHareketleriM malKabulFisi);
}
