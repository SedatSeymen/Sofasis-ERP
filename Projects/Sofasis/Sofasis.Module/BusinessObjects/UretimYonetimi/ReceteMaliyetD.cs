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
    [XafDisplayName("Üretim Reçete Maliyet Kalemleri")]
    [Appearance("ReceteMaliyetDColoredInListView", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "MalzemeAdi = 'GENEL GİDER'", Context = "ListView", 
        FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]

    [Appearance("ReceteMaliyetEksikVeri", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "Tutar = 0 or Tutar is Null", Context = "ListView",
        FontStyle = DevExpress.Drawing.DXFontStyle.Bold ,FontColor ="Red", Priority = 1)]
    public class ReceteMaliyetD : BaseClass
    { 
        public ReceteMaliyetD(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        ReceteTanimM receteTanimM;
        decimal? tutar;
        decimal? birimFiyat;
        BirimTanim birimTanim;
        decimal? miktar;
        string malzemeAdi;


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [Appearance("ED_ReceteMaliyetD_MalzemeAdi", Enabled = false, Criteria = "", Context = "DetailView")]

        [XafDisplayName("Malzeme Adı")]
        public string MalzemeAdi
        {
            get => malzemeAdi;
            set => SetPropertyValue(nameof(MalzemeAdi), ref malzemeAdi, value);
        }

        [XafDisplayName("Kullanılan Miktar")]
        [Appearance("ED_ReceteMaliyetD_Miktar", Enabled = false, Criteria = "", Context = "DetailView")]

        public decimal? Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        [XafDisplayName("Birim")]
        [Appearance("ED_ReceteMaliyetD_Birim", Enabled = false, Criteria = "", Context = "DetailView")]

        public BirimTanim BirimTanim
        {
            get => birimTanim;
            set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
        }

        [XafDisplayName("Birim Fiyat")]
        [Appearance("ED_ReceteMaliyetD_BirimFiyat", Enabled = false, Criteria = "", Context = "DetailView")]

        public decimal? BirimFiyat
        {
            get => birimFiyat;
            set => SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
        }

        [XafDisplayName("Tutar")]
        [Appearance("ED_ReceteMaliyetD_Tutar", Enabled = false, Criteria = "", Context = "DetailView")]

        public decimal? Tutar
        {
            get => tutar;
            set => SetPropertyValue(nameof(Tutar), ref tutar, value);
        }

        [Association("ReceteTanimM-ReceteMaliyetDs")]
        public ReceteTanimM ReceteTanimM
        {
            get => receteTanimM;
            set => SetPropertyValue(nameof(ReceteTanimM), ref receteTanimM, value);
        }
    }
}