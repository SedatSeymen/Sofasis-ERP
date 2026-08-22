/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokTanimYeniKayitVarsayilanlariController.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok/Hizmet/Masraf Tanımlama ekranlarından "Yeni" ile açılan
 *                    StokTanim kaydına StokHizmetMasrafTipi VE FisTuruTanim'i
 *                    View.Id'ye göre otomatik atar — ortak iskelet artık
 *                    YeniKayitFisTuruAtayiciControllerBase'de (bkz. o dosya).
 * ****************************************************************************
 */

using SofasisERP.Module.BusinessObjects;
using System.Collections.Generic;

namespace SofasisERP.Module.Controllers.Process;

public class StokTanimYeniKayitVarsayilanlariController : YeniKayitFisTuruAtayiciControllerBase<StokTanim>
{
    static readonly Dictionary<string, string> ViewIdFisTuruKodulari = new()
    {
        ["HizmetTanim_DetailView"] = "HZMTTN",
        ["MasrafTanim_DetailView"] = "MSRFTN",
        ["StokTanim_DetailView"] = "STOKTN",
    };

    public StokTanimYeniKayitVarsayilanlariController() : base(ViewIdFisTuruKodulari) { }

    protected override void VarsayilanlariUygula(StokTanim entity, FisTuruTanim fisTuru, string viewId)
    {
        entity.StokHizmetMasrafTipi = viewId switch
        {
            "HizmetTanim_DetailView" => StokHizmetMasrafTipi.Hizmet,
            "MasrafTanim_DetailView" => StokHizmetMasrafTipi.Masraf,
            _ => StokHizmetMasrafTipi.Stok
        };
        entity.FisTuruTanim = fisTuru;
    }
}
