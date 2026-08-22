/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapKoduUretimYontemi.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : CariHesapParametre'de seçilen CariHesapKodu üretim yöntemi —
 *                    StokKoduUretimYontemi ile BİREBİR aynı desen.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

// 4-ajanlı testte (2026-08-22, kıdemli muhasebeci bulgusu) tespit edildi: eski
// caption'lar teknik jargon içeriyordu — bkz. StokKoduUretimYontemi.cs aynı gerekçe.
public enum CariHesapKoduUretimYontemi
{
    [XafDisplayName("Basit Sıra Numarası (ör. CARITN-260822001)")]
    Otomatik,

    [XafDisplayName("Kategoriye Göre Kod (ör. 320.01.01.0001)")]
    Jenerator
}
