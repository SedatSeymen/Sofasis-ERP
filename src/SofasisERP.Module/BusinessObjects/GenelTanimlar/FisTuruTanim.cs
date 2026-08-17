/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FisTuruTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Belge/fiş türü tanımı — eski projeden uyarlandı, şu an
 *                    yalnızca StokTanim/Hizmet/Masraf numaralandırması için
 *                    gereken minimal hâliyle taşındı (FisTuruKodu = numaralandırma
 *                    öneki). Eski projedeki FinansModulTipi/FinansBorcAlacakTipi/
 *                    StokHareketYonu/ViewName alanları BİLEREK yok — Kasa/Banka/
 *                    Cari/Fatura/İrsaliye/StokHareketi modülleri bu projede henüz
 *                    mevcut değil, o modüller eklendiğinde bu sınıf genişletilecek.
 *                    FisTuruVarsayilanDegerleri ise (bu Fiş Türü için tip bazlı
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
[DefaultProperty(nameof(FisTuruAdi))]
[XafDisplayName("Fiş Türü Tanımlama")]
public class FisTuruTanim : BaseClassWithAudit
{
    public FisTuruTanim(Session session) : base(session) { }

    string fisTuruKodu;
    string fisTuruAdi;

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

    [Association("FisTuruTanim-FisTuruVarsayilanDegerleri"), Aggregated]
    [XafDisplayName("Varsayılan Değerler")]
    public XPCollection<FisTuruVarsayilanDegeri> FisTuruVarsayilanDegerleri
        => GetCollection<FisTuruVarsayilanDegeri>(nameof(FisTuruVarsayilanDegerleri));
}
