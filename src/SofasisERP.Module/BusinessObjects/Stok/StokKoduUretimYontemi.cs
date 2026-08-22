/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokKoduUretimYontemi.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : StokParametre'de seçilen StokKodu üretim yöntemi.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

// 4-ajanlı testte (2026-08-22, kıdemli muhasebeci bulgusu) tespit edildi: eski
// caption'lar ("Otomatik (Fiş Türü-Tarih-Sıra No)" / "Kod Jeneratörü (Tip.Grup.
// AltGrup.SıraNo)") teknik jargon içeriyordu — muhasebe bilgisi olmayan kullanıcı
// için anlaşılır değildi (bkz. [[feedback_muhasebe_bilgisi_olmayan_kullanici]]).
// İş diline çevrildi, teknik format parantez içinde ikinci planda bırakıldı.
public enum StokKoduUretimYontemi
{
    [XafDisplayName("Basit Sıra Numarası (ör. STOKTN-260822001)")]
    Otomatik,

    [XafDisplayName("Kategoriye Göre Kod (ör. 150.01.01.0001)")]
    Jenerator
}
