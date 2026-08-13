using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

// ADR-006: karşılaştırma mantığı business object'te veya controller'da değil, servis katmanında
// yaşar. Karsilastir(), bir Talebe gelen TÜM tekliflerin satırlarını aynı Talep kalemine göre
// gruplayıp en düşük birim fiyatlı satır(lar)ı EnDusukFiyatMi=true olarak işaretler — sonuç,
// karşılaştırma ListView'indeki [Appearance] rengiyle görünür hale gelir.
public interface ISatinAlmaTeklifServisi
{
    void Karsilastir(SatinAlmaTalebiM talep);

    // Onaylı (veya zaten teklife çıkılmış — ikinci/üçüncü rakip teklif eklenirken) bir Talep'ten
    // bir Teklif TASLAĞI oluşturur — KAYDETMEZ. Kalemler SatinAlmaTeklifM.KaynakTalep setter'ı
    // tarafından otomatik ön-doldurulur; dönen nesne kullanıcının Tedarikçi seçip fiyat girmesi
    // için bir DetailView'da açılmak üzeredir (bkz. SatinAlmaTalebiTeklifOlusturController).
    // NOT: Talep'ten DOĞRUDAN Sipariş oluşturan bir kısayol KASITLI OLARAK yok — standart satın
    // alma akışı (RFQ/teklif toplama best practice) Teklif adımının atlanmasına izin vermez.
    SatinAlmaTeklifM TalepdenTeklifTaslagiOlustur(SatinAlmaTalebiM talep);
}
