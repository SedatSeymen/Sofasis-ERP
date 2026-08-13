using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sofasis.Module.BusinessObjects;


#region Cari Hesap Enum Types

public enum CariKasaBankaTipi
{
    [XafDisplayName("Cari")]
    Cari,
    [XafDisplayName("Kasa")]
    Kasa,
    [XafDisplayName("Banka")]
    Banka
}

public enum CariHesapTipi
{
    [XafDisplayName("Müşteri")]
    Musteri,
    [XafDisplayName("Tedarikçi")]
    Tedarikci,
    [XafDisplayName("Müşteri + Tedarikçi")]
    MüşteriTedarikci
}
public enum CariFirmaHesapTuru
{
    [XafDisplayName("Şahıs Firması")]
    Sahis,
    [XafDisplayName("Limited Şirket")]
    LimitedSirket,
    [XafDisplayName("Anonim Şirket")]
    AnonimSirket
}


#endregion

#region Stok Enum Types

public enum StokHizmetMasrafTipi
{
    [XafDisplayName("Stok")]
    Stok,
    [XafDisplayName("Hizmet")]
    Hizmet,
    [XafDisplayName("Masraf")]
    Masraf
}
public enum StokSiparisSeviyeKontrolu
{
    [XafDisplayName("Yapılsın")]
    Yapilsin,
    [XafDisplayName("Yapılmasın")]
    Yapılmasın
}
public enum StokTipi
{
    [XafDisplayName("Mamül")]
    Mamul,
    [XafDisplayName("Yarı Mamül")]
    YarıMamul,
    [XafDisplayName("Ham Madde")]
    Hammadde,
    [XafDisplayName("Diğer")]
    Diger
}

public enum StokHareketYonu
{
    [XafDisplayName("Yok")]
    Yok,
    [XafDisplayName("Giriş")]
    Giris,
    [XafDisplayName("Çıkış")]
    Cikis
}

public enum NegatifStokPolitikasi
{
    [XafDisplayName("İzin Ver")]
    IzinVer,
    [XafDisplayName("Uyar")]
    Uyar,
    [XafDisplayName("Engelle")]
    Engelle
}

#endregion

#region Genel Tanımlar Enum Types

public enum FinansModulTipi
{
    [XafDisplayName("Stok")]
    Stok,
    [XafDisplayName("Cari")]
    Cari,
    [XafDisplayName("Fatura")]
    Fatura,
    [XafDisplayName("İrsaliye")]
    Irsaliye,
    [XafDisplayName("Sipariş")]
    Siparis,
    [XafDisplayName("Kasa")]
    Kasa,
    [XafDisplayName("Banka")]
    Banka,
    [XafDisplayName("Çek")]
    Cek,
    [XafDisplayName("Senet")]
    Senet,
    [XafDisplayName("Üretim")]
    Uretim,
    [XafDisplayName("Satın Alma")]
    SatinAlma,
}

public enum FinansBorcAlacakTipi
{
    [XafDisplayName("Yok")]
    Yok,
    [XafDisplayName("Borç")]
    Borc,
    [XafDisplayName("Alacak")]
    Alacak,
    [XafDisplayName("Borç/Alacak")]
    BorcAlacak
}


public enum KDVTipi
{
    [XafDisplayName("KDV Hariç")]
    KDVHaric,
    [XafDisplayName("KDV Dahil")]
    KDVDahil
}

public enum DovizKuruTipi
{
    [XafDisplayName("Alış Kuru")]
    Alis,
    [XafDisplayName("Satış Kuru")]
    Satis
}

// GenelParametre.MiktarOndalikMaski / TutarOndalikMaski için ortak seçenek kümesi — her iki
// alan da aynı 0-6 ondalık basamak aralığından seçilebilir; tek enum ile tekrar önlenir.
public enum OndalikBasamakSayisi
{
    [XafDisplayName("0 (#,##0)")]
    Basamak0 = 0,
    [XafDisplayName("1 (#,##0.0)")]
    Basamak1 = 1,
    [XafDisplayName("2 (#,##0.00)")]
    Basamak2 = 2,
    [XafDisplayName("3 (#,##0.000)")]
    Basamak3 = 3,
    [XafDisplayName("4 (#,##0.0000)")]
    Basamak4 = 4,
    [XafDisplayName("5 (#,##0.00000)")]
    Basamak5 = 5,
    [XafDisplayName("6 (#,##0.000000)")]
    Basamak6 = 6,
}


