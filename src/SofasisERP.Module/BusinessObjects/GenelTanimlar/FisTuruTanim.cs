/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FisTuruTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Belge/fiş türü tanımı — eski projeden uyarlandı (FisTuruKodu =
 *                    numaralandırma öneki). FinansModulTipi/FinansBorcAlacakTipi
 *                    Cari Hesap Hareketleri için eklendi (2026-08-18) — Borç/Alacak
 *                    alan görünürlüğü bunlara göre belirlenir. StokHareketYonu
 *                    (Faz 2, StokHareketleriM/D) aynı desenle eklendi — Stok
 *                    modülünün Giriş/Çıkış yönü, ViewName eski projedeki gibi
 *                    hâlâ BİLEREK yok (bu projede fiş-türü-varyant DetailView'ları
 *                    xafml'de doğrudan tanımlanıyor, ayrı bir alan gerekmiyor).
 *                    FisTuruVarsayilanDegerleri (bu Fiş Türü için tip bazlı
 *                    varsayılan değer tanımlama — ör. HZMTTN için varsayılan Döviz/
 *                    KDV) eski projeden aynen taşındı, bkz. FisTuruVarsayilanDegeri.cs
 *                    ve BaseClass.FisTuruVarsayilanlariniUygula.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(FisTuruAdi))]
[XafDisplayName("Fiş Türü Tanımlama")]
public class FisTuruTanim : BaseClassWithAudit
{
    public FisTuruTanim(Session session) : base(session) { }

    string fisTuruKodu;
    string fisTuruAdi;
    FinansModulTipi finansModulTipi;
    FinansBorcAlacakTipi finansBorcAlacakTipi;
    StokHareketYonu stokHareketYonu;

    // Numaralandırmadaki önek budur (bkz. NumberSequenceService.SonrakiNumara) —
    // ayrı bir "Önek" alanı yok, eski projedeki gibi kod = önek.
    [Size(6)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_FisTuruTanim_FisTuruKodu", DefaultContexts.Save, "Lütfen Fiş Türü Kodunu Giriniz...")]
    [XafDisplayName("Fiş Türü Kodu")]
    public string FisTuruKodu
    {
        get => fisTuruKodu;
        set => SetPropertyValue(nameof(FisTuruKodu), ref fisTuruKodu, value);
    }

    [Size(50)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_FisTuruTanim_FisTuruAdi", DefaultContexts.Save, "Lütfen Fiş Türü Adını Giriniz...")]
    [XafDisplayName("Fiş Türü Adı")]
    public string FisTuruAdi
    {
        get => fisTuruAdi;
        set => SetPropertyValue(nameof(FisTuruAdi), ref fisTuruAdi, value);
    }

    [XafDisplayName("Finans Modül Tipi")]
    public FinansModulTipi FinansModulTipi
    {
        get => finansModulTipi;
        set => SetPropertyValue(nameof(FinansModulTipi), ref finansModulTipi, value);
    }

    [XafDisplayName("Borç Alacak Tipi")]
    public FinansBorcAlacakTipi FinansBorcAlacakTipi
    {
        get => finansBorcAlacakTipi;
        set => SetPropertyValue(nameof(FinansBorcAlacakTipi), ref finansBorcAlacakTipi, value);
    }

    [XafDisplayName("Stok Hareket Yönü")]
    public StokHareketYonu StokHareketYonu
    {
        get => stokHareketYonu;
        set => SetPropertyValue(nameof(StokHareketYonu), ref stokHareketYonu, value);
    }

    [Association("FisTuruTanim-FisTuruVarsayilanDegerleri"), Aggregated]
    [XafDisplayName("Varsayılan Değerler")]
    public XPCollection<FisTuruVarsayilanDegeri> FisTuruVarsayilanDegerleri
        => GetCollection<FisTuruVarsayilanDegeri>(nameof(FisTuruVarsayilanDegerleri));
}
