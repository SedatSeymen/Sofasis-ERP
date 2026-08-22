/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaCariBankaHareketleri.cs
 * Oluşturma Tarihi : 08/20/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/20/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kullanıcı kararıyla (2026-08-20) MASTER-DETAIL OLMAYAN, tek düz
 *                    seviyeli Kasa/Banka/Cari hareket tablosu — eski ERP'nin
 *                    (D:\2025\SofasisERP) KasaBankaHareketleri.cs/CariHesapHareketleri.cs
 *                    deseninden esinlenildi: her kayıt kendi başına TAM bir harekettir,
 *                    ayrı bir Fiş/Satır (Master/Detail) çifti YOK.
 *
 *                    Eski ERP, Kasa/Banka ve Cari arasında ortak bir taban tip
 *                    olmadığından İKİ AYRI sınıf + "ayna kayıt" (mirror record,
 *                    IntegrationCode eşleştirmesi + sonsuz-döngü önleyici bayrak)
 *                    kullanmak zorundaydı. Bu projede Kasa/Banka/Cari zaten ORTAK
 *                    bir taban tipten (Hesap, Class Table Inheritance) türediğinden
 *                    (bkz. Hesap.cs) bu karmaşıklığa hiç gerek yok: TEK sınıf, İKİ
 *                    Hesap-tipli alan (KaynakHesap/KarsiHesap) yeterli — bir satır
 *                    "WHERE KaynakHesap = X OR KarsiHesap = X" sorgusuyla hem Kasa'nın
 *                    hem Cari'nin listesinde otomatik görünür, ayna/ikinci fiziksel
 *                    satır üretilmez.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.ComponentModel;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(FisNo))]
[XafDisplayName("Kasa/Cari/Banka Hareketi")]
[RuleCriteria("Rule_KasaCariBankaHareketleri_Denge", DefaultContexts.Save,
    "YerelBorcTutar = YerelAlacakTutar",
    "Fiş dengesiz: Yerel Borç, Yerel Alacak'a eşit değil. Farklı döviz cinsli hesaplar arasında kur farkı bu sistemde otomatik dengelenmez — tutarları eşitleyiniz.")]