#endregion

#region Kasa Banka Enum Types

#endregion

#region Üretim Yönetimi
public enum ReceteMalzemeTipi
{
    [XafDisplayName("Sabit")]
    Sabit,
    [XafDisplayName("Değişken")]
    Degisken
}

#endregion

#region Satın Alma Yönetimi

// Eski AlimSiparisDurumu enum'u (OnayBekliyor/Onaylandi/SiparisVerildi/TeslimAlindi) hiçbir sınıfa
// bağlı değildi (silinmiş bir SASiparisM taslağından kalan ölü kod) — SatinAlmaTalebiM.Durum
// tarafından gerçekten kullanılan bu enum'la değiştirildi.
public enum SatinAlmaTalepDurumu
{
    [XafDisplayName("Taslak")]
    Taslak,
    [XafDisplayName("Onay Bekliyor")]
    OnayBekliyor,
    [XafDisplayName("Onaylandı")]
    Onaylandi,
    [XafDisplayName("Reddedildi")]
    Reddedildi,
    [XafDisplayName("Teklife Çıkıldı")]
    TeklifeCikildi,
    [XafDisplayName("Sipariş Edildi")]
    SiparisEdildi,
    [XafDisplayName("İptal Edildi")]
    IptalEdildi
}

public enum SatinAlmaTeklifDurumu
{
    [XafDisplayName("Beklemede")]
    Beklemede,
    [XafDisplayName("Alındı")]
    Alindi,
    [XafDisplayName("Seçildi")]
    Secildi,
    [XafDisplayName("Reddedildi")]
    Reddedildi
}

// v1'de kısmi teslimat/kısmi faturalama YOK — bir Sipariş tek seferde tam Mal Kabul edilir (SA-4)
// ve tek Fatura ile kapanır (SA-5); bu yüzden yalnızca 4 durum yeterli.
public enum SatinAlmaSiparisDurumu
{
    [XafDisplayName("Verildi")]
    Verildi,
    [XafDisplayName("Mal Kabul Yapıldı")]
    MalKabulYapildi,
    [XafDisplayName("Faturalandı")]
    Faturalandi,
    [XafDisplayName("İptal Edildi")]
    IptalEdildi
}

// SA-5 (Alış Faturası) kapsamında üçlü eşleştirme (Sipariş/Mal Kabul/Fatura miktar-fiyat) sapması
// bulunduğunda izlenecek politika. NegatifStokPolitikasi (StokParametre) ile değer kümesi kasıtlı
// olarak aynı 3'lüdür ama anlamsal olarak farklı bir parametreye ait olduğundan yeniden KULLANILMAZ.
public enum UcluEslestirmePolitikasi
{
    [XafDisplayName("İzin Ver")]
    IzinVer,
    [XafDisplayName("Uyar")]
    Uyar,
    [XafDisplayName("Engelle")]
    Engelle
}
#endregion

#region Satış Pazarlama Yönetimi

public enum SiparisTipi
{
    [XafDisplayName("YURTİÇİ SİPARİŞİ")]
    YurticiSiparisi,
    [XafDisplayName("YURTDIŞI SİPARİŞİ")]
    YurtdisiSiparisi,
    [XafDisplayName("STOK SİPARİŞİ")]
    StokSiparisi,
    [XafDisplayName("TAMİR SİPARİŞİ")]
    TamirSiparisi
}

public enum SatisSiparisDurumu
{
    [XafDisplayName("SİPARİŞ GİRİLDİ")]
    SiparisGirildi,
    [XafDisplayName("ÜRETİM PLANI YAPILDI")]
    Planlandi,
    [XafDisplayName("ÜRETİME ALINDI")]
    UretimeAlindi,
    [XafDisplayName("ÜRETİLDİ")]
    Uretildi,
    [XafDisplayName("SEVK BEKLİYOR")]
    SevkBekliyor,
    [XafDisplayName("SEVK EDİLDİ")]
    SevkEdildi
}
#endregion
