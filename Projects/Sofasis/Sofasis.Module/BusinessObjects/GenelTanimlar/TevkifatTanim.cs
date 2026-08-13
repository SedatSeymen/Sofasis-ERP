using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects;

// KDVTanim ile birebir aynı desen (varsayılan tekil kayıt, ordinal-güvenli oran alanı).
// TevkifatOrani, KDV tutarının yüzde kaçının tevkifata tabi olduğunu tutar (ör. Türk mevzuatındaki
// 5/10 payı = %50, 9/10 payı = %90) — pay/payda kesir gösterimi yerine BİLEREK tek bir yüzde alanı
// kullanıldı (YAGNI: hesaplama için tek gereken değer bu, kesir gösterimi yalnızca kozmetik olurdu).
[DefaultProperty("TevkifatAdi")]
[DefaultClassOptions]
[XafDisplayName("Tevkifat Tanımlama")]
[NavigationItem("Genel Tanımlar")]
public class TevkifatTanim : BaseClassWithAudit
{
    public TevkifatTanim(Session session)
        : base(session)
    {
    }

    string tevkifatAdi;
    decimal tevkifatOrani;
    bool isVarsayilan;

    [Size(50)]
    [XafDisplayName("Tevkifat Adı")]
    [RuleRequiredField("RuleRequired_TevkifatTanim_TevkifatAdi", DefaultContexts.Save, "Lütfen Tevkifat Adını Giriniz...")]
    public string TevkifatAdi
    {
        get => tevkifatAdi;
        set => SetPropertyValue(nameof(TevkifatAdi), ref tevkifatAdi, value);
    }

    [XafDisplayName("Tevkifat Oranı (%)"), ToolTip("KDV tutarının yüzde kaçının tevkifata tabi olduğunu giriniz")]
    [ModelDefault("EditMask", "N0")]
    [ModelDefault("DisplayFormat", "{0:N0}")]
    [RuleValueComparison("Rule_TevkifatTanim_Orani_Araligi_Min", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen tevkifat oranını sıfırdan büyük giriniz.")]
    [RuleValueComparison("Rule_TevkifatTanim_Orani_Araligi_Max", DefaultContexts.Save, ValueComparisonType.LessThanOrEqual, 100,
        CustomMessageTemplate = "Tevkifat oranı 100'den büyük olamaz.")]
    public decimal TevkifatOrani
    {
        get => tevkifatOrani;
        set => SetPropertyValue(nameof(TevkifatOrani), ref tevkifatOrani, value);
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }

    protected override void OnSaving()
    {
        if (IsVarsayilan)
        {
            TevkifatTanim entity = Session.FindObject<TevkifatTanim>(
                CriteriaOperator.FromLambda<TevkifatTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
