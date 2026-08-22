/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : YeniKayitFisTuruAtayiciControllerBase.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : 4-ajanlı testte (2026-08-22, yazılım mühendisi bulgusu) tespit
 *                    edildi: StokTanimYeniKayitVarsayilanlariController, StokHareketleri
 *                    YeniKayitVarsayilaniController ve KasaCariBankaHareketleriYeniKayit
 *                    VarsayilaniController "View.Id'ye göre FisTuruKodu bul → FisTuruTanim
 *                    ata" iskeletini birebir tekrarlıyordu. Bu ortak taban sınıf,
 *                    HesapEkstresiRaporuControllerBase'de zaten kanıtlanmış desenle
 *                    (generic taban + kardeş alt sınıflar) aynı yaklaşımı izler — her
 *                    somut alt sınıf yalnızca View.Id→FisTuruKodu eşlemesini ve (varsa)
 *                    kendine özgü ek atamayı tanımlar.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using System.Collections.Generic;

namespace SofasisERP.Module.Controllers.Process;

public abstract class YeniKayitFisTuruAtayiciControllerBase<TEntity> : ViewController<DetailView> where TEntity : class
{
    readonly IReadOnlyDictionary<string, string> viewIdFisTuruKodulari;

    protected YeniKayitFisTuruAtayiciControllerBase(IReadOnlyDictionary<string, string> viewIdFisTuruKodulari)
    {
        this.viewIdFisTuruKodulari = viewIdFisTuruKodulari;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is not TEntity entity) return;
        if (!View.ObjectSpace.IsNewObject(entity)) return;
        if (!viewIdFisTuruKodulari.TryGetValue(View.Id, out string fisTuruKodu)) return;

        BusinessObjects.FisTuruTanim fisTuru = View.ObjectSpace.FindObject<BusinessObjects.FisTuruTanim>(
            CriteriaOperator.FromLambda<BusinessObjects.FisTuruTanim>(x => x.FisTuruKodu == fisTuruKodu));
        VarsayilanlariUygula(entity, fisTuru, View.Id);
    }

    // Her alt sınıf en azından entity.FisTuruTanim = fisTuru atar; StokTanim gibi ek
    // bir alan (StokHizmetMasrafTipi) gerektiren durumlarda burası genişletilir.
    protected abstract void VarsayilanlariUygula(TEntity entity, BusinessObjects.FisTuruTanim fisTuru, string viewId);
}
