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
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Fiyat Liste Şablonları")]
    [DefaultProperty("SablonAdi")]
    public class FiyatListeSablonM : BaseClassWithAuditAndDescription
    { 
        public FiyatListeSablonM(Session session)
            : base(session)
        {
            this.FiyatListeSablonDs.CollectionChanged += FiyatListeSablonDs_CollectionChanged;
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();               
        }

        private void FiyatListeSablonDs_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
        {
            if (this != null)
            {
                var list = this.FiyatListeSablonDs.ToList().OrderBy(x => x.SiraNo);

                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        item.SiraNo = 0;
                    }
                    foreach (var item in list)
                    {
                        if (item.SiraNo == 0 || item.SiraNo == null)
                        {
                            int? newRotaSiraNo = list.Max(obj => obj.SiraNo);
                            if (newRotaSiraNo == null) newRotaSiraNo = 0;
                            item.SiraNo = newRotaSiraNo + 1;
                        }

                    }
                }


            }
        }

        string sablonAdi;

        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Şablon Adı"), ToolTip("")]
        [Appearance("ED_FiyatListeSablonM_SablonAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FiyatListeSablonM_SablonAdi", DefaultContexts.Save, "Lütfen Fiyat Listesi Şablon Adını Giriniz...")]


        public string SablonAdi
        {
            get => sablonAdi;
            set => SetPropertyValue(nameof(SablonAdi), ref sablonAdi, value);
        }

        [Association("FiyatListeSablonM-FiyatListeSablonDs"),Aggregated]
        public XPCollection<FiyatListeSablonD> FiyatListeSablonDs
        {
            get
            {
                return GetCollection<FiyatListeSablonD>(nameof(FiyatListeSablonDs));
            }
        }

        protected override void OnDeleting()
        {
            try
            {
                var Fiyatlist = new XPQuery<SatisFiyatListeM>(Session).Where(x=> x.FiyatListeSablonM == this);

                if (Fiyatlist.Count() > 0)
                {
                    throw new UserFriendlyException("Bu Şablon Sipariş Fiyat Listelerinde Kullanılmış !!! Silemezsiniz");
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