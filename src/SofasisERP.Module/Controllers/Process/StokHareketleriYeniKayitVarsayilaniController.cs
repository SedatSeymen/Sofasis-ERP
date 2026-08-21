/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHareketleriYeniKayitVarsayilaniController.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok Hareketleri'nin Fiş Türü bazlı ayrı ekranlarından
 *                    (Açılış/Alış Girişi/Satış Çıkışı/Sayım Fazlası/Sayım
 *                    Eksiği/Fire) "Yeni" ile açılan fişe View.Id'ye göre
 *                    FisTuruTanim'i otomatik atar — CariHesapHareketleriYeni-
 *                    KayitVarsayilaniController ile aynı kanıtlanmış desen.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Controllers.Process;

public class StokHareketleriYeniKayitVarsayilaniController : ViewController<DetailView>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is not StokHareketleriM entity) return;
        if (!View.ObjectSpace.IsNewObject(entity)) return;

        string fisTuruKodu = View.Id switch
        {
            "StokHareketleriM_AcilisDetailView" => "STACLS",
            "StokHareketleriM_AlisGirisiDetailView" => "STALIS",
            "StokHareketleriM_MalKabulGirisiDetailView" => "STSAGR",
            "StokHareketleriM_UretimGirisiDetailView" => "STURET",
            "StokHareketleriM_SayimFazlasiDetailView" => "STSFAZ",
            "StokHareketleriM_SatisCikisiDetailView" => "STSTIS",
            "StokHareketleriM_UretimTuketimiDetailView" => "STURTK",
            "StokHareketleriM_SayimEksigiDetailView" => "STSEKS",
            "StokHareketleriM_FireDetailView" => "STFIRE",
            "StokHareketleriM_SatinAlmaIadeCikisiDetailView" => "STSAIC",
            "StokHareketleriM_SatisIadeGirisiDetailView" => "STSTIG",
            _ => null
        };
        if (fisTuruKodu == null) return;

        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == fisTuruKodu));
    }
}
