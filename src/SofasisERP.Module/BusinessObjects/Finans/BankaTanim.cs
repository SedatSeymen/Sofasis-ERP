/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BankaTanim.cs
 * Oluşturma Tarihi : 08/19/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/19/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Faz 3 — Banka KURUMU tanımı (ör. "Türkiye İş Bankası A.Ş.").
 *                    BankaHesabiTanim'den (bir hesabın kendisi) FARKLI bir katman —
 *                    eski ERP'deki BankaTanim.cs ile birebir port. Katalog/lookup
 *                    niteliğinde, DovizTanim/UlkeTanim gibi genel tanımlarla aynı
 *                    üsluptaki bir referans tablosu.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(BankaAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Banka Tanımlama")]
public class BankaTanim : BaseClassWithAudit
{
    public BankaTanim(Session session) : base(session) { }

    string bankaAdi;
    string swiftKodu;

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Banka Adı"), ToolTip("Banka Adını Giriniz")]
    [RuleRequiredField("RuleRequired_BankaTanim_BankaAdi", DefaultContexts.Save, "Lütfen Banka Adını Giriniz...")]
    public string BankaAdi
    {
        get => bankaAdi;
        set => SetPropertyValue(nameof(BankaAdi), ref bankaAdi, value);
    }

    [Size(11)]
    [XafDisplayName("Swift Kodu"), ToolTip("Bankanın SWIFT/BIC kodunu giriniz (örn. TCZBTR2A)")]
    [RuleRegularExpression(DefaultContexts.Save, "^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$",
        SkipNullOrEmptyValues = true, CustomMessageTemplate = "Lütfen Geçerli Bir SWIFT/BIC Kodu Giriniz...")]
    public string SwiftKodu
    {
        get => swiftKodu;
        set => SetPropertyValue(nameof(SwiftKodu), ref swiftKodu, value);
    }
}
