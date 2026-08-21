/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BankaHesabiTanim.cs
 * Oluşturma Tarihi : 08/19/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/19/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Faz 3 düzeltmesi — artık Hesap'tan türer (bkz. Hesap.cs).
 *                    HesapAdi/DovizTanim/IsVarsayilan/GuncelBakiye taban sınıfta;
 *                    "BankaHesapAdi" diye ayrı bir alan YOK — HesapAdi, BankaTanim/
 *                    SubeAdi/HesapNo değiştikçe "{BankaAdi}-{ŞubeAdi}-{HesapNo}"
 *                    biçiminde otomatik hesaplanır (kullanıcı isteği), alan kilitli.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(HesapAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Banka Hesabı Tanımlama")]
[Appearance("ED_BankaHesabiTanim_HesapAdi", Enabled = false, TargetItems = nameof(HesapAdi), Context = "DetailView")]
public class BankaHesabiTanim : KasaBankaHesabi
{
    public BankaHesabiTanim(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this)) HesapTuru = HesapTuru.Banka;
    }

    BankaTanim bankaTanim;
    string subeAdi;
    string hesapNo;
    string iban;

    [XafDisplayName("Banka")]
    [RuleRequiredField("RuleRequired_BankaHesabiTanim_BankaTanim", DefaultContexts.Save, "Lütfen Banka Seçiniz...")]
    public BankaTanim BankaTanim
    {
        get => bankaTanim;
        set => SetPropertyValue(nameof(BankaTanim), ref bankaTanim, value);
    }

    [Size(64)]
    [XafDisplayName("Şube Adı")]
    [RuleRequiredField("RuleRequired_BankaHesabiTanim_SubeAdi", DefaultContexts.Save, "Lütfen Şube Adını Giriniz...")]
    public string SubeAdi
    {
        get => subeAdi;
        set => SetPropertyValue(nameof(SubeAdi), ref subeAdi, value);
    }

    [Size(32)]
    [RuleUniqueValue]
    [XafDisplayName("Hesap No")]
    [RuleRequiredField("RuleRequired_BankaHesabiTanim_HesapNo", DefaultContexts.Save, "Lütfen Hesap Numarasını Giriniz...")]
    public string HesapNo
    {
        get => hesapNo;
        set => SetPropertyValue(nameof(HesapNo), ref hesapNo, value);
    }

    [Size(34)]
    [RuleUniqueValue]
    [XafDisplayName("IBAN")]
    public string IBAN
    {
        get => iban;
        set => SetPropertyValue(nameof(IBAN), ref iban, value);
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (propertyName is nameof(BankaTanim) or nameof(SubeAdi) or nameof(HesapNo))
            HesapAdiniHesapla();
    }

    void HesapAdiniHesapla()
    {
        if (BankaTanim == null || string.IsNullOrEmpty(SubeAdi) || string.IsNullOrEmpty(HesapNo))
            return;
        HesapAdi = $"{BankaTanim.BankaAdi}-{SubeAdi}-{HesapNo}";
    }

    protected override void OnSaving()
    {
        HesapAdiniHesapla();

        if (IsVarsayilan)
        {
            BankaHesabiTanim entity = Session.FindObject<BankaHesabiTanim>(
                CriteriaOperator.FromLambda<BankaHesabiTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
