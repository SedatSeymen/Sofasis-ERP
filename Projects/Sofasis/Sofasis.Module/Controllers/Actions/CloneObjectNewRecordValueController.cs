using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.CloneObject;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sofasis.Module.Controllers
{
    public partial class CloneObjectNewRecordValueController : ViewController
    {
        IObjectSpace os;
       public CloneObjectNewRecordValueController()
        {
            InitializeComponent();
        }
        protected override void OnActivated()
        {
            os = View.ObjectSpace;

            CloneObjectViewController cloneObjectController =
                Frame.GetController<CloneObjectViewController>();
            if (cloneObjectController != null)
            {
                cloneObjectController.CustomShowClonedObject += CloneObjectController_CustomShowClonedObject;

            }
            base.OnActivated();
        }

        private void CloneObjectController_CustomShowClonedObject(object sender, CustomShowClonedObjectEventArgs e)
        {
            if(e.ClonedObject.GetType() == typeof(SatisSiparisM))
                (e.ClonedObject as SatisSiparisM).SiparisKodu = Helper.ConstNewRecordText;

            if (e.ClonedObject.GetType() == typeof(StokTanim))
                (e.ClonedObject as StokTanim).StokKodu = Helper.ConstNewRecordText;

            if (e.ClonedObject.GetType() == typeof(ReceteTanimM))
                (e.ClonedObject as ReceteTanimM).ReceteKodu = Helper.ConstNewRecordText;
        }
    }
}
