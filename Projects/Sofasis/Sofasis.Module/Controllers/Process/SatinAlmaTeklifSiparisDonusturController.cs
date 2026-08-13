using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System.Collections.Generic;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    // Dönüştürme mantığının tamamı ISatinAlmaSiparisServisi'nde (ADR-006) — bu controller yalnızca
    // "Teklif Karşılaştırma" ekranına (SatinAlmaTeklifD_ListView), kullanıcının orada seçtiği
    // satırları Sipariş(ler)e çeviren aksiyonu ekler. Farklı tedarikçilerden/Taleplerden seçilen
    // satırlar otomatik olarak ayrı Sipariş'lere ayrılır (bkz. servis).
    public class SatinAlmaTeklifSiparisDonusturController : ViewController<ListView>
    {
        readonly ISatinAlmaSiparisServisi siparisServisi = new SatinAlmaSiparisServisi();

        SimpleAction sipariseDonusturAction;

        public SatinAlmaTeklifSiparisDonusturController()
        {
            TargetViewId = "SatinAlmaTeklifD_ListView";

            sipariseDonusturAction = new SimpleAction(this, "SatinAlmaTeklifSipariseDonustur", PredefinedCategory.RecordEdit)
            {
                Caption = "Siparişe Dönüştür",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ImageName = "Action_Workflow_Activate"
            };
            sipariseDonusturAction.Execute += SipariseDonusturAction_Execute;
        }

        void SipariseDonusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            List<SatinAlmaTeklifD> secilenSatirlar = e.SelectedObjects.Cast<SatinAlmaTeklifD>().ToList();
            List<SatinAlmaSiparisiM> olusanSiparisler = siparisServisi.TekliflerdenSiparisOlustur(secilenSatirlar);
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
