/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ModelSetDetay.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : ModelSetTanim'in Aggregated detayı — saf Detail,
 *                    [DefaultClassOptions] BİLEREK yok, yalnızca ModelSetTanim'in
 *                    koleksiyonundan düzenlenir. Aynı Set içinde aynı Stok iki kez
 *                    eklenemez, aynı Sıra No iki kez kullanılamaz (kullanıcı kararı).
 *                    SiraNo elle girilmez — kullanıcı kararı (2026-08-18):
 *                    OnSaving'de aynı ModelSetTanim içindeki mevcut en yüksek
 *                    SiraNo + 1 olarak otomatik atanır.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[XafDisplayName("Model Set Detay")]
public class ModelSetDetay : BaseClass
{
    public ModelSetDetay(Session session) : base(session) { }

    ModelSetTanim modelSetTanim;
    StokTanim stokTanim;
    int siraNo;

    [Association("ModelSetTanim-ModelSetDetaylari")]
    [Appearance("InvisibleModelSetTanim_Detail", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
    [XafDisplayName("Model Set")]
    public ModelSetTanim ModelSetTanim
    {
        get => modelSetTanim;
        set => SetPropertyValue(nameof(ModelSetTanim), ref modelSetTanim, value);
    }

    // Yalnızca master'daki ModelSetTanim.ModelTanim'e ait kayıtlar seçilebilir — DataSourceProperty
    // zinciri ModelTanim.StokTanims'e kadar iniyor (bkz. ModelTanim.cs, StokAltGrupTanim'deki
    // kanıtlanmış "StokGrubu.StokAltGrupTanims" deseniyle aynı). @This ile tek attribute içinde
    // nesne-tipi karşılaştırması (Model = @This...) DENENDİ, XPO'da "@This" bir property yolu
    // gibi StokTanim sınıfı üzerinde çözümlenmeye çalışıp InvalidPropertyPathException fırlattı —
    // canlı testte yakalandı. DataSourceCriteria ise yalnızca sabit (nesne bağımsız) Hizmet/Masraf
    // filtresi için kullanılıyor.
    [Indexed(nameof(ModelSetTanim), Unique = true)]
    [DataSourceProperty("ModelSetTanim.ModelTanim.StokTanims", DataSourcePropertyIsNullMode.SelectNothing)]
    [DataSourceCriteria("StokHizmetMasrafTipi = 'Stok'")]
    [RuleRequiredField("RuleRequired_ModelSetDetay_StokTanim", DefaultContexts.Save, "Lütfen Stok Seçiniz...")]
    [XafDisplayName("Stok")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    [Indexed(nameof(ModelSetTanim), Unique = true)]
    [Appearance("ED_ModelSetDetay_SiraNo", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Sıra No")]
    public int SiraNo
    {
        get => siraNo;
        set => SetPropertyValue(nameof(SiraNo), ref siraNo, value);
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && SiraNo == 0 && ModelSetTanim != null)
        {
            int maxSiraNo = ModelSetTanim.ModelSetDetaylari
                .Where(x => x != this)
                .Select(x => (int?)x.SiraNo)
                .Max() ?? 0;
            SiraNo = maxSiraNo + 1;
        }
        base.OnSaving();
    }
}
