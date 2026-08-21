/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariAdresTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : CariHesapTanim'in Aggregated detayı — kullanıcı isteği
 *                    (2026-08-18): eski ERP'deki sabit Ev/İş Adresi ikilisi
 *                    yerine Adres Tipi'ne göre esnek çok satırlı yapı (Fatura/
 *                    Ev/Şirket). Alan şeması eski `AdresTanim.cs`'den (Ülke/
 *                    Şehir cascading dropdown) — İlçe eski projede YOKTU, biz
 *                    zaten Ülke/Şehir/İlçe hiyerarşisini kurduğumuz için ekledik.
 *                    Saf Detail — [DefaultClassOptions] BİLEREK yok, yalnızca
 *                    CariHesapTanim'in Aggregated koleksiyonundan düzenlenir.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(AdresTipi))]
[XafDisplayName("Cari Adres Tanımlama")]
public class CariAdresTanim : BaseClass
{
    public CariAdresTanim(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            UlkeTanim = Session.FindObject<UlkeTanim>(
                CriteriaOperator.FromLambda<UlkeTanim>(x => x.IsVarsayilan));
        }
    }

    CariAdresTipi adresTipi;
    string adres1;
    string adres2;
    string postaKodu;
    UlkeTanim ulkeTanim;
    SehirTanim sehirTanim;
    IlceTanim ilceTanim;
    CariHesapTanim cariHesapTanim;

    [Indexed(nameof(CariHesapTanim), Unique = true)]
    [XafDisplayName("Adres Tipi")]
    public CariAdresTipi AdresTipi
    {
        get => adresTipi;
        set => SetPropertyValue(nameof(AdresTipi), ref adresTipi, value);
    }

    [Size(100)]
    [XafDisplayName("Adres 1")]
    public string Adres1
    {
        get => adres1;
        set => SetPropertyValue(nameof(Adres1), ref adres1, value);
    }

    [Size(100)]
    [XafDisplayName("Adres 2")]
    public string Adres2
    {
        get => adres2;
        set => SetPropertyValue(nameof(Adres2), ref adres2, value);
    }

    [Size(10)]
    [XafDisplayName("Posta Kodu")]
    public string PostaKodu
    {
        get => postaKodu;
        set => SetPropertyValue(nameof(PostaKodu), ref postaKodu, value);
    }

    [ImmediatePostData]
    [XafDisplayName("Ülke")]
    public UlkeTanim UlkeTanim
    {
        get => ulkeTanim;
        set
        {
            bool degisti = ulkeTanim != value;
            SetPropertyValue(nameof(UlkeTanim), ref ulkeTanim, value);
            if (degisti && !IsLoading)
                SehirTanim = null;
        }
    }

    [ImmediatePostData]
    [DataSourceProperty("UlkeTanim.SehirTanims", DataSourcePropertyIsNullMode.SelectAll)]
    [XafDisplayName("Şehir")]
    public SehirTanim SehirTanim
    {
        get => sehirTanim;
        set
        {
            bool degisti = sehirTanim != value;
            SetPropertyValue(nameof(SehirTanim), ref sehirTanim, value);
            if (degisti && !IsLoading)
                IlceTanim = null;
        }
    }

    [DataSourceProperty("SehirTanim.IlceTanims", DataSourcePropertyIsNullMode.SelectAll)]
    [XafDisplayName("İlçe")]
    public IlceTanim IlceTanim
    {
        get => ilceTanim;
        set => SetPropertyValue(nameof(IlceTanim), ref ilceTanim, value);
    }

    [Association("CariHesapTanim-CariAdresleri")]
    [Appearance("InvisibleCariHesapTanim_AdresDetail", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
    [XafDisplayName("Cari Hesap")]
    public CariHesapTanim CariHesapTanim
    {
        get => cariHesapTanim;
        set => SetPropertyValue(nameof(CariHesapTanim), ref cariHesapTanim, value);
    }
}
