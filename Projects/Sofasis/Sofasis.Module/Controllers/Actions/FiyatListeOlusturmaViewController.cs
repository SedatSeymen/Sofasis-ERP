using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    public partial class FiyatListeOlusturmaViewController : ObjectViewController<DetailView,SatisFiyatListeM>
    {
        IObjectSpace os;
        SimpleAction actCreatePriceList;
        XPCollection<SatisFiyatListeD> DetailList;
        SatisFiyatListeM MasterEntity;
        MaliyetParametre parametre;
        public FiyatListeOlusturmaViewController()
        {
            InitializeComponent();
            actCreatePriceList = new SimpleAction(this, "actCreatePriceList", PredefinedCategory.Edit)
            {
                Caption = "Fiyat Listesi Oluştur",
                ImageName = "ModelEditor_GenerateContent"
            };
            actCreatePriceList.Execute += ActCreatePriceList_Execute;
        }

        private void ActCreatePriceList_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (DetailList == null) return;
            else if (DetailList.Count > 0)
            {
                throw new UserFriendlyException("Bu Liste Daha Önce Oluşturulmuş!!! Yeniden Oluşturmak İçin Detay Verilerini Silmeniz Gerekir...");
            }
            else if (View.CurrentObject == null) return;

            else if (((SatisFiyatListeM)View.CurrentObject).FiyatListeSablonM == null)
            {
                throw new UserFriendlyException("Lütfen Fiyat Liste Şablonunu Seçip Yeniden Deneyiniz...");
            }
            else if (((SatisFiyatListeM)View.CurrentObject).FiyatListeSablonM.FiyatListeSablonDs == null)
            {
                throw new UserFriendlyException("Fiyat Liste Şablonunda Ürün Tanımlanmamış...");
            }
            else
            {
                try
                {
                    decimal? ListeOrani = 0;
                    ListeOrani = MasterEntity.ListeOrani / 100;
                    ListeOrani = 1 + ListeOrani;

                    FiyatListeSablonM entity = ((SatisFiyatListeM)View.CurrentObject).FiyatListeSablonM;
                    UrunMaliyetHesapla();

                    foreach (var item in entity.FiyatListeSablonDs.OrderBy(x => x.SiraNo))
                    {
                        SatisFiyatListeD fiyatListeD = os.CreateObject<SatisFiyatListeD>();
                        fiyatListeD.StokTanim = item.StokTanim;
                        fiyatListeD.StokModelTanim = item.StokModelTanim;
                        fiyatListeD.BirimFiyat = UrunBirimFiyatHesapla(item.StokTanim);
                        fiyatListeD.ListeFiyati = fiyatListeD.BirimFiyat * ListeOrani;
                        fiyatListeD.FiyatListesindeYazdir = item.FiyatListesindeYazdir;
                        DetailList.Add(fiyatListeD);

                    }
                    entity.Save();
                    os.CommitChanges();
                    View.RefreshDataSource();
                    actCreatePriceList.Enabled[View.Id] = false;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void UrunMaliyetHesapla()
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
                    // D-4/B6: commit + refresh döngü DIŞINA alındı (TopluReceteMaliyetViewController'da
                    // zaten uygulanmış düzeltmenin unutulmuş bir kopyası) — N reçete için N commit yerine tek commit.
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
        decimal? UrunBirimFiyatHesapla(StokTanim stokTanim)
        {
            StokTanim stok = stokTanim;
            decimal? result = 0;
            decimal? KarOrani = 0;
            CriteriaOperator criteria = new BinaryOperator("StokTanim", stok);
            ReceteTanimM recete = os.FindObject<ReceteTanimM>(criteria);

            KarOrani = MasterEntity.KarOrani/100;
            KarOrani = 1 + KarOrani;

            if (recete != null) 
            {
                if (MasterEntity.DovizTanim.DovizKodu == "TRY")
                {
                    result = recete.UrunMaliyeti * KarOrani;
                }
                if (MasterEntity.DovizTanim.DovizKodu == "USD")
                {
                    result = (recete.UrunMaliyeti * KarOrani)/parametre.USDSatisKuru;
                }
                if (MasterEntity.DovizTanim.DovizKodu == "EUR")
                {
                    result = (recete.UrunMaliyeti * KarOrani) / parametre.EuroSatisKuru;
                }

                return result;
            }
            else return 0;
        }
        protected override void OnActivated()
        {
            os = View.ObjectSpace;
            DetailList = ((SatisFiyatListeM)View.CurrentObject).SatisFiyatListeD;
            parametre = os.GetObjects<MaliyetParametre>().FirstOrDefault();
            MasterEntity = View.CurrentObject as SatisFiyatListeM;

        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            actCreatePriceList.Enabled[View.Id] = true;

        }
    }
}