public class KasaCariBankaHareketleri : BaseClassWithAuditAndDescription
{
    public KasaCariBankaHareketleri(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            FisTarihi = TurkiyeZamani.Bugun;
            BelgeTarihi = TurkiyeZamani.Bugun;
        }
    }

    string fisNo;
    DateTime fisTarihi;
    FisTuruTanim fisTuruTanim;
    string belgeNo;
    DateTime belgeTarihi;
    DateTime vadeTarihi;
    OdemeSekli odemeSekli;
    Hesap kaynakHesap;
    Hesap karsiHesap;
    decimal borcTutar;
    decimal alacakTutar;
    DovizTanim dovizTanim;
    decimal dovizKuru;
    DovizTanim karsiDovizTanim;
    decimal karsiDovizKuru;
    decimal yerelBorcTutar;
    decimal yerelAlacakTutar;
    bool motorIslendi;
    decimal uygulananYerelBorcTutar;
    decimal uygulananYerelAlacakTutar;

    [Size(20)]
    [Indexed(Unique = true)]
    [Appearance("ED_KasaCariBankaHareketleri_FisNo", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Fiş No")]
    public string FisNo
    {
        get => fisNo;
        set => SetPropertyValue(nameof(FisNo), ref fisNo, value);
    }

    [RuleRequiredField("RuleRequired_KasaCariBankaHareketleri_FisTarihi", DefaultContexts.Save, "Lütfen Fiş Tarihini Giriniz...")]
    [Appearance("ED_KasaCariBankaHareketleri_FisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Fiş Tarihi")]
    // Mükerrer-kayıt kontrolü (OnSaving) ve ekstre/rapor sorguları bu üç alanın
    // kombinasyonuyla filtreliyor — indekssiz olduğu için tablo büyüdükçe her
    // "Kaydet" yavaşlıyordu (22.08.2026 denetimi G10).
    [Indexed("KaynakHesap;KarsiHesap")]
    public DateTime FisTarihi
    {
        get => fisTarihi;
        set => SetPropertyValue(nameof(FisTarihi), ref fisTarihi, value);
    }

    [RuleRequiredField("RuleRequired_KasaCariBankaHareketleri_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Seçiniz...")]
    [Appearance("ED_KasaCariBankaHareketleri_FisTuruTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Fiş Türü")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    [Size(13)]
    [XafDisplayName("Belge No")]
    public string BelgeNo
    {
        get => belgeNo;
        set => SetPropertyValue(nameof(BelgeNo), ref belgeNo, value);
    }

    [XafDisplayName("Belge Tarihi")]
    public DateTime BelgeTarihi
    {
        get => belgeTarihi;
        set => SetPropertyValue(nameof(BelgeTarihi), ref belgeTarihi, value);
    }

    [XafDisplayName("Vade Tarihi")]
    public DateTime VadeTarihi
    {
        get => vadeTarihi;
        set => SetPropertyValue(nameof(VadeTarihi), ref vadeTarihi, value);
    }

    // Yalnızca Cari Tahsilat/Ödeme/Virman/Açılış ekranlarında gösterilir (bkz.
    // Model.DesignedDiffs.xafml) — Kasa/Banka ekranlarında zaten hangi Kasa/Banka
    // hesabının kullanıldığı belli olduğundan gizlenir.
    [XafDisplayName("Ödeme Şekli")]
    public OdemeSekli OdemeSekli
    {
        get => odemeSekli;
        set => SetPropertyValue(nameof(OdemeSekli), ref odemeSekli, value);
    }

    [RuleRequiredField("RuleRequired_KasaCariBankaHareketleri_KaynakHesap", DefaultContexts.Save, "Lütfen Kaynak Hesabı Seçiniz...")]
    [Appearance("ED_KasaCariBankaHareketleri_KaynakHesap", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Kaynak Hesap")]
    public Hesap KaynakHesap
    {
        get => kaynakHesap;
        set => SetPropertyValue(nameof(KaynakHesap), ref kaynakHesap, value);
    }

    [RuleRequiredField("RuleRequired_KasaCariBankaHareketleri_KarsiHesap", DefaultContexts.Save, "Lütfen Karşı Hesabı Seçiniz...")]
    [Appearance("ED_KasaCariBankaHareketleri_KarsiHesap", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Karşı Hesap")]
    public Hesap KarsiHesap
    {
        get => karsiHesap;
        set => SetPropertyValue(nameof(KarsiHesap), ref karsiHesap, value);
    }

    // KaynakHesap'ın KENDİ döviz cinsinden Borç tutarı. Her 12 modül ekranında TEK
    // görünen tutar alanı budur (AlacakTutar hep gizli/otomatik) — aşağıdaki set'te
    // aynı ham tutarla AlacakTutar'a da otomatik kopyalanır (aynı döviz cinsi
    // varsayımıyla; farklı döviz cinsi durumunda kullanıcı AlacakTutar'ı elle
    // düzeltebilir, bu tek-yönlü kopyalama ikinci bir manuel değişikliği EZMEZ).
    // Yalnızca YENİ kayıtta VEYA Açılış Fişi'nde değiştirilebilir (bkz. Appearance
    // Criteria) — diğer fiş türlerinde kaydedilmiş bir fişin tutarı değiştirilemez,
    // yanlış girilen fiş SİLİNİP yeniden girilir (4-ajanlı testte bulunan "düzenlemede
    // bakiye motoru atlanıyor" hatasının kökten çözümü). Açılış Fişi bir "işlem" değil,
    // kurulum/mutabakat sırasında düzeltilmesi normal olan referans veri olduğu için
    // kullanıcı isteğiyle (2026-08-21) istisna tutuldu — düzenleme sonrası bakiye
    // motorunun doğru çalışması için ObjectSaving()'de "fark" mekanizması eklendi
    // (bkz. UygulananYerelBorcTutar/UygulananYerelAlacakTutar).
    [RuleValueComparison("Rule_KasaCariBankaHareketleri_BorcTutar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen Tutarı sıfırdan büyük giriniz.")]
    [Appearance("ED_KasaCariBankaHareketleri_BorcTutar", Enabled = false,
        Criteria = "!(IsNewObject(this)) And Not ([FisTuruTanim.FisTuruKodu] = 'ACILIS')", Context = "DetailView")]
    [XafDisplayName("Borç Tutar")]
    public decimal BorcTutar
    {
        get => borcTutar;
        set
        {
            SetPropertyValue(nameof(BorcTutar), ref borcTutar, value);
            YerelBorcTutar = Math.Round(value * (DovizKuru == 0 ? 1 : DovizKuru), 2, MidpointRounding.AwayFromZero);
            if (!IsLoading && value > 0 && alacakTutar == 0) AlacakTutar = value;
        }
    }

    // KarsiHesap'ın KENDİ döviz cinsinden Alacak tutarı.
    [RuleValueComparison("Rule_KasaCariBankaHareketleri_AlacakTutar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen Alacak Tutarını sıfırdan büyük giriniz.")]
    [XafDisplayName("Alacak Tutar")]
    public decimal AlacakTutar
    {
        get => alacakTutar;
        set
        {
            SetPropertyValue(nameof(AlacakTutar), ref alacakTutar, value);
            YerelAlacakTutar = Math.Round(value * (KarsiDovizKuru == 0 ? 1 : KarsiDovizKuru), 2, MidpointRounding.AwayFromZero);
        }
    }

    [XafDisplayName("Döviz Kodu (Kaynak)")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [RuleValueComparison("Rule_KasaCariBankaHareketleri_DovizKuru_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Döviz kuru bulunamadı — lütfen Döviz Kodu/Kuru bilgisini kontrol ediniz.")]
    [XafDisplayName("Döviz Kuru (Kaynak)")]
    public decimal DovizKuru
    {
        get => dovizKuru;
        set
        {
            SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
            YerelBorcTutar = Math.Round(BorcTutar * (value == 0 ? 1 : value), 2, MidpointRounding.AwayFromZero);
        }
    }

    [XafDisplayName("Döviz Kodu (Karşı)")]
    public DovizTanim KarsiDovizTanim
    {
        get => karsiDovizTanim;
        set => SetPropertyValue(nameof(KarsiDovizTanim), ref karsiDovizTanim, value);
    }

    [RuleValueComparison("Rule_KasaCariBankaHareketleri_KarsiDovizKuru_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Karşı taraf döviz kuru bulunamadı — lütfen Döviz Kodu/Kuru bilgisini kontrol ediniz.")]
    [XafDisplayName("Döviz Kuru (Karşı)")]
    public decimal KarsiDovizKuru
    {
        get => karsiDovizKuru;
        set
        {
            SetPropertyValue(nameof(KarsiDovizKuru), ref karsiDovizKuru, value);
            YerelAlacakTutar = Math.Round(AlacakTutar * (value == 0 ? 1 : value), 2, MidpointRounding.AwayFromZero);
        }
    }

    [Appearance("ED_KasaCariBankaHareketleri_YerelBorcTutar", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Yerel Borç Tutar")]
    public decimal YerelBorcTutar
    {
        get => yerelBorcTutar;
        set => SetPropertyValue(nameof(YerelBorcTutar), ref yerelBorcTutar, value);
    }

    [Appearance("ED_KasaCariBankaHareketleri_YerelAlacakTutar", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Yerel Alacak Tutar")]
    public decimal YerelAlacakTutar
    {
        get => yerelAlacakTutar;
        set => SetPropertyValue(nameof(YerelAlacakTutar), ref yerelAlacakTutar, value);
    }

    [Browsable(false)]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool MotorIslendi
    {
        get => motorIslendi;
        set => SetPropertyValue(nameof(MotorIslendi), ref motorIslendi, value);
    }

    // GuncelBakiye'ye EN SON uygulanan (fiilen işlenmiş) yerel tutarlar — Açılış Fişi
    // düzenlemesinde (bkz. BorcTutar Appearance Criteria) ObjectSaving()'in "fark"
    // hesaplayabilmesi için gerekli. YerelBorcTutar/YerelAlacakTutar kullanıcı ekranda
    // değer değiştirdiği an bellekte GÜNCELLENİR (henüz kaydedilmemiş olsa bile) —
    // bu yüzden "az önce ne uygulanmıştı" bilgisini AYRI saklamak zorunludur.
    [Browsable(false)]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public decimal UygulananYerelBorcTutar
    {
        get => uygulananYerelBorcTutar;
        set => SetPropertyValue(nameof(UygulananYerelBorcTutar), ref uygulananYerelBorcTutar, value);
    }

    [Browsable(false)]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public decimal UygulananYerelAlacakTutar
    {
        get => uygulananYerelAlacakTutar;
        set => SetPropertyValue(nameof(UygulananYerelAlacakTutar), ref uygulananYerelAlacakTutar, value);
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        // NOT: DovizTanim/KarsiDovizTanim ataması burada değişiklik varsa KENDİ
        // OnChanged dalını (aşağıda) tetikler — DovizKuruGuncelle'ı burada AYRICA
        // çağırmak (eski haliyle olduğu gibi) sorguyu gereksiz yere 2 katına çıkarırdı.
        if (propertyName == nameof(KaynakHesap) && newValue != null && !IsLoading)
            DovizTanim = ((Hesap)newValue).DovizTanim;
        if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            DovizKuruGuncelle((DovizTanim)newValue, kaynak: true);

        if (propertyName == nameof(KarsiHesap) && newValue != null && !IsLoading)
            KarsiDovizTanim = ((Hesap)newValue).DovizTanim;
        if (propertyName == nameof(KarsiDovizTanim) && newValue != null && !IsLoading)
            DovizKuruGuncelle((DovizTanim)newValue, kaynak: false);

        // Fiş Tarihi değişirse (yeni kayıtta serbestçe düzenlenebilir), o tarihe göre
        // kur yeniden hesaplanır — aksi halde eski tarihin kuru donuk kalır.
        if (propertyName == nameof(FisTarihi) && !IsLoading)
        {
            if (DovizTanim != null) DovizKuruGuncelle(DovizTanim, kaynak: true);
            if (KarsiDovizTanim != null) DovizKuruGuncelle(KarsiDovizTanim, kaynak: false);
        }
    }

    void DovizKuruGuncelle(DovizTanim doviz, bool kaynak)
    {
        decimal kur;
        if (doviz.DovizKodu == "TRY")
        {
            kur = 1;
        }
        else
        {
            DateTime fisTarihiGunu = FisTarihi.Date;
            // Tam gün eşleşmesi bulunamazsa (hafta sonu/resmi tatil — TCMB o gün kur
            // yayınlamaz) en son yayınlanan kuru (<=) fallback olarak kullan; hiç yoksa
            // 0 kalır ve mevcut DovizKuru>0 kuralı kaydı bloke eder (22.08.2026 denetimi
            // G15).
            kur = new XPQuery<DovizGunlukKurD>(Session)
                .Where(x => x.DovizTanim == doviz && x.KurTarihi <= fisTarihiGunu)
                .OrderByDescending(x => x.KurTarihi)
                .Select(x => x.DovizSatis)
                .FirstOrDefault();
        }
        if (kaynak) DovizKuru = kur; else KarsiDovizKuru = kur;
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this))
        {
            // 4-ajanlı testte bulunan "mükerrer kayıt" riski: aynı gün, aynı fiş türü,
            // aynı iki hesap, aynı tutarla başka bir hareket zaten varsa muhtemelen
            // aynı işlem yanlışlıkla iki kez (ör. hem Kasa hem Cari ekranından) giriliyordur.
            KasaCariBankaHareketleri mukerrer = Session.FindObject<KasaCariBankaHareketleri>(
                CriteriaOperator.FromLambda<KasaCariBankaHareketleri>(x =>
                    x.Oid != Oid &&
                    x.FisTuruTanim == FisTuruTanim &&
                    x.FisTarihi == FisTarihi &&
                    x.KaynakHesap == KaynakHesap &&
                    x.KarsiHesap == KarsiHesap &&
                    x.BorcTutar == BorcTutar));
            if (mukerrer != null)
                throw new UserFriendlyException(
                    $"Aynı tarih, aynı hesaplar ve aynı tutarla bir hareket zaten kayıtlı (Fiş No: {mukerrer.FisNo}). " +
                    "Mükerrer kayıt olabilir — lütfen kontrol ediniz. Gerçekten farklı bir işlemse tutarı/tarihi birebir aynı girmeyiniz.");

            if (string.IsNullOrEmpty(FisNo) && FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                FisNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, FisTarihi);
            }
        }
        base.OnSaving();
    }

    public override void ObjectSaving()
    {
        if (!MotorIslendi)
        {
            if (KaynakHesap == null)
                throw new UserFriendlyException("Lütfen Kaynak Hesabı seçiniz.");
            if (KarsiHesap == null)
                throw new UserFriendlyException("Lütfen Karşı Hesabı seçiniz.");

            // Cari Açılış Fişi'nde Tedarikçi (bize alacaklı) seçilirse yön ters çevrilir:
            // ekran Cari'yi hep KaynakHesap/Borç tarafına sabitler (bkz. xafml), ama bir
            // Tedarikçinin açılış bakiyesi muhasebesel olarak Alacak yönünde olmalıdır
            // (biz onlara borçluyuz) — 4-ajanlı testte bulunan hata. KaynakHesap/KarsiHesap
            // property setter'ları üzerinden takas edilir ki OnChanged zaten doğru
            // Döviz/Kur/Yerel Tutar yeniden hesaplamasını KENDİLİĞİNDEN yapsın.
            if (FisTuruTanim?.FisTuruKodu == "ACILIS" && KaynakHesap is CariHesapTanim kaynakCari
                && kaynakCari.CariHesapSinifi == CariHesapTipi.Tedarikci)
            {
                Hesap eskiKaynak = KaynakHesap;
                Hesap eskiKarsi = KarsiHesap;
                KaynakHesap = eskiKarsi;
                KarsiHesap = eskiKaynak;
            }

            // Tüm Hesap türleri TEK ve evrensel işaretli kuralla işlenir (bkz. Hesap.cs):
            // Borç KaynakHesap'ı artırır, Alacak KarsiHesap'ı azaltır — hesabın Cari/Kasa/
            // Banka ya da Müşteri/Tedarikçi olması bu formülü DEĞİŞTİRMEZ (çift-kayıt
            // muhasebesinin temel kuralı). Bir Tedarikçi'nin "doğal" bakiye yönü (biz
            // onlara borçluyuz) yalnızca AÇILIŞ fişindeki Kaynak/Karşı TAKASI ile (yukarıda)
            // sağlanır; işlem bazında tipe göre işaret çevirme daha önce burada vardı ama
            // raporların (Hesap Ekstresi, Tüm Cariler) bu özel durumu asla replike
            // etmemesi yüzünden kart/ekstre çelişkisine yol açtığı 22.08.2026 tarihli
            // denetimde tespit edildi — kaldırıldı (DUZELTME_GOREVLERI.md G1).
            KaynakHesap.GuncelBakiye += YerelBorcTutar;
            KarsiHesap.GuncelBakiye -= YerelAlacakTutar;

            UygulananYerelBorcTutar = YerelBorcTutar;
            UygulananYerelAlacakTutar = YerelAlacakTutar;
            MotorIslendi = true;
        }
        else if (YerelBorcTutar != UygulananYerelBorcTutar || YerelAlacakTutar != UygulananYerelAlacakTutar)
        {
            // Kayıt zaten işlenmiş (MotorIslendi=true) ama tutar değişmiş — bu yalnızca
            // Açılış Fişi'nde mümkün (bkz. BorcTutar Appearance Criteria, diğer fiş
            // türlerinde bu alan zaten kilitli). Eski etkiyi geri alıp FARKI uygula —
            // KaynakHesap/KarsiHesap kendisi bu ekranlarda değiştirilemediği için
            // (kendi Appearance kuralları hâlâ kilitli) aynı hesaplara işlenir.
            KaynakHesap.GuncelBakiye += (YerelBorcTutar - UygulananYerelBorcTutar);
            KarsiHesap.GuncelBakiye -= (YerelAlacakTutar - UygulananYerelAlacakTutar);

            UygulananYerelBorcTutar = YerelBorcTutar;
            UygulananYerelAlacakTutar = YerelAlacakTutar;
        }
        base.ObjectSaving();
    }

    public override void ObjectDeleting()
    {
        if (MotorIslendi)
        {
            // UygulananYerelBorcTutar/UygulananYerelAlacakTutar kullanılır — YerelBorcTutar/
            // YerelAlacakTutar DEĞİL: kullanıcı Açılış Fişi'nde tutarı ekranda değiştirip
            // KAYDETMEDEN silme yaparsa, Yerel* alanları henüz commit edilmemiş (yanlış)
            // değeri taşır; Uygulanan* alanları ise her zaman GERÇEKTEN GuncelBakiye'ye
            // işlenmiş son tutarı yansıtır (yalnızca başarılı ObjectSaving()'de güncellenir).
            // ObjectSaving()'deki evrensel işaretli kural burada AYNEN (ters) tekrarlanır.
            KaynakHesap.GuncelBakiye -= UygulananYerelBorcTutar;
            KarsiHesap.GuncelBakiye += UygulananYerelAlacakTutar;
        }
        base.ObjectDeleting();
    }
}
