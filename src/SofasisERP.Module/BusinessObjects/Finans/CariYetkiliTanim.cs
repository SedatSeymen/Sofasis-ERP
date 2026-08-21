/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariYetkiliTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : CariHesapTanim'in Aggregated detayı — kullanıcı isteği
 *                    (2026-08-18): Cari Hesap kartında yetkili kişi (irtibat)
 *                    bilgileri. Saf Detail — [DefaultClassOptions] BİLEREK yok.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(AdSoyad))]
[XafDisplayName("Cari Yetkili Tanımlama")]
public class CariYetkiliTanim : BaseClass
{
    public CariYetkiliTanim(Session session) : base(session) { }

    string adSoyad;
    string gorevi;
    string telefon;
    string ePosta;
    CariHesapTanim cariHesapTanim;

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleRequiredField("RuleRequired_CariYetkiliTanim_AdSoyad", DefaultContexts.Save, "Lütfen Ad Soyad Giriniz...")]
    [XafDisplayName("Ad Soyad")]
    public string AdSoyad
    {
        get => adSoyad;
        set => SetPropertyValue(nameof(AdSoyad), ref adSoyad, value);
    }

    [Size(50)]
    [XafDisplayName("Görevi")]
    public string Gorevi
    {
        get => gorevi;
        set => SetPropertyValue(nameof(Gorevi), ref gorevi, value);
    }

    [Size(16)]
    [XafDisplayName("Telefon")]
    public string Telefon
    {
        get => telefon;
        set => SetPropertyValue(nameof(Telefon), ref telefon, value);
    }

    [Size(100)]
    [RuleRegularExpression(DefaultContexts.Save, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
        CustomMessageTemplate = "Lütfen Geçerli Bir E Posta Adresi Giriniz...")]
    [XafDisplayName("E Posta")]
    public string EPosta
    {
        get => ePosta;
        set => SetPropertyValue(nameof(EPosta), ref ePosta, value);
    }

    [Association("CariHesapTanim-CariYetkiliTanimlari")]
    [Appearance("InvisibleCariHesapTanim_YetkiliDetail", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
    [XafDisplayName("Cari Hesap")]
    public CariHesapTanim CariHesapTanim
    {
        get => cariHesapTanim;
        set => SetPropertyValue(nameof(CariHesapTanim), ref cariHesapTanim, value);
    }
}
