using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using System;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    public partial class TopluReceteMaliyetViewController : ObjectViewController<ListView, ReceteTanimM>
    {
        SimpleAction calcTotalMaliyet;
        IObjectSpace os = null;
        public TopluReceteMaliyetViewController()
        {
            InitializeComponent();
            calcTotalMaliyet = new SimpleAction(this, "calcTotalMaliyet", PredefinedCategory.View)
            {
                Caption = "Toplu Maliyet Hesapla",
                ImageName = "CalculateNow"
            };
            calcTotalMaliyet.Execute += calcTotalMaliyet_Execute;
        }

        private void calcTotalMaliyet_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                IList<ReceteTanimM> ReceteList = os.GetObjects<ReceteTanimM>();
                if (ReceteList != null)
                {
                    MaliyetParametre parametre = os.GetObjects<MaliyetParametre>().FirstOrDefault();
                    decimal? ToplamGider =
                        os.GetObjects<StokTanim>().Where(x => x.StokHizmetMasrafTipi == StokHizmetMasrafTipi.Masraf).Sum(x => x.AlisFiyati);

                    foreach (var recete in ReceteList)
                    {
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
                                recete.UrunMaliyeti = recete.UrunMaliyeti + rmaliyet.Tutar;
                                recete.ReceteMaliyetDs.Add(rmaliyet);

                            }
                            recete.UrunMaliyeti = recete.ReceteMaliyetDs.Sum(x => x.Tutar);
                        }
                    }
                    os.CommitChanges();
                    View.RefreshDataSource();
                }
                os.Refresh();

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
