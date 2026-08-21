/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DepoTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok hareketlerinin işlendiği depo/lokasyon tanımı — eski
 *                    projeden sadeleştirilerek uyarlandı (Şehir/Ülke/Adres
 *                    alanları bu turda yok, ileride eklenebilir). Faz 2
 *                    (StokHareketleriM/D + Ağırlıklı Ortalama Maliyet motoru)
 *                    kapsamının temelidir.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(DepoAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Depo Tanımlama")]
public class DepoTanim : BaseClassWithAudit
{
    public DepoTanim(Session session) : base(session) { }

    bool isVarsayilan;
    string depoKodu;
    string depoAdi;

    [Size(32)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Depo Kodu")]
    [RuleRequiredField("RuleRequired_DepoTanim_DepoKodu", DefaultContexts.Save, "Lütfen Depo Kodunu Giriniz...")]
    public string DepoKodu
    {
        get => depoKodu;
        set => SetPropertyValue(nameof(DepoKodu), ref depoKodu, value);
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Depo Adı")]
    [RuleRequiredField("RuleRequired_DepoTanim_DepoAdi", DefaultContexts.Save, "Lütfen Depo Adını Giriniz...")]
    public string DepoAdi
    {
        get => depoAdi;
        set => SetPropertyValue(nameof(DepoAdi), ref depoAdi, value);
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }

    // BirimTanim/DovizTanim/KDVTanim ile aynı kanıtlanmış tekil-varsayılan deseni.
    protected override void OnSaving()
    {
        if (IsVarsayilan)
        {
            DepoTanim entity = Session.FindObject<DepoTanim>(
                CriteriaOperator.FromLambda<DepoTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }

    // CariHesapTanim.OnDeleting ile aynı üslup: bu depoya bağlı hareket/bakiye/transfer
    // varsa silme reddedilir — aksi halde StokHareketleriD/StokBakiye kayıtları yetim kalır.
    protected override void OnDeleting()
    {
        try
        {
            if (new XPQuery<StokHareketleriM>(Session).Count(x => x.Depo == this) > 0)
                throw new UserFriendlyException("Bu depo hareket görmüş, silemezsiniz.");
            if (new XPQuery<StokBakiye>(Session).Count(x => x.Depo == this) > 0)
                throw new UserFriendlyException("Bu deponun bakiyesi bulunuyor, silemezsiniz.");
            if (new XPQuery<StokTransferi>(Session).Count(x => x.KaynakDepo == this || x.HedefDepo == this) > 0)
                throw new UserFriendlyException("Bu depo transferlerde kullanılmış, silemezsiniz.");
            base.OnDeleting();
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError(ex);
            throw;
        }
    }
}
