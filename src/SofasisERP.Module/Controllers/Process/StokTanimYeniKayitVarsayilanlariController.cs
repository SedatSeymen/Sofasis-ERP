/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokTanimYeniKayitVarsayilanlariController.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok/Hizmet/Masraf Tanımlama ekranlarından "Yeni" ile açılan
 *                    StokTanim kaydına StokHizmetMasrafTipi VE FisTuruTanim'i
 *                    View.Id'ye göre otomatik atar — eski projedeki
 *                    NewRecordDefaultsViewController ile aynı kanıtlanmış desen
 *                    (View.Id bazlı varsayılan atama).
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Controllers.Process;

public class StokTanimYeniKayitVarsayilanlariController : ViewController<DetailView>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is not StokTanim entity) return;
        if (!View.ObjectSpace.IsNewObject(entity)) return;

        string fisTuruKodu = View.Id switch
        {
            "HizmetTanim_DetailView" => "HZMTTN",
            "MasrafTanim_DetailView" => "MSRFTN",
            "StokTanim_DetailView" => "STOKTN",
            _ => null
        };
        if (fisTuruKodu == null) return;

        entity.StokHizmetMasrafTipi = View.Id switch
        {
            "HizmetTanim_DetailView" => StokHizmetMasrafTipi.Hizmet,
            "MasrafTanim_DetailView" => StokHizmetMasrafTipi.Masraf,
            _ => StokHizmetMasrafTipi.Stok
        };
        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == fisTuruKodu));
    }
}
