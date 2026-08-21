/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariGrupTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap gruplama tanımı — eski ERP'den birebir taşındı
 *                    (kullanıcı kararı 2026-08-18). Sistem kaydı kilidi ve
 *                    varsayılan-turuncu Appearance'ları BaseClass'ımızda zaten
 *                    genel kural olarak var, ayrıca eklenmedi.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(CariGrupAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Cari Grup Tanımlama")]
public class CariGrupTanim : BaseClassWithAuditAndDescription
{
    public CariGrupTanim(Session session) : base(session) { }

    bool isVarsayilan;
    string cariGrupAdi;

    [Size(50)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_CariGrupTanim_CariGrupAdi", DefaultContexts.Save, "Lütfen Cari Grup Adını Giriniz...")]
    [XafDisplayName("Cari Grup Adı")]
    public string CariGrupAdi
    {
        get => cariGrupAdi;
        set => SetPropertyValue(nameof(CariGrupAdi), ref cariGrupAdi, value);
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
            CariGrupTanim entity = Session.FindObject<CariGrupTanim>(
                CriteriaOperator.FromLambda<CariGrupTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
