/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : HesapEkstresiRaporuController.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari/Kasa/Banka "Tüm Hareketler" ekranlarına "Hesap Ekstresi"
 *                    rapor aksiyonu ekler — eski ERP'nin CariHesapEkstresiRaporuController
 *                    deseninden (PopupWindowShowAction + kriter ekranı + ReportPreviewContext
 *                    köprüsü) uyarlandı, TEK jenerik taban sınıf + 3 ince alt sınıf olarak
 *                    (bu oturumda daha önce KasaCariBankaHareketleriTumHareketlerYonlendirmeController
 *                    için kullanılan aynı desen). Kardeş sınıflar arasında yalnızca kriter
 *                    tipi (Hesap özelliğinin gerçek tipi Cari/Kasa/Banka'ya göre değişiyor)
 *                    ve hedef ListView farklı — bkz. dosya sonundaki 3 alt sınıf.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Services;
using System;
using System.Linq;

namespace SofasisERP.Module.Controllers.Process;

public abstract class HesapEkstresiRaporuControllerBase<TKriteri> : ObjectViewController<ListView, KasaCariBankaHareketleri>
    where TKriteri : class
{
    readonly string reportDisplayName;
    readonly string raporBasligi;
    readonly Func<TKriteri, Hesap> hesapSecici;
    readonly Func<TKriteri, DateTime> baslangicSecici;
    readonly Func<TKriteri, DateTime> bitisSecici;
    readonly PopupWindowShowAction ekstreAction;

    protected HesapEkstresiRaporuControllerBase(string targetViewId, string actionCaption, string reportDisplayName,
        string raporBasligi, Func<TKriteri, Hesap> hesapSecici, Func<TKriteri, DateTime> baslangicSecici, Func<TKriteri, DateTime> bitisSecici)
    {
        TargetViewId = targetViewId;
        this.reportDisplayName = reportDisplayName;
        this.raporBasligi = raporBasligi;
        this.hesapSecici = hesapSecici;
        this.baslangicSecici = baslangicSecici;
        this.bitisSecici = bitisSecici;

        ekstreAction = new PopupWindowShowAction(this, GetType().Name, PredefinedCategory.Reports)
        {
            Caption = actionCaption,
            ImageName = "Action_Report_Object_Inplace_Preview"
        };
        ekstreAction.CustomizePopupWindowParams += EkstreAction_CustomizePopupWindowParams;
        ekstreAction.Execute += EkstreAction_Execute;
    }

    void EkstreAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        IObjectSpace popupObjectSpace = Application.CreateObjectSpace(typeof(TKriteri));
        TKriteri kriter = popupObjectSpace.CreateObject<TKriteri>();
        popupObjectSpace.CommitChanges();
        e.View = Application.CreateDetailView(popupObjectSpace, kriter);
        e.DialogController.SaveOnAccept = false;
    }

    void EkstreAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        TKriteri kriter = (TKriteri)e.PopupWindow.View.CurrentObject;
        Hesap hesapNested = hesapSecici(kriter);
        if (hesapNested == null)
            throw new UserFriendlyException("Lütfen bir Hesap seçiniz...");

        Hesap hesap = (Hesap)ObjectSpace.GetObject(hesapNested);
        DateTime baslangic = baslangicSecici(kriter);
        DateTime bitis = bitisSecici(kriter);

        // Devreden bakiye: dönem başlangıcından ÖNCEki tüm hareketlerin, seçili Hesap
        // perspektifinden net toplamı. UygulananYerel* kullanılır (fiilen GuncelBakiye'ye
        // işlenmiş tutar — bkz. KasaCariBankaHareketleri.ObjectSaving).
        decimal acilisBakiyesi = new XPQuery<KasaCariBankaHareketleri>(((XPObjectSpace)ObjectSpace).Session)
            .Where(x => (x.KaynakHesap == hesap || x.KarsiHesap == hesap) && x.FisTarihi < baslangic)
            .ToList()
            .Sum(x => (x.KaynakHesap == hesap ? x.UygulananYerelBorcTutar : 0m) - (x.KarsiHesap == hesap ? x.UygulananYerelAlacakTutar : 0m));

        ReportServiceController reportController = Frame.GetController<ReportServiceController>();
        if (reportController == null) return;

        IReportStorage reportStorage = Application.ServiceProvider.GetRequiredService<IReportStorage>();
        using IObjectSpace reportObjectSpace = Application.CreateObjectSpace(typeof(ReportDataV2));
        ReportDataV2 reportData = reportObjectSpace.GetObjectsQuery<ReportDataV2>()
            .FirstOrDefault(r => r.DisplayName == reportDisplayName);
        if (reportData == null)
            throw new UserFriendlyException($"'{reportDisplayName}' raporu kayıtlı değil.");
        string handle = reportStorage.GetReportContainerHandle(reportData);

        ReportPreviewContext previewContext = Application.ServiceProvider.GetRequiredService<ReportPreviewContext>();
        previewContext.DataSource = null;
        previewContext.ParameterValues.Clear();
        previewContext.ParameterValues["RaporBasligi"] = raporBasligi;
        previewContext.ParameterValues["HesapAdi"] = hesap.HesapAdi;
        previewContext.ParameterValues["DovizKodu"] = hesap.DovizTanim?.DovizKodu ?? "";
        previewContext.ParameterValues["BaslangicTarihi"] = baslangic;
        previewContext.ParameterValues["BitisTarihi"] = bitis;
        previewContext.ParameterValues["AcilisBakiyesi"] = acilisBakiyesi;
        previewContext.ParameterValues["SeciliHesapOid"] = hesap.Oid;

        CriteriaOperator kriterOperator = CriteriaOperator.And(
            CriteriaOperator.Or(new BinaryOperator("KaynakHesap", hesap), new BinaryOperator("KarsiHesap", hesap)),
            new BinaryOperator("FisTarihi", baslangic, BinaryOperatorType.GreaterOrEqual),
            new BinaryOperator("FisTarihi", bitis, BinaryOperatorType.LessOrEqual));

        reportController.ShowPreview(handle, kriterOperator);
    }
}

