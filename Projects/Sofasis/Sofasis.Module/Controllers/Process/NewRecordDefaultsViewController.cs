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
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sofasis.Module.Controllers
{
    public partial class NewRecordDefaultsViewController : ViewController<DetailView>
    {

        public NewRecordDefaultsViewController()
        {
            InitializeComponent();
        }
        protected override void OnActivated()
        {
            if (View.CurrentObject == null) return;

            if (View.ObjectSpace.IsNewObject(View.CurrentObject))
            {
                if (View.CurrentObject is CariHesapTanim)
                {
                    var entity = View.CurrentObject as CariHesapTanim;

                    if (View.Id == "CariHesapTanim_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CARITN"));
                        entity.FisTuruTanim = FisTuruEntity;
                    }
                }
                if (View.CurrentObject is StokTanim)
                {
                    StokTanim entity = View.CurrentObject as StokTanim;

                    if (View.Id == "MasrafTanim_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "MSRFTN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.StokHizmetMasrafTipi = StokHizmetMasrafTipi.Masraf;
                    }
                    if (View.Id == "HizmetTanim_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "HZMTTN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.StokHizmetMasrafTipi = StokHizmetMasrafTipi.Hizmet;
                    }
                    if (View.Id == "StokTanim_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STOKTN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.StokHizmetMasrafTipi = StokHizmetMasrafTipi.Stok;
                    }
                }
                if (View.CurrentObject is CariHesapHareketleri)
                {
                    CariHesapHareketleri entity = View.CurrentObject as CariHesapHareketleri;

                    if (View.Id == "CariHesapAcilisFisi_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CAACLS"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;
                    }
                    if (View.Id == "CariHesapNakitTahsilat_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CATHSL"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;

                    }
                    if (View.Id == "CariHesapNakitOdeme_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CAODME"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;

                    }
                    if (View.Id == "CariHesapBorcDekontu_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CABDKN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;

                    }
                    if (View.Id == "CariHesapAlacakDekontu_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CAADKN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;

                    }
                    if (View.Id == "CariHesapVirman_ListView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "CAVRMN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Cari;

                    }
                }
                if (View.CurrentObject is KasaBankaHareketleri)
                {
                    KasaBankaHareketleri entity = View.CurrentObject as KasaBankaHareketleri;

                    if (View.Id == "KasaAcilisFisi_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "KSACLS"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;
                    }
                    if (View.Id == "KasaTahsilatFisi_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "KSTHSL"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;
                    }
                    if (View.Id == "KasaOdemeFisi_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "KSODME"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;
                    }
                    if (View.Id == "BankaAcilisFisi_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "BNACLS"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Banka;
                    }
                    if (View.Id == "BankaGelenHavale_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "BNGLNH"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Banka;
                    }
                    if (View.Id == "BankaGidenHavale_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "BNGDNH"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Banka;
                    }
                    if (View.Id == "BankaYatirilanPara_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "BNYATP"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Banka;
                    }
                    if (View.Id == "BankaCekilenPara_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "BNCEKP"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Banka;
                    }

                }
                if (View.CurrentObject is KasaBankaTanim)
                {
                    var entity = View.CurrentObject as KasaBankaTanim;
                    entity.AcilisBakiyesi = 0;

                    if (View.Id == "BankaHesapTanim_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "BANKTN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Banka;
                    }
                    if (View.Id == "KasaHesapTanim_DetailView")
                    {
                        FisTuruTanim FisTuruEntity = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "KASATN"));
                        entity.FisTuruTanim = FisTuruEntity;
                        entity.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;
                    }
                }
                if (View.CurrentObject is StokHareketleriM)
                {
                    StokHareketleriM entity = View.CurrentObject as StokHareketleriM;

                    if (View.Id == "StokAcilisFisi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STACLS"));
                    }
                    if (View.Id == "StokAlisGirisi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STALIS"));
                    }
                    if (View.Id == "StokUretimGirisi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STURET"));
                    }
                    if (View.Id == "StokSayimFazlasi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STSFAZ"));
                    }
                    if (View.Id == "StokSatisCikisi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STSTIS"));
                    }
                    if (View.Id == "StokUretimTuketimi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STURTK"));
                    }
                    if (View.Id == "StokSayimEksigi_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STSEKS"));
                    }
                    if (View.Id == "StokFireZayiat_DetailView")
                    {
                        entity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "STFIRE"));
                    }
                }
                if (View.CurrentObject is SatinAlmaTalebiM satinAlmaTalebiEntity)
                {
                    if (View.Id == "SatinAlmaTalebiM_DetailView")
                    {
                        satinAlmaTalebiEntity.FisTuruTanim = View.ObjectSpace.FindObject<FisTuruTanim>(
                            new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), "SATALT"));
                    }
                }
                base.OnActivated();
            }


        }
    }
}
