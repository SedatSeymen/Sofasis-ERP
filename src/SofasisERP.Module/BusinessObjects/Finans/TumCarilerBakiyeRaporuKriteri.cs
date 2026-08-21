/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TumCarilerBakiyeRaporuKriteri.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Tüm Cariler Borç/Alacak Raporu" için popup kriter ekranı.
 *                    Tüm alanlar OPSİYONEL — hiçbiri girilmezse rapor mevcut ay için
 *                    TÜM Cari hesapları listeler. Cari Hesap Kodu aralığı iki referans
 *                    (Başlangıç/Bitiş Cari) üzerinden kodun alfabetik/sayısal sırasına
 *                    göre filtrelenir (bkz. TumCarilerBakiyeRaporuController).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[NonPersistent]
[XafDisplayName("Rapor Kriterleri")]
public class TumCarilerBakiyeRaporuKriteri : XPLiteObject
{
    public TumCarilerBakiyeRaporuKriteri(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        DateTime bugun = TurkiyeZamani.Bugun;
        baslangicTarihi = new DateTime(bugun.Year, bugun.Month, 1);
        bitisTarihi = bugun;
    }

    DateTime baslangicTarihi;
    DateTime bitisTarihi;
    CariHesapTanim baslangicCari;
    CariHesapTanim bitisCari;

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

    [XafDisplayName("Başlangıç Cari Hesap Kodu")]
    public CariHesapTanim BaslangicCari
    {
        get => baslangicCari;
        set => SetPropertyValue(nameof(BaslangicCari), ref baslangicCari, value);
    }

    [XafDisplayName("Bitiş Cari Hesap Kodu")]
    public CariHesapTanim BitisCari
    {
        get => bitisCari;
        set => SetPropertyValue(nameof(BitisCari), ref bitisCari, value);
    }
}
