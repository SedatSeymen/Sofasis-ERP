/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KDVTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : KDV oranı tanımı — eski projeden uyarlandı.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(KDVOrani))]
[DefaultClassOptions]
[XafDisplayName("KDV Tanımlama")]
public class KDVTanim : BaseClassWithAudit
{
    public KDVTanim(Session session) : base(session) { }

    bool isVarsayilan;
    decimal kDVOrani;

    [XafDisplayName("KDV Oranı"), ToolTip("KDV Oranını Giriniz")]
    [ModelDefault("EditMask", "N")]
    [ModelDefault("DisplayFormat", "{0:N0}")]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    public decimal KDVOrani
    {
        get => kDVOrani;
        set => SetPropertyValue(nameof(KDVOrani), ref kDVOrani, value);
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
            KDVTanim entity = Session.FindObject<KDVTanim>(
                CriteriaOperator.FromLambda<KDVTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
