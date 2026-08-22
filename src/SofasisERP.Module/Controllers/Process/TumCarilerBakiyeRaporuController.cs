/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TumCarilerBakiyeRaporuController.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : CariHesapTanim_ListView'a "Tüm Cariler Borç/Alacak Raporu"
 *                    aksiyonu ekler. HesapEkstresiRaporuController'ın aksine tek
 *                    bir Hesap için değil, (opsiyonel Cari Hesap Kodu aralığıyla
 *                    filtrelenmiş) TÜM Cari hesaplar için özet satır üretir —
 *                    rapor canlı bir XPO CollectionDataSource'a değil, burada
 *                    önceden hesaplanan List<CariBakiyeSatiri>'ye bağlanır
 *                    (bkz. ReportPreviewContext, TumCarilerBakiyeRaporuBuilder).
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Reports;
using SofasisERP.Module.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SofasisERP.Module.Controllers.Process;

public class TumCarilerBakiyeRaporuController : ObjectViewController<ListView, CariHesapTanim>
{
    const string ReportDisplayName = "Tüm Cariler Borç/Alacak Raporu";
    readonly PopupWindowShowAction raporAction;

    public TumCarilerBakiyeRaporuController()
    {
        TargetViewId = "CariHesapTanim_ListView";

        raporAction = new PopupWindowShowAction(this, nameof(TumCarilerBakiyeRaporuController), PredefinedCategory.Reports)
        {
            Caption = "Tüm Cariler Borç/Alacak Raporu",
            ImageName = "Action_Report_Object_Inplace_Preview"
        };
        raporAction.CustomizePopupWindowParams += RaporAction_CustomizePopupWindowParams;
        raporAction.Execute += RaporAction_Execute;
    }

