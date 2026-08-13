using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System.Collections.Generic;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    // Kullanıcı isteği: "Satın Alma Teklifleri" ana listesinden bir veya birden çok teklif
    // (SatinAlmaTeklifM, teklifin TÜMÜ — Teklif Karşılaştırma'daki gibi tek tek kalem seçmeye
    // gerek kalmadan) doğrudan Sipariş'e dönüştürülebilsin. Aynı servis (ISatinAlmaSiparisServisi.
    // TekliflerdenSiparisOlustur) kullanılır — {Tedarikçi, Talep} bazında gruplama sayesinde farklı
    // tedarikçilerden seçilen teklifler otomatik olarak ayrı Sipariş'lere ayrılır, aynı tedarikçiden
    // olanlar tek Sipariş'te birleşir. Zaten kesinleşmiş fiyat/tedarikçi taşıdığından (Teklif
    // Karşılaştırma'daki satır-bazlı dönüştürme gibi) doğrudan KAYDEDİLİR — Talep'ten doğrudan
    // dönüştürmenin aksine (bkz. SatinAlmaTalebiSiparisOlusturController) taslak/gözden geçirme
    // adımına gerek yoktur.
    public class SatinAlmaTeklifSiparisOlusturController : ViewController<ListView>
    {
        readonly ISatinAlmaSiparisServisi siparisServisi = new SatinAlmaSiparisServisi();

        SimpleAction siparisOlusturAction;

        public SatinAlmaTeklifSiparisOlusturController()
        {
            TargetViewId = "SatinAlmaTeklifM_ListView";

            siparisOlusturAction = new SimpleAction(this, "SatinAlmaTeklifSiparisOlustur", PredefinedCategory.RecordEdit)
            {
                Caption = "Sipariş Oluştur",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                // Yalnızca henüz karara bağlanmamış (Beklemede/Alındı) teklifler seçiliyken aktif —
                // zaten Seçildi/Reddedildi bir teklif tekrar dönüştürülmeye çalışılırsa (sunucu
                // tarafında SatinAlmaSiparisiM.OnSaving'deki tekrar-sipariş koruması yine de
                // engeller) kullanıcı butonun devre dışı olmasından durumu zaten anlar.
                TargetObjectsCriteria = "Durum = 0 Or Durum = 1",
                ImageName = "Action_Workflow_Activate"
            };
            siparisOlusturAction.Execute += SiparisOlusturAction_Execute;
        }

        void SiparisOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            List<SatinAlmaTeklifD> tumSatirlar = e.SelectedObjects.Cast<SatinAlmaTeklifM>()
                .SelectMany(teklif => teklif.SatinAlmaTeklifDs.Cast<SatinAlmaTeklifD>())
                .ToList();
            List<SatinAlmaSiparisiM> olusanSiparisler = siparisServisi.TekliflerdenSiparisOlustur(tumSatirlar);
            ObjectSpace.CommitChanges();

            string siparisNolari = string.Join(", ", olusanSiparisler.Select(s => s.SiparisNo));
            string mesaj = olusanSiparisler.Count == 1
                ? $"Sipariş oluşturuldu: {siparisNolari}"
                : $"{olusanSiparisler.Count} sipariş oluşturuldu: {siparisNolari}";
            Application.ShowViewStrategy.ShowMessage(mesaj, InformationType.Success);

            View.Refresh();
        }
    }
}
