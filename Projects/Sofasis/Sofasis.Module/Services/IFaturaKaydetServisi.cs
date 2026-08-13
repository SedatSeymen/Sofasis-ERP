using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

public interface IFaturaKaydetServisi
{
    // Bir İrsaliyeden (IrsaliyeM, fiş türü IRALIS) henüz faturalanmamış TÜM satırlar için bir
    // FaturaM (fiş türü FAALIS) taslağı + karşılık gelen FaturaD satırlarını oluşturur — KAYDETMEZ.
    // KDV/Tevkifat hesaplaması ve Cari ayna kaydı FaturaD/FaturaM'in KENDİ ObjectSaving()'inde
    // (CariHesapHareketleri/KasaBankaHareketleri ile AYNI, doğrudan-XPO-hook deseninde) gerçekleşir
    // — bkz. o dosyalardaki yorumlar. Bu servis yalnızca taslağı kurar (ADR-006: iş mantığı servis
    // katmanında, ama burada "iş mantığı" tek seferlik kurulum; hesaplama/ayna KENDİ nesnesinin
    // sorumluluğunda — CariHesapHareketleri/KasaBankaHareketleri ile AYNI, kanıtlanmış mimari desen).
    // NOT (2026-08-14): Parametre önceden StokHareketleriM idi — İrsaliye ayrı bir iş nesnesi olarak
    // kurulunca (bkz. docs/CHANGELOG.md) doğrudan İrsaliye'den başlar; Sipariş artık geriye izlemeye
    // gerek kalmadan doğrudan IrsaliyeM.KaynakSiparis'ten okunur.
    FaturaM FaturaTaslagiOlustur(DevExpress.ExpressApp.IObjectSpace objectSpace, IrsaliyeM irsaliye);
}