public class CariHesapEkstresiRaporuController : HesapEkstresiRaporuControllerBase<CariHesapEkstresiKriteri>
{
    // Yalnızca "Tüm Cari Hareketleri" değil, tek tek Açılış/Tahsilat/Ödeme/Virman Fişi
    // ekranlarında da rapor butonu görünsün diye TargetViewId'ye (noktalı virgülle
    // ayrılmış) tüm 5 ListView eklendi — kullanıcı isteği (2026-08-21).
    public CariHesapEkstresiRaporuController()
        : base(
            "KasaCariBankaHareketleri_CariAcilisListView;" +
            "KasaCariBankaHareketleri_CariTahsilatListView;" +
            "KasaCariBankaHareketleri_CariOdemeListView;" +
            "KasaCariBankaHareketleri_CariVirmanListView;" +
            "KasaCariBankaHareketleri_CariTumHareketlerListView",
            "Cari Hesap Ekstresi", "Cari Hesap Ekstresi", "Cari Hesap Ekstresi",
            k => k.Hesap, k => k.BaslangicTarihi, k => k.BitisTarihi)
    { }
}

public class KasaHesapEkstresiRaporuController : HesapEkstresiRaporuControllerBase<KasaHesapEkstresiKriteri>
{
    public KasaHesapEkstresiRaporuController()
        : base(
            "KasaCariBankaHareketleri_KasaAcilisListView;" +
            "KasaCariBankaHareketleri_KasaTahsilatListView;" +
            "KasaCariBankaHareketleri_KasaOdemeListView;" +
            "KasaCariBankaHareketleri_KasaVirmanListView;" +
            "KasaCariBankaHareketleri_KasaTumHareketlerListView",
            "Kasa Ekstresi", "Kasa Ekstresi", "Kasa Ekstresi",
            k => k.Hesap, k => k.BaslangicTarihi, k => k.BitisTarihi)
    { }
}

public class BankaHesapEkstresiRaporuController : HesapEkstresiRaporuControllerBase<BankaHesapEkstresiKriteri>
{
    public BankaHesapEkstresiRaporuController()
        : base(
            "KasaCariBankaHareketleri_BankaAcilisListView;" +
            "KasaCariBankaHareketleri_BankaTahsilatListView;" +
            "KasaCariBankaHareketleri_BankaOdemeListView;" +
            "KasaCariBankaHareketleri_BankaVirmanListView;" +
            "KasaCariBankaHareketleri_BankaTumHareketlerListView",
            "Banka Ekstresi", "Banka Ekstresi", "Banka Ekstresi",
            k => k.Hesap, k => k.BaslangicTarihi, k => k.BitisTarihi)
    { }
}
