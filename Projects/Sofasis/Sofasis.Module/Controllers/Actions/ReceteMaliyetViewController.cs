using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using System;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    public partial class ReceteMaliyetViewController : ViewController
    {
        SimpleAction calcMaliyet;
        IObjectSpace os = null;
        public ReceteMaliyetViewController()
        {
            InitializeComponent();

            TargetViewType = ViewType.DetailView;
            TargetObjectType = typeof(ReceteTanimM);

            calcMaliyet = new SimpleAction(this, "calcMaliyet", PredefinedCategory.View)
            {
                Caption = "Maliyet Hesapla",
                ImageName = "CalculateNow"
            };
            calcMaliyet.Execute += calcMaliyet_Execute;
        }

        private void calcMaliyet_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                MaliyetParametre parametre = os.GetObjects<MaliyetParametre>().FirstOrDefault();
                decimal? ToplamGider =
                    os.GetObjects<StokTanim>().Where(x => x.StokHizmetMasrafTipi == StokHizmetMasrafTipi.Masraf).Sum(x => x.AlisFiyati);
                ReceteTanimM recete = View.CurrentObject as ReceteTanimM;


                if (recete != null && parametre != null)
                {
                    os.Delete(recete.ReceteMaliyetDs);

                    recete.UrunMaliyeti = 0;

                    ReceteMaliyetD maliyet = os.CreateObject<ReceteMaliyetD>();
                    maliyet.MalzemeAdi = "GENEL GİDER";
                    maliyet.BirimFiyat = ToplamGider / parametre.UretilecekTakimSayisi;
                    maliyet.Miktar = recete.GenelGiderOrani;
                    maliyet.Tutar = maliyet.Miktar * (maliyet.BirimFiyat / 100);
                    recete.ReceteMaliyetDs.Add(maliyet);


                    foreach (var item in recete.ReceteTanimDs)
                    {
                        ReceteMaliyetD rmaliyet = os.CreateObject<ReceteMaliyetD>();
                        rmaliyet.MalzemeAdi = item.StokTanim.StokAdi;

                        if (item.StokTanim.DovizTanim.DovizKodu == "TRY")
                            rmaliyet.BirimFiyat = item.StokTanim.AlisFiyati;
                        else if (item.StokTanim.DovizTanim.DovizKodu == "USD")
                            rmaliyet.BirimFiyat = item.StokTanim.AlisFiyati * parametre.USDAlisKuru;
                        else if (item.StokTanim.DovizTanim.DovizKodu == "EURO")
                            rmaliyet.BirimFiyat = item.StokTanim.AlisFiyati * parametre.EuroAlisKuru;
                        rmaliyet.BirimTanim = item.BirimTanim;
                        rmaliyet.Miktar = item.Miktar;
                        rmaliyet.Tutar = item.Miktar * rmaliyet.BirimFiyat;
                        recete.ReceteMaliyetDs.Add(rmaliyet);
                    }
                    recete.UrunMaliyeti = recete.ReceteMaliyetDs.Sum(x => x.Tutar);
                    os.CommitChanges();
                    View.RefreshDataSource();
                }

            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
                throw;
            }



        }

        protected override void OnActivated()
        {
            base.OnActivated();
            os = View.ObjectSpace;
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }
    }
}
