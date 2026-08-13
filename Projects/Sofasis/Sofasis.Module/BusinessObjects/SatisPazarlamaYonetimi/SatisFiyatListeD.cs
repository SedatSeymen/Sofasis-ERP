using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Fiyat Liste Detay Bilgileri")]
    public class SatisFiyatListeD : BaseClass
    {
        public SatisFiyatListeD(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            FiyatListesindeYazdir = true;
        }

        bool fiyatListesindeYazdir;
        SatisFiyatListeM fiyatListeM;
        decimal? listeFiyati;
        decimal? birimFiyat;
        StokTanim stokTanim;
        StokModelTanim stokModelTanim;


        [XafDisplayName("Model Adı"), ToolTip("Lütfen Stok Model Adını Giriniz...")]
        [Appearance("ED_SatisFiyatListeD_StokModelTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public StokModelTanim StokModelTanim
        {
            get => stokModelTanim;
            set
            {
                SetPropertyValue(nameof(StokModelTanim), ref stokModelTanim, value);
                if (IsLoading)
                {
                    StokTanim = null;
                }
            }
        }

        [XafDisplayName("Stok Adı"), ToolTip("Lütfen Stok Adını Giriniz")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeD_StokTanim", DefaultContexts.Save, "Lütfen Stok Adını Giriniz...")]
        [Appearance("ED_SatisFiyatListeD_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [DataSourceCriteria("StokModelTanim = '@This.StokModelTanim' and StokTipi = '@StokTipi.Mamul'")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [XafDisplayName("Birim Fiyat"), ToolTip("Lütfen Birim Fiyatını Giriniz...")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeD_BirimFiyat", DefaultContexts.Save, "Lütfen Birim Fiyatını Giriniz...")]
        public decimal? BirimFiyat
        {
            get => birimFiyat;
            set
            {
                SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
                CalculateFiyat(BirimFiyat);
            }
        }

        [XafDisplayName("Liste Fiyatı"), ToolTip("Lütfen Liste Fiyatını Giriniz...")]
        [Appearance("ED_SatisFiyatListeD_ListeFiyati", Enabled = false, Criteria = "", Context = "DetailView")]
        public decimal? ListeFiyati
        {
            get => listeFiyati;
            set => SetPropertyValue(nameof(ListeFiyati), ref listeFiyati, value);
        }

        [XafDisplayName("Fiyat Listesinde Yazdırılsın")]
        public bool FiyatListesindeYazdir
        {
            get => fiyatListesindeYazdir;
            set => SetPropertyValue(nameof(FiyatListesindeYazdir), ref fiyatListesindeYazdir, value);
        }


        [Association("SatisFiyatListeM-SatisFiyatListeD")]

        public SatisFiyatListeM SatisFiyatListeM
        {
            get => fiyatListeM;
            set => SetPropertyValue(nameof(SatisFiyatListeM), ref fiyatListeM, value);
        }

        void CalculateFiyat(decimal? BirimFiyat)
        {
            decimal? _BirimFiyat = 0;
            decimal? _ListeOrani = 0;

            if (SatisFiyatListeM != null)
            {
                if (SatisFiyatListeM.KDVTanim == null) return;
                if (SatisFiyatListeM.ListeOrani == null) return;
                _BirimFiyat = BirimFiyat;
                _ListeOrani = SatisFiyatListeM.ListeOrani / 100;
                _ListeOrani = 1 + _ListeOrani;

                ListeFiyati = BirimFiyat * _ListeOrani;
            }
        }

    }
}