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

public enum StokKoduUretimYontemi
{
    [XafDisplayName("Otomatik (Fiş Türü-Tarih-Sıra No)")]
    Otomatik,

    [XafDisplayName("Kod Jeneratörü (Tip.Grup.AltGrup.SıraNo)")]
    Jenerator
}
