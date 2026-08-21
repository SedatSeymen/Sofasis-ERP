/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaCariBankaHareketleriYeniKayitVarsayilaniController.cs
 * Oluşturma Tarihi : 08/20/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/20/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kasa/Banka/Cari'nin her biri için hareket tipi bazlı ayrı
 *                    ekranlarından (Açılış/Tahsilat/Ödeme/Virman) "Yeni" ile
 *                    açılan fişe View.Id'ye göre FisTuruTanim'i otomatik atar —
 *                    StokHareketleriYeniKayitVarsayilaniController ile aynı
 *                    kanıtlanmış desen.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Controllers.Process;

public class KasaCariBankaHareketleriYeniKayitVarsayilaniController : ViewController<DetailView>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is not KasaCariBankaHareketleri entity) return;
        if (!View.ObjectSpace.IsNewObject(entity)) return;

        string fisTuruKodu = View.Id switch
        {
            "KasaCariBankaHareketleri_KasaAcilisDetailView" => "ACILIS",
            "KasaCariBankaHareketleri_KasaTahsilatDetailView" => "TAHSIL",
            "KasaCariBankaHareketleri_KasaOdemeDetailView" => "ODEME",
            "KasaCariBankaHareketleri_KasaVirmanDetailView" => "VIRMAN",
            "KasaCariBankaHareketleri_BankaAcilisDetailView" => "ACILIS",
            "KasaCariBankaHareketleri_BankaTahsilatDetailView" => "TAHSIL",
            "KasaCariBankaHareketleri_BankaOdemeDetailView" => "ODEME",
            "KasaCariBankaHareketleri_BankaVirmanDetailView" => "VIRMAN",
            "KasaCariBankaHareketleri_CariAcilisDetailView" => "ACILIS",
            "KasaCariBankaHareketleri_CariTahsilatDetailView" => "TAHSIL",
            "KasaCariBankaHareketleri_CariOdemeDetailView" => "ODEME",
            "KasaCariBankaHareketleri_CariVirmanDetailView" => "VIRMAN",
            _ => null
        };
        if (fisTuruKodu == null) return;

        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == fisTuruKodu));
    }
}
