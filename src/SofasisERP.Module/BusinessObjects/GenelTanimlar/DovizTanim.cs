/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DovizTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Para birimi tanımı — eski projeden uyarlandı. FisTuruTanim
 *                    bazlı varsayılan değer hedefi olarak seçilebilmesi için
 *                    IFisTuruVarsayilanHedefi arayüzü uygulanır.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(DovizKodu))]
[DefaultClassOptions]
[XafDisplayName("Döviz Tanımlama")]
public class DovizTanim : BaseClassWithAudit, IFisTuruVarsayilanHedefi
{
    public DovizTanim(Session session) : base(session) { }

    bool isVarsayilan;
    string dovizKodu;
    string dovizAdi;
    string sembol;

    [Size(5)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Döviz Kodu"), ToolTip("Döviz Kodu Giriniz")]
    [RuleRequiredField("RuleRequired_DovizTanim_DovizKodu", DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
    [Appearance("ED_DovizTanim_DovizKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    public string DovizKodu
    {
        get => dovizKodu;
        set => SetPropertyValue(nameof(DovizKodu), ref dovizKodu, value);
    }

    [Size(50)]
    [XafDisplayName("Döviz Adı"), ToolTip("Döviz Adını Giriniz")]
    [RuleRequiredField("RuleRequired_DovizTanim_DovizAdi", DefaultContexts.Save, "Lütfen Döviz Adını Giriniz...")]
    public string DovizAdi
    {
        get => dovizAdi;
        set => SetPropertyValue(nameof(DovizAdi), ref dovizAdi, value);
    }

    // Tutar alanlarının ondalık maskında her para birimi kendi sembolüyle
    // gösterilebilir diye ayrı tutulur — sabit ₺ değil.
    [Size(5)]
    [XafDisplayName("Sembol"), ToolTip("Para Birimi Sembolü (₺, $, € vb.)")]
    public string Sembol
    {
        get => sembol;
        set => SetPropertyValue(nameof(Sembol), ref sembol, value);
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
            DovizTanim entity = Session.FindObject<DovizTanim>(
                CriteriaOperator.FromLambda<DovizTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