    void RaporAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        IObjectSpace popupObjectSpace = Application.CreateObjectSpace(typeof(TumCarilerBakiyeRaporuKriteri));
        TumCarilerBakiyeRaporuKriteri kriter = popupObjectSpace.CreateObject<TumCarilerBakiyeRaporuKriteri>();
        popupObjectSpace.CommitChanges();
        e.View = Application.CreateDetailView(popupObjectSpace, kriter);
        e.DialogController.SaveOnAccept = false;
    }

    void RaporAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        TumCarilerBakiyeRaporuKriteri kriter = (TumCarilerBakiyeRaporuKriteri)e.PopupWindow.View.CurrentObject;
        DateTime baslangic = kriter.BaslangicTarihi;
        DateTime bitis = kriter.BitisTarihi;
        string baslangicKodu = kriter.BaslangicCari != null ? ((CariHesapTanim)ObjectSpace.GetObject(kriter.BaslangicCari)).CariHesapKodu : null;
        string bitisKodu = kriter.BitisCari != null ? ((CariHesapTanim)ObjectSpace.GetObject(kriter.BitisCari)).CariHesapKodu : null;

        Session session = ((XPObjectSpace)ObjectSpace).Session;

        List<CariHesapTanim> cariler = new XPQuery<CariHesapTanim>(session).ToList();
        if (baslangicKodu != null)
            cariler = cariler.Where(c => string.Compare(c.CariHesapKodu, baslangicKodu, StringComparison.Ordinal) >= 0).ToList();
        if (bitisKodu != null)
            cariler = cariler.Where(c => string.Compare(c.CariHesapKodu, bitisKodu, StringComparison.Ordinal) <= 0).ToList();
        cariler = cariler.OrderBy(c => c.CariHesapKodu, StringComparer.Ordinal).ToList();

        // 22.08.2026 denetiminde bulunan hata (G8): TÜM hareketler .ToList() ile
        // belleğe çekilip her Cari için bellek-içi Where çalıştırılıyordu ->
        // O(Cari × Hareket), gerçek veride OutOfMemory/dakikalarca bekleme. Artık
        // dört ayrı sunucu-taraflı GROUP BY sorgusuyla (KaynakHesap/KarsiHesap ×
        // öncesi/dönem) Cari başına toplamlar önceden hesaplanıp sözlüğe alınır —
        // hiçbir yerde hareket satırları tam nesne olarak materialize edilmez.
        Dictionary<Guid, decimal> borcOncesi = new XPQuery<KasaCariBankaHareketleri>(session)
            .Where(x => x.MotorIslendi && x.FisTarihi < baslangic)
            .GroupBy(x => x.KaynakHesap)
            .Select(g => new { Hesap = g.Key, Toplam = g.Sum(x => x.UygulananYerelBorcTutar) })
            .ToDictionary(x => x.Hesap.Oid, x => x.Toplam);
        Dictionary<Guid, decimal> alacakOncesi = new XPQuery<KasaCariBankaHareketleri>(session)
            .Where(x => x.MotorIslendi && x.FisTarihi < baslangic)
            .GroupBy(x => x.KarsiHesap)
            .Select(g => new { Hesap = g.Key, Toplam = g.Sum(x => x.UygulananYerelAlacakTutar) })
            .ToDictionary(x => x.Hesap.Oid, x => x.Toplam);
        Dictionary<Guid, decimal> borcDonem = new XPQuery<KasaCariBankaHareketleri>(session)
            .Where(x => x.MotorIslendi && x.FisTarihi >= baslangic && x.FisTarihi <= bitis)
            .GroupBy(x => x.KaynakHesap)
            .Select(g => new { Hesap = g.Key, Toplam = g.Sum(x => x.UygulananYerelBorcTutar) })
            .ToDictionary(x => x.Hesap.Oid, x => x.Toplam);
        Dictionary<Guid, decimal> alacakDonem = new XPQuery<KasaCariBankaHareketleri>(session)
            .Where(x => x.MotorIslendi && x.FisTarihi >= baslangic && x.FisTarihi <= bitis)
            .GroupBy(x => x.KarsiHesap)
            .Select(g => new { Hesap = g.Key, Toplam = g.Sum(x => x.UygulananYerelAlacakTutar) })
            .ToDictionary(x => x.Hesap.Oid, x => x.Toplam);

        List<CariBakiyeSatiri> satirlar = new List<CariBakiyeSatiri>();
        foreach (CariHesapTanim cari in cariler)
        {
            decimal devreden = borcOncesi.GetValueOrDefault(cari.Oid) - alacakOncesi.GetValueOrDefault(cari.Oid);
            decimal donemBorc = borcDonem.GetValueOrDefault(cari.Oid);
            decimal donemAlacak = alacakDonem.GetValueOrDefault(cari.Oid);
            decimal kapanis = devreden + donemBorc - donemAlacak;

            satirlar.Add(new CariBakiyeSatiri
            {
                CariHesapKodu = cari.CariHesapKodu,
                CariHesapAdi = cari.HesapAdi,
                DovizKodu = cari.DovizTanim?.DovizKodu ?? "",
                DevredenBakiye = devreden,
                DonemBorc = donemBorc,
                DonemAlacak = donemAlacak,
                KapanisBakiyesi = kapanis
            });
        }

        ReportServiceController reportController = Frame.GetController<ReportServiceController>();
        if (reportController == null) return;

        IReportStorage reportStorage = Application.ServiceProvider.GetRequiredService<IReportStorage>();
        using IObjectSpace reportObjectSpace = Application.CreateObjectSpace(typeof(ReportDataV2));
        ReportDataV2 reportData = reportObjectSpace.GetObjectsQuery<ReportDataV2>()
            .FirstOrDefault(r => r.DisplayName == ReportDisplayName);
        if (reportData == null)
            throw new UserFriendlyException($"'{ReportDisplayName}' raporu kayıtlı değil.");
        string handle = reportStorage.GetReportContainerHandle(reportData);

        ReportPreviewContext previewContext = Application.ServiceProvider.GetRequiredService<ReportPreviewContext>();
        previewContext.DataSource = satirlar;
        previewContext.ParameterValues.Clear();
        previewContext.ParameterValues["RaporBasligi"] = ReportDisplayName;
        previewContext.ParameterValues["BaslangicTarihi"] = baslangic;
        previewContext.ParameterValues["BitisTarihi"] = bitis;

        reportController.ShowPreview(handle);
    }
}
