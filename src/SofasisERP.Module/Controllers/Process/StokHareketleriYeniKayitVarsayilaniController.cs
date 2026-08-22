/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHareketleriYeniKayitVarsayilaniController.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok Hareketleri'nin Fiş Türü bazlı ayrı ekranlarından
 *                    (Açılış/Alış Girişi/Satış Çıkışı/Sayım Fazlası/Sayım
 *                    Eksiği/Fire) "Yeni" ile açılan fişe View.Id'ye göre
 *                    FisTuruTanim'i otomatik atar — ortak iskelet artık
 *                    YeniKayitFisTuruAtayiciControllerBase'de (bkz. o dosya).
 * ****************************************************************************
 */

using SofasisERP.Module.BusinessObjects;
using System.Collections.Generic;

namespace SofasisERP.Module.Controllers.Process;

public class StokHareketleriYeniKayitVarsayilaniController : YeniKayitFisTuruAtayiciControllerBase<StokHareketleriM>
{
    static readonly Dictionary<string, string> ViewIdFisTuruKodulari = new()
    {
        ["StokHareketleriM_AcilisDetailView"] = "STACLS",
        ["StokHareketleriM_AlisGirisiDetailView"] = "STALIS",
        ["StokHareketleriM_MalKabulGirisiDetailView"] = "STSAGR",
        ["StokHareketleriM_UretimGirisiDetailView"] = "STURET",
        ["StokHareketleriM_SayimFazlasiDetailView"] = "STSFAZ",
        ["StokHareketleriM_SatisCikisiDetailView"] = "STSTIS",
        ["StokHareketleriM_UretimTuketimiDetailView"] = "STURTK",
        ["StokHareketleriM_SayimEksigiDetailView"] = "STSEKS",
        ["StokHareketleriM_FireDetailView"] = "STFIRE",
        ["StokHareketleriM_SatinAlmaIadeCikisiDetailView"] = "STSAIC",
        ["StokHareketleriM_SatisIadeGirisiDetailView"] = "STSTIG",
    };

    public StokHareketleriYeniKayitVarsayilaniController() : base(ViewIdFisTuruKodulari) { }

    protected override void VarsayilanlariUygula(StokHareketleriM entity, FisTuruTanim fisTuru, string viewId)
        => entity.FisTuruTanim = fisTuru;
}
