/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaTanim.cs
 * Oluşturma Tarihi : 08/19/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/19/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Faz 3 düzeltmesi — artık Hesap'tan türer (bkz. Hesap.cs).
 *                    HesapAdi/DovizTanim/IsVarsayilan/GuncelBakiye taban sınıfta;
 *                    burada yalnızca Kasa'ya özgü KasaKodu kalır. Kod üretimi
 *                    üretilmez ama tekil-varsayılan deseni KasaTanim TÜRÜ içinde
 *                    kalmaya devam eder (Session.FindObject&lt;KasaTanim&gt; — XPO'nun
 *                    kalıtım filtrelemesi otomatik, Banka/Cari kayıtlarını hiç
 *                    görmez).
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(HesapAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Kasa Tanımlama")]
public class KasaTanim : KasaBankaHesabi
{
    public KasaTanim(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this)) HesapTuru = HesapTuru.Kasa;
    }

    string kasaKodu;

    [Size(32)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Kasa Kodu")]
    [RuleRequiredField("RuleRequired_KasaTanim_KasaKodu", DefaultContexts.Save, "Lütfen Kasa Kodunu Giriniz...")]
    public string KasaKodu
    {
        get => kasaKodu;
        set => SetPropertyValue(nameof(KasaKodu), ref kasaKodu, value);
    }

    protected override void OnSaving()
    {
        if (IsVarsayilan)
        {
            KasaTanim entity = Session.FindObject<KasaTanim>(
                CriteriaOperator.FromLambda<KasaTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
