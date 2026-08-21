/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaCariBankaHareketleriTumHareketlerYonlendirmeController.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Tüm Cari/Kasa/Banka Hareketleri" ekranlarında bir satıra çift
 *                    tıklandığında, genel (Fiş Türüne özel olmayan) DetailView yerine
 *                    o satırın KENDİ Fiş Türüne ait DetailView'ının açılmasını sağlar
 *                    (ör. bir Tahsilat satırı → CariTahsilatDetailView).
 *
 *                    Eski ERP'deki (D:\2025\SofasisERP) ProcessCariHesapHareketleriListViewRowController
 *                    ve ProcessKasaBankaHesapHareketleriListViewRowController sınıflarındaki
 *                    "ListViewProcessCurrentObjectController.CustomProcessSelectedItem" deseninin
 *                    aynısı — tek fark, eski projede FisTuruTanim.ViewName alanı doğrudan hedef
 *                    ListView adını taşıyordu (List→Detail string replace yeterliydi). Bu projede
 *                    Kasa/Banka/Cari TEK sınıfta birleşik olduğundan ve aynı FisTuruKodu (ör.
 *                    "TAHSIL") üç ayrı ekranda (Kasa/Banka/Cari) kullanıldığından, hedef DetailView
 *                    FisTuruKodu + bu ekranın kendi ailesi (Cari/Kasa/Banka, her alt sınıfta sabit)
 *                    birleştirilerek bulunuyor.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Controllers.Process;

public abstract class KasaCariBankaHareketleriTumHareketlerYonlendirmeControllerBase : ViewController
{
    static readonly Dictionary<string, string> FisTuruKoduEkAdi = new()
    {
        { "ACILIS", "Acilis" },
        { "TAHSIL", "Tahsilat" },
        { "ODEME", "Odeme" },
        { "VIRMAN", "Virman" },
    };

    readonly string aileAdi;
    ListViewProcessCurrentObjectController listProcessController;

    protected KasaCariBankaHareketleriTumHareketlerYonlendirmeControllerBase(string targetViewId, string aileAdi)
    {
        TargetViewId = targetViewId;
        this.aileAdi = aileAdi;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        listProcessController = Frame.GetController<ListViewProcessCurrentObjectController>();
        if (listProcessController != null)
            listProcessController.CustomProcessSelectedItem += ListProcessController_CustomProcessSelectedItem;
    }

    void ListProcessController_CustomProcessSelectedItem(object sender, CustomProcessListViewSelectedItemEventArgs e)
    {
        KasaCariBankaHareketleri hareket = (KasaCariBankaHareketleri)e.InnerArgs.CurrentObject;
        string fisTuruKodu = hareket.FisTuruTanim?.FisTuruKodu;
        if (fisTuruKodu == null || !FisTuruKoduEkAdi.TryGetValue(fisTuruKodu, out string ekAdi))
            return;

        string hedefDetailViewId = $"KasaCariBankaHareketleri_{aileAdi}{ekAdi}DetailView";
        IObjectSpace objectSpace = Application.CreateObjectSpace(hareket.GetType());
        e.InnerArgs.ShowViewParameters.CreatedView = Application.CreateDetailView(
            objectSpace, hedefDetailViewId, true, objectSpace.GetObject(hareket));
        e.Handled = true;
    }

    protected override void OnDeactivated()
    {
        if (listProcessController != null)
            listProcessController.CustomProcessSelectedItem -= ListProcessController_CustomProcessSelectedItem;
        listProcessController = null;
        base.OnDeactivated();
    }
}

public class KasaCariBankaHareketleriCariTumHareketlerYonlendirmeController
    : KasaCariBankaHareketleriTumHareketlerYonlendirmeControllerBase
{
    public KasaCariBankaHareketleriCariTumHareketlerYonlendirmeController()
        : base("KasaCariBankaHareketleri_CariTumHareketlerListView", "Cari") { }
}

public class KasaCariBankaHareketleriKasaTumHareketlerYonlendirmeController
    : KasaCariBankaHareketleriTumHareketlerYonlendirmeControllerBase
{
    public KasaCariBankaHareketleriKasaTumHareketlerYonlendirmeController()
        : base("KasaCariBankaHareketleri_KasaTumHareketlerListView", "Kasa") { }
}

public class KasaCariBankaHareketleriBankaTumHareketlerYonlendirmeController
    : KasaCariBankaHareketleriTumHareketlerYonlendirmeControllerBase
{
    public KasaCariBankaHareketleriBankaTumHareketlerYonlendirmeController()
        : base("KasaCariBankaHareketleri_BankaTumHareketlerListView", "Banka") { }
}
