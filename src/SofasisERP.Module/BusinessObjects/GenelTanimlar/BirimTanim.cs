/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BirimTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Ölçü birimi tanımı (Adet, Kg, Metre vb.) — eski projeden
 *                    uyarlandı. FisTuruTanim bazlı varsayılan değer hedefi
 *                    olarak seçilebilmesi için IFisTuruVarsayilanHedefi arayüzü
 *                    uygulanır.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(BirimAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Birim Tanımlama")]
public class BirimTanim : BaseClassWithAudit, IFisTuruVarsayilanHedefi
{
    public BirimTanim(Session session) : base(session) { }

    bool isVarsayilan;
    string birimAdi;

    [Size(50)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Birim Adı"), ToolTip("Birim Adını Giriniz")]
    [RuleRequiredField("RuleRequired_BirimTanim_BirimAdi", DefaultContexts.Save, "Lütfen Birim Adını Giriniz...")]
    public string BirimAdi
    {
        get => birimAdi;
        set => SetPropertyValue(nameof(BirimAdi), ref birimAdi, value);
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }

    // NOT: IsVarsayilan alanı vardı ama tekil-zorlama YOKTU — birden fazla "varsayılan"
    // kayıt oluşabiliyordu (denetim raporunda bulundu, doğrulandı). DovizTanim/KDVTanim'deki
    // kanıtlanmış desenle aynı.
    protected override void OnSaving()
    {
        if (IsVarsayilan)
        {
            BirimTanim entity = Session.FindObject<BirimTanim>(
                CriteriaOperator.FromLambda<BirimTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
