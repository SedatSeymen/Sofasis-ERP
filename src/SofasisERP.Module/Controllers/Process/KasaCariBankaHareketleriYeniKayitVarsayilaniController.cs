/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaCariBankaHareketleriYeniKayitVarsayilaniController.cs
 * Oluşturma Tarihi : 08/20/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kasa/Banka/Cari'nin her biri için hareket tipi bazlı ayrı
 *                    ekranlarından (Açılış/Tahsilat/Ödeme/Virman) "Yeni" ile
 *                    açılan fişe View.Id'ye göre FisTuruTanim'i otomatik atar —
 *                    ortak iskelet artık YeniKayitFisTuruAtayiciControllerBase'de
 *                    (bkz. o dosya).
 * ****************************************************************************
 */

using SofasisERP.Module.BusinessObjects;
using System.Collections.Generic;

namespace SofasisERP.Module.Controllers.Process;

public class KasaCariBankaHareketleriYeniKayitVarsayilaniController : YeniKayitFisTuruAtayiciControllerBase<KasaCariBankaHareketleri>
{
    static readonly Dictionary<string, string> ViewIdFisTuruKodulari = new()
    {
        ["KasaCariBankaHareketleri_KasaAcilisDetailView"] = "ACILIS",
        ["KasaCariBankaHareketleri_KasaTahsilatDetailView"] = "TAHSIL",
        ["KasaCariBankaHareketleri_KasaOdemeDetailView"] = "ODEME",
        ["KasaCariBankaHareketleri_KasaVirmanDetailView"] = "VIRMAN",
        ["KasaCariBankaHareketleri_BankaAcilisDetailView"] = "ACILIS",
        ["KasaCariBankaHareketleri_BankaTahsilatDetailView"] = "TAHSIL",
        ["KasaCariBankaHareketleri_BankaOdemeDetailView"] = "ODEME",
        ["KasaCariBankaHareketleri_BankaVirmanDetailView"] = "VIRMAN",
        ["KasaCariBankaHareketleri_CariAcilisDetailView"] = "ACILIS",
        ["KasaCariBankaHareketleri_CariTahsilatDetailView"] = "TAHSIL",
        ["KasaCariBankaHareketleri_CariOdemeDetailView"] = "ODEME",
        ["KasaCariBankaHareketleri_CariVirmanDetailView"] = "VIRMAN",
    };

    public KasaCariBankaHareketleriYeniKayitVarsayilaniController() : base(ViewIdFisTuruKodulari) { }

    protected override void VarsayilanlariUygula(KasaCariBankaHareketleri entity, FisTuruTanim fisTuru, string viewId)
        => entity.FisTuruTanim = fisTuru;
}
