/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapEkstresiKriteri.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Cari Hesap Ekstresi" raporu için popup kriter ekranı — Hesap
 *                    alanı CariHesapTanim tipinde olduğundan lookup otomatik olarak
 *                    yalnızca Cari hesapları listeler (DataSourceCriteria gerekmez).
 *                    Kasa/Banka için ayrı (yapısal olarak aynı) kriter sınıfları var —
 *                    bkz. KasaHesapEkstresiKriteri.cs, BankaHesapEkstresiKriteri.cs.
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
public class CariHesapEkstresiKriteri : XPLiteObject
{
    public CariHesapEkstresiKriteri(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        DateTime bugun = TurkiyeZamani.Bugun;
        baslangicTarihi = new DateTime(bugun.Year, bugun.Month, 1);
        bitisTarihi = bugun;
    }

    CariHesapTanim hesap;
    DateTime baslangicTarihi;
    DateTime bitisTarihi;

    [XafDisplayName("Cari Hesap")]
    [RuleRequiredField("RuleRequired_CariHesapEkstresiKriteri_Hesap", DefaultContexts.Save, "Lütfen bir Cari Hesap seçiniz...")]
    public CariHesapTanim Hesap
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
