/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : OdemeSekli.cs
 * Oluşturma Tarihi : 08/20/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/20/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : KasaCariBankaHareketleri.OdemeSekli için. 4-ajanlı testte
 *                    (2026-08-21) Havale/EFT'nin eksik olduğu bulundu, eklendi.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum OdemeSekli
{
    [XafDisplayName("Nakit")]
    Nakit,

    [XafDisplayName("Kredi Kartı")]
    KrediKarti,

    [XafDisplayName("Havale/EFT")]
    HavaleEft
}
