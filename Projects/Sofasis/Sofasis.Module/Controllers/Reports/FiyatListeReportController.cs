using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sofasis.Module.Controllers
{
    public partial class FiyatListeReportController : ObjectViewController<ListView,SatisFiyatListeM>
    {
        SingleChoiceAction FiyatListeReportViewController_Action;
        IList SelectedDataList;
        public FiyatListeReportController()
        {
            InitializeComponent();
            FiyatListeReportViewController_Action = new SingleChoiceAction(this, 
                "FiyatListeReportViewController_Action", PredefinedCategory.Edit)
            {
                Caption = "Fiyat Listeleri Yazdırma",
                ItemType = SingleChoiceActionItemType.ItemIsOperation,
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ImageName = "Print"
            };
            FiyatListeReportViewController_Action.Execute += FiyatListeReportViewController_Action_Execute; ;
        }

        private void FiyatListeReportViewController_Action_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            List<string> filterList = new List<string>();

            IReportDataV2 reportData = e.SelectedChoiceActionItem.Data as IReportDataV2;
            ReportsModuleV2 reportsModule = ReportsModuleV2.FindReportsModule(Application.Modules);
            if (reportsModule != null)
            {
                reportsModule.ReportsDataSourceHelper.BeforeShowPreview += ReportsDataSourceHelper_BeforeShowPreview;
            }

            if (reportData == null) return;
#pragma warning disable CS0618 // Type or member is obsolete
            string handle = ReportDataProvider.ReportsStorage.GetReportContainerHandle(reportData);
#pragma warning restore CS0618 // Type or member is obsolete
            SelectedDataList = View.Editor.GetSelectedObjects();
            


            // NOT: MasterKeyID, WW_FiyatListeRapor dataview'ının kendi ayrı string(13) PK'sı; Faz B
            // (Guid Oid migrasyonu, ADR-015) kapsamında bu tabloya dokunulmadı (aktif kullanılmıyor,
            // dolduran bir yazıcı bulunamadı). .Oid.ToString() (36 karakter) artık MasterKeyID
            // (varchar 13) ile eşleşmediğinden bu filtre fiilen hiçbir satır bulamaz.
            for (int i = 0; i < SelectedDataList.Count; i++)
            {
                filterList.Add(((SatisFiyatListeM)SelectedDataList[i]).Oid.ToString());
            }
            if (filterList.Count > 0)
            {
                ReportServiceController controller = Frame.GetController<ReportServiceController>();
                if (controller != null)
                {
                    CriteriaOperator criteria = new InOperator("MasterKeyID", filterList.AsEnumerable());
                    SortProperty[] sortlist = new SortProperty[2];
                    sortlist.Append(new SortProperty("DetayKeyID", DevExpress.Xpo.DB.SortingDirection.Ascending));
                    sortlist.Append(new SortProperty("StokModelAdi", DevExpress.Xpo.DB.SortingDirection.Ascending));

                    controller.ShowPreview(handle, criteria, sortlist);
                }
            }
        }

        private void ReportsDataSourceHelper_BeforeShowPreview(object sender, BeforeShowPreviewEventArgs e)
        {
            string name = string.Empty;
            SatisFiyatListeM source = SelectedDataList[0] as SatisFiyatListeM;
            if (source != null)
            {
                string day, month, year;
                day = DateTime.Now.Day.ToString();
                if (day.Length == 1) day = "0" + day;
                month = DateTime.Now.Month.ToString();
                if (month.Length == 1) month = "0" + day;
                year = DateTime.Now.Year.ToString();

                if (source.DovizTanim.DovizKodu == "TRY")
                    name = "TRY_FiyatListesi_" + day + month + year;
                else name = source.DovizTanim.DovizKodu + "_PriceList_" + day + month + year;
            }
            e.Report.ExportOptions.PrintPreview.DefaultFileName = name;

        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            FiyatListeReportViewController_Action.Items.Clear();
#pragma warning disable CS0618 // Type or member is obsolete
            IObjectSpace objectSpace = ReportDataProvider.ReportObjectSpaceProvider.CreateObjectSpace(typeof(ReportDataV2));
#pragma warning restore CS0618 // Type or member is obsolete

            CriteriaOperator criteria = CriteriaOperator.Parse("DataTypeName  " +
                "In ('Sofasis.Module.BusinessObjects.WW_FiyatListeRapor')");
            IList<ReportDataV2> reportData = objectSpace.GetObjects<ReportDataV2>(criteria);
            foreach (ReportDataV2 report in reportData)
            { 
                ChoiceActionItem item = new ChoiceActionItem(report.DisplayName, report);
                item.ImageName = "Action_Report_Object_Inplace_Preview";
                FiyatListeReportViewController_Action.Items.Add(item);
            }
        }
    }
}
