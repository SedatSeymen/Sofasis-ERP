/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaHesapEkstresiKriteri.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Kasa Ekstresi" raporu için popup kriter ekranı — bkz.
 *                    CariHesapEkstresiKriteri.cs'deki genel açıklama.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[NonPersistent]
[XafDisplayName("Rapor Kriterleri")]
public class KasaHesapEkstresiKriteri : XPLiteObject
{
    public KasaHesapEkstresiKriteri(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        DateTime bugun = TurkiyeZamani.Bugun;
        baslangicTarihi = new DateTime(bugun.Year, bugun.Month, 1);
        bitisTarihi = bugun;
    }

    KasaTanim hesap;
    DateTime baslangicTarihi;
    DateTime bitisTarihi;

    [XafDisplayName("Kasa")]
    [RuleRequiredField("RuleRequired_KasaHesapEkstresiKriteri_Hesap", DefaultContexts.Save, "Lütfen bir Kasa seçiniz...")]
    public KasaTanim Hesap
    {
        get => hesap;
        set => SetPropertyValue(nameof(Hesap), ref hesap, value);
    }

    [XafDisplayName("Başlangıç Tarihi")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    public DateTime BaslangicTarihi
    {
        get => baslangicTarihi;
        set => SetPropertyValue(nameof(BaslangicTarihi), ref baslangicTarihi, value);
    }

    [XafDisplayName("Bitiş Tarihi")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    public DateTime BitisTarihi
    {
        get => bitisTarihi;
        set => SetPropertyValue(nameof(BitisTarihi), ref bitisTarihi, value);
    }
}
