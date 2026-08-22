/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : GenelParametre.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Sistem geneli parametreler (singleton). Şimdilik yalnızca
 *                    miktar/tutar/kur ondalık basamak ayarlarını içerir — eski
 *                    projeden uyarlandı.
 * ****************************************************************************
 */

using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Genel Parametre Tanımlama")]
public class GenelParametre : BaseClass
{
    public GenelParametre(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            XPCollection<GenelParametre> mevcutlar = new XPCollection<GenelParametre>(Session);
            if (mevcutlar.Count >= 1)
            {
                this.CancelEdit();
                GenelParametre mevcut = mevcutlar[0];
                Session.DropChanges();
                Session.Reload(mevcut);
            }
        }
    }

    // Tek-satır garantisi: değer her zaman 1'dir; benzersiz indeks DB seviyesinde
    // ikinci bir satır eklenmesini engeller. AfterConstruction'daki Count kontrolü
    // yalnızca UI akışını (yeni kayıt yerine mevcuda yönlendirme) sağlar — asıl
    // garanti burada, çünkü Count kontrolü tek başına DB'de zaten 2+ satır varsa
    // yetersiz kalır (denetim raporu O4).
    int tekKayitAnahtari = 1;

    [Indexed(Unique = true)]
    [Browsable(false)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [VisibleInDashboards(false)]
    public int TekKayitAnahtari
    {
        get => tekKayitAnahtari;
        set => SetPropertyValue(nameof(TekKayitAnahtari), ref tekKayitAnahtari, value);
    }

    OndalikBasamakSayisi miktarOndalikMaski = OndalikBasamakSayisi.Basamak2;
    OndalikBasamakSayisi tutarOndalikMaski = OndalikBasamakSayisi.Basamak2;
    OndalikBasamakSayisi kurOndalikMaski = OndalikBasamakSayisi.Basamak2;

    [XafDisplayName("Miktar Ondalık Maskı")]
    public OndalikBasamakSayisi MiktarOndalikMaski
    {
        get => miktarOndalikMaski;
        set => SetPropertyValue(nameof(MiktarOndalikMaski), ref miktarOndalikMaski, value);
    }

    [XafDisplayName("Tutar Ondalık Maskı")]
    public OndalikBasamakSayisi TutarOndalikMaski
    {
        get => tutarOndalikMaski;
        set => SetPropertyValue(nameof(TutarOndalikMaski), ref tutarOndalikMaski, value);
    }

    [XafDisplayName("Kur Ondalık Maskı")]
    public OndalikBasamakSayisi KurOndalikMaski
    {
        get => kurOndalikMaski;
        set => SetPropertyValue(nameof(KurOndalikMaski), ref kurOndalikMaski, value);
    }

    // GenelParametreOkuyucu'nun process-genelindeki önbelleğini geçersiz kılar
    // (22.08.2026 denetimi G11) — değişiklik başarıyla kaydedildikten sonra
    // bir sonraki okuma güncel değerleri görsün diye.
    protected override void OnSaved()
    {
        base.OnSaved();
        GenelParametreOkuyucu.OnbellegiTemizle();
    }
}
