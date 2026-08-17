/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FisTuruVarsayilanDegeri.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Bir Fiş Türü'ne, herhangi bir hedef nesne tipi (KDV,
 *                    Döviz, Birim, ileride eklenecek başka tipler) için
 *                    varsayılan değer tanımlamayı sağlayan genel amaçlı alt
 *                    tablo (eski projeden aynen taşındı). HedefTip seçimi
 *                    IFisTuruVarsayilanHedefi arayüzünü uygulayan sınıflarla
 *                    sınırlıdır; VarsayilanDeger bu tipin gerçek bir kaydına
 *                    XPWeakReference ile (tip güvenli, polimorfik) referans
 *                    verir. Aktif=false olan satırlar otomatik uygulamada
 *                    atlanır.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[XafDisplayName("Fiş Türü Varsayılan Değeri")]
public class FisTuruVarsayilanDegeri : BaseClass
{
    public FisTuruVarsayilanDegeri(Session session) : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            Aktif = true;
            VarsayilanDeger = new XPWeakReference(Session);
        }
    }

    Type hedefTip;
    XPWeakReference varsayilanDeger;
    bool aktif;
    FisTuruTanim fisTuruTanim;

    [Association("FisTuruTanim-FisTuruVarsayilanDegerleri")]
    [VisibleInListView(false)]
    [XafDisplayName("Fiş Türü")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    [Indexed("FisTuruTanim", Unique = true)]
    [XafDisplayName("Hedef Nesne Tipi"), ToolTip("Varsayılan değerin uygulanacağı alan tipini seçiniz")]
    [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
    [TypeConverter(typeof(FisTuruVarsayilanHedefTipiConverter))]
    [RuleRequiredField("RuleRequired_FisTuruVarsayilanDegeri_HedefTip", DefaultContexts.Save, "Lütfen Hedef Nesne Tipini Seçiniz...")]
    public Type HedefTip
    {
        get => hedefTip;
        set
        {
            bool degisti = hedefTip != value;
            SetPropertyValue(nameof(HedefTip), ref hedefTip, value);
            // Tip değiştiğinde eski seçili değer artık yeni tiple uyumsuz olabilir; tutarsız
            // referans bırakmamak için temizlenir (kullanıcı yeni tipe göre yeniden seçer).
            if (degisti && VarsayilanDeger?.Target != null && (value == null || VarsayilanDeger.Target.GetType() != value))
            {
                VarsayilanDeger.Target = null;
                OnChanged(nameof(VarsayilanDegerNesne));
            }
        }
    }

    [Aggregated]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [XafDisplayName("Varsayılan Değer")]
    public XPWeakReference VarsayilanDeger
    {
        get => varsayilanDeger;
        set => SetPropertyValue(nameof(VarsayilanDeger), ref varsayilanDeger, value);
    }

    [XafDisplayName("Seçili Değer"), ToolTip("HedefTip'e göre seçilen kayıt"), VisibleInDetailView(false)]
    public string VarsayilanDegerGosterim
        => VarsayilanDeger?.Target != null ? CaptionHelper.GetDisplayText(VarsayilanDeger.Target) : "(Seçilmedi)";

    // NonPersistent: HedefTip'e göre değişen tipte gerçek referans tutan XPWeakReference'ın
    // (VarsayilanDeger) DetailView'da düzenlenebilir tek karşılığı. Blazor'da özel bir
    // PropertyEditor (SofasisERP.Blazor.Server\Editors\FisTuruVarsayilanDegerNesnePropertyEditor)
    // bu alanı HedefTip'e göre dinamik olarak dolan bir ComboBox olarak render eder.
    [NonPersistent]
    [VisibleInListView(false)]
    [XafDisplayName("Seçili Değer"), ToolTip("Hedef Nesne Tipi'ne göre kayıt listesinden seçiminizi yapınız")]
    public object VarsayilanDegerNesne
    {
        get => VarsayilanDeger?.Target;
        set
        {
            if (VarsayilanDeger == null)
                VarsayilanDeger = new XPWeakReference(Session);
            if (!ReferenceEquals(VarsayilanDeger.Target, value))
            {
                VarsayilanDeger.Target = value;
                OnChanged(nameof(VarsayilanDegerNesne));
            }
        }
    }

    [XafDisplayName("Aktif mi?"), ToolTip("Pasif satırlar yeni kayıt oluşumunda uygulanmaz")]
    public bool Aktif
    {
        get => aktif;
        set => SetPropertyValue(nameof(Aktif), ref aktif, value);
    }
}
