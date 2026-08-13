using DevExpress.CodeParser;
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
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo.Metadata;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sofasis.Module.Controllers
{
    public partial class CariFiyatGuncelleViewController : ViewController
    {
        PopupWindowShowAction showCariFiyatGuncelleAction;
        public CariFiyatGuncelleViewController()
        {
            InitializeComponent();
            TargetObjectType = typeof(SatisFiyatListeM);
            TargetViewType = ViewType.DetailView;
            showCariFiyatGuncelleAction = new PopupWindowShowAction(this, "ShowNotesAction", PredefinedCategory.Edit)
            {
                Caption = "Cari Fiyat Listesi Güncelle"
            };
            showCariFiyatGuncelleAction.Execute += new PopupWindowShowActionExecuteEventHandler(showCariFiyatGuncelleAction_Execute);
            showCariFiyatGuncelleAction.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
            showCariFiyatGuncelleAction.CustomizePopupWindowParams += showCariFiyatGuncelleAction_CustomizePopupWindowParams;
        }

        private void showCariFiyatGuncelleAction_CustomizePopupWindowParams(
            object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(CariFiyatGuncelleme));
            Object currentObject = objectSpace.GetObject(View.CurrentObject);
            if (currentObject != null)
            {
                object cariFiyat = objectSpace.CreateObject(typeof(CariFiyatGuncelleme));
                (cariFiyat as CariFiyatGuncelleme).EskiFiyatListesi= (currentObject as SatisFiyatListeM);
                e.View = Application.CreateDetailView(objectSpace, cariFiyat);

            }
            else
            {
                objectSpace.Dispose();
            }
        }

        void showCariFiyatGuncelleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            CariFiyatGuncelleme cariFiyat = e.PopupWindowViewCurrentObject as CariFiyatGuncelleme;
            IObjectSpace cariOS = e.PopupWindowView.ObjectSpace as IObjectSpace;

            if (cariFiyat != null)
            {
                if(cariFiyat.EskiFiyatListesi == cariFiyat.YeniFiyatListesi)
                {
                    cariFiyat.EskiFiyatListesi = null;
                    cariFiyat.YeniFiyatListesi = null;
                    throw new UserFriendlyException("Eski Fiyat Listesi İle Yeni Fiyat Listesi Aynı Olamaz");

                }
                else if (cariFiyat.EskiFiyatListesi == null)
                {
                    throw new UserFriendlyException("Eski Fiyat Listesi Boş Olamaz!!!");

                }
                else if (cariFiyat.YeniFiyatListesi == null)
                {
                    throw new UserFriendlyException("Yeni Fiyat Listesi Boş Olamaz!!!");

                }
                else if (cariFiyat.YeniFiyatListesi.BaslangicTarihi < cariFiyat.EskiFiyatListesi.BaslangicTarihi)
                {
                    throw new UserFriendlyException("Yeni Fiyat Liste Tarihi, Eski Fiyat Liste Tarihinden Daha Eski Olamaz!!!");

                }
                else if (cariFiyat.EskiFiyatListesi.BaslangicTarihi<= cariFiyat.YeniFiyatListesi.BaslangicTarihi)
                {
                    CriteriaOperator taskCriteria =
                        new BinaryOperator(nameof(CariHesapTanim.SatisFiyatListeM), cariFiyat.EskiFiyatListesi);

                    IList cariList = cariOS.GetObjects(typeof(CariHesapTanim), taskCriteria, false);
                    
                    foreach (CariHesapTanim obj in cariList)
                    {
                        obj.SatisFiyatListeM = cariFiyat.YeniFiyatListesi;
                        obj.Save();
                    }
                    cariOS.CommitChanges();
                    showCariFiyatGuncelleAction.Enabled[View.Id] = false;
                }
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();

        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            showCariFiyatGuncelleAction.Enabled[View.Id] = true;
        }
    }
}
