using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sofasis.Module.Controllers
{
    public partial class SatisSiparisModelKonfigrasyonController : ViewController
    {
        PopupWindowShowAction showSiparisKonfigratorAction;
        public SatisSiparisModelKonfigrasyonController()
        {
            InitializeComponent();
            TargetObjectType = typeof(SatisSiparisM);
            TargetViewType = ViewType.DetailView;

            showSiparisKonfigratorAction = new PopupWindowShowAction(this, "showSiparisKonfigratorAction", PredefinedCategory.Edit)
            {
                Caption = "Sipariş Konfigratör",
                ImageName = "ModelEditor_GenerateContent"


            };
            showSiparisKonfigratorAction.Execute += new PopupWindowShowActionExecuteEventHandler(showSiparisKonfigratorAction_Execute);
            showSiparisKonfigratorAction.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
            showSiparisKonfigratorAction.CustomizePopupWindowParams += showSiparisKonfigratorAction_CustomizePopupWindowParams;

        }

        private void showSiparisKonfigratorAction_CustomizePopupWindowParams(
            object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(SiparisKonfigrator));
            Object currentObject = objectSpace.GetObject(View.CurrentObject);
            if (currentObject != null)
            {
                object siparisKonfig = objectSpace.CreateObject(typeof(SiparisKonfigrator));
                e.View = Application.CreateDetailView(objectSpace, siparisKonfig);

            }
            else
            {
                objectSpace.Dispose();
            }
        }

        void showSiparisKonfigratorAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            SiparisKonfigrator siparisKonfig = e.PopupWindowViewCurrentObject as SiparisKonfigrator;
            IObjectSpace KonfigOS = e.PopupWindowView.ObjectSpace as IObjectSpace;
            //if(View.ObjectSpace.IsNewObject(View.CurrentObject))
            //{
            //    (View.CurrentObject as SatisSiparisM).Save();
            //}

            if (siparisKonfig != null)
            {
                if (siparisKonfig.StokModelKonfigrasyonM == null)
                {
                    throw new UserFriendlyException("Lütfen Ürün Konfigrasyon Seçimi Yapınız...");

                }
                else if (siparisKonfig.StokTanim == null)
                {
                    throw new UserFriendlyException("Lütfen Kumaş Seçimi Yapınız...");

                }

                else
                {
                    IList KonfList = siparisKonfig.StokModelKonfigrasyonM.StokModelKonfigrasyonDs;
                    foreach (StokModelKonfigrasyonD obj in KonfList)
                    {
                        // obj/siparisKonfig popup penceresinin kendi (ayrı) ObjectSpace'ine ait;
                        // View.ObjectSpace'te oluşturulan item'a atanabilmeleri için View.ObjectSpace'e
                        // eşlenmeleri gerekiyor (GetObject, PK tipinden bağımsız çalışır).
                        var Grup = View.ObjectSpace.GetObject(obj.StokTanim.StokGrupTanim);
                        var Stok = View.ObjectSpace.GetObject(obj.StokTanim);
                        var Kumas = View.ObjectSpace.GetObject(siparisKonfig.StokTanim);

                        SatisSiparisD item = View.ObjectSpace.CreateObject<SatisSiparisD>();
                        item.StokGrupTanim = Grup;
                        item.KumasStokAdi = Kumas;
                        item.SatisSiparisM = View.CurrentObject as SatisSiparisM;
                        item.StokTanim = Stok;
                        item.BirimTanim = Stok.BirimTanim;
                        item.Miktar = obj.Miktar;
                        var fiyatlisteDetay = (View.CurrentObject as SatisSiparisM).SatisFiyatListeM.SatisFiyatListeD;
                        var stokFiyat = fiyatlisteDetay.FirstOrDefault<SatisFiyatListeD>(x => x.StokTanim == Stok);
                        if (stokFiyat != null)
                            item.BirimFiyat = stokFiyat.BirimFiyat;
                        item.Save();
                    }

                    View.ObjectSpace.CommitChanges();
                    showSiparisKonfigratorAction.Enabled[View.Id] = false;
                }
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            showSiparisKonfigratorAction.Enabled[View.Id] = false;
            View.ObjectSpace.Committing += ObjectSpace_Committing;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            showSiparisKonfigratorAction.Enabled[View.Id] = true;
        }

        private void ObjectSpace_Committing(object sender, CancelEventArgs e)
        {
            if (View.ObjectSpace.IsObjectToSave(View.CurrentObject))
            {
                showSiparisKonfigratorAction.Enabled[View.Id] = true;
            }
        }
    }
}