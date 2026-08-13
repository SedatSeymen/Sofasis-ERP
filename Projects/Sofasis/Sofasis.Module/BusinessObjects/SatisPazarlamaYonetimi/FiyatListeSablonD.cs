using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class FiyatListeSablonD : BaseClass
    { 
        public FiyatListeSablonD(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if(Session.IsNewObject(this))
            {
                SiraNo = 0;
                FiyatListesindeYazdir = true;
            }
        }

        int? siraNo;
        FiyatListeSablonM fiyatListeSablonM;
        StokModelTanim stokModelTanim;
        StokTanim stokTanim;
        bool fiyatListesindeYazdir;


        [XafDisplayName("Sıra No"), ToolTip("")]
        [Appearance("ED_FiyatListeSablonD_SiraNo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FiyatListeSablonD_SiraNo", DefaultContexts.Save, "Lütfen Model Adını Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:n0}")]
        [ModelDefault("EditMask", "n0")]
        public int? SiraNo
        {
            get => siraNo;
            set => SetPropertyValue(nameof(SiraNo), ref siraNo, value);
        }

        [XafDisplayName("Model Adı"), ToolTip("")]
        [Appearance("ED_FiyatListeSablonD_ModelAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FiyatListeSablonD_ModelAdi", DefaultContexts.Save, "Lütfen Model Adını Giriniz...")]
        public StokModelTanim StokModelTanim
        {
            get => stokModelTanim;
            set => SetPropertyValue(nameof(StokModelTanim), ref stokModelTanim, value);
        }

        [XafDisplayName("Stok Adı"), ToolTip("")]
        [Appearance("ED_FiyatListeSablonD_StokAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FiyatListeSablonD_StokAdi", DefaultContexts.Save, "Lütfen Stok Adını Giriniz...")]
        [DataSourceCriteria("StokModelTanim = '@This.StokModelTanim'")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [XafDisplayName("Fiyat Listesinde Yazdırılsın")]
        public bool FiyatListesindeYazdir
        {
            get => fiyatListesindeYazdir;
            set => SetPropertyValue(nameof(FiyatListesindeYazdir), ref fiyatListesindeYazdir, value);
        }


        [Association("FiyatListeSablonM-FiyatListeSablonDs")]
        public FiyatListeSablonM FiyatListeSablonM
        {
            get => fiyatListeSablonM;
            set => SetPropertyValue(nameof(FiyatListeSablonM), ref fiyatListeSablonM, value);
        }

        protected override void OnDeleting()
        {
            try
            {
                var Fiyatlist = new XPQuery<SatisFiyatListeD>(Session).Where(x=> x.StokTanim == this.StokTanim);

                if (Fiyatlist.Count() > 0)
                {
                    throw new UserFriendlyException("Bu Stok Fiyat Listesinde Kullanılmış !!! Silemezsiniz");
                }

                else
                    base.OnDeleting();

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

    }
}