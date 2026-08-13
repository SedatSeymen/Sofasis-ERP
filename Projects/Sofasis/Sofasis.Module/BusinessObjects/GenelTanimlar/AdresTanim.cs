using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [DefaultProperty("TamAdresAdi")]
    [XafDisplayName("Adres Tanımlama")]
    public class AdresTanim : BaseClass
    {
        public AdresTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if(Session.IsNewObject(This))
            {
                UlkeTanim UlkeEntity = Session.FindObject<UlkeTanim>(
                    new BinaryOperator(nameof(UlkeTanim.IsVarsayilan), true));
                if (UlkeEntity != null)
                {
                    this.UlkeTanim = UlkeEntity;
                }
            }

        }

        string adres2;
        string adres1;
        SehirTanim sehirTanim;
        UlkeTanim ulkeTanim;
        string postaKodu;


        [Size(100)]
        [XafDisplayName("Adres 1"), ToolTip("Lütfen Adres 1 Satırını Giriniz")]

        public string Adres1
        {
            get => adres1;
            set => SetPropertyValue(nameof(Adres1), ref adres1, value);
        }


        [Size(100)]
        [XafDisplayName("Adres 2"), ToolTip("Lütfen Adres 2 Satırını Giriniz")]
        public string Adres2
        {
            get => adres2;
            set => SetPropertyValue(nameof(Adres2), ref adres2, value);
        }


        [Size(10)]
        [XafDisplayName("Posta Kodu"), ToolTip("Lütfen Posta Kodu Giriniz")]
        public string PostaKodu
        {
            get => postaKodu;
            set => SetPropertyValue(nameof(PostaKodu), ref postaKodu, value);
        }

        [VisibleInListView(false)]
        [XafDisplayName("Ülke Adı"), ToolTip("Lütfen Ülke Adını Giriniz")]
        public UlkeTanim UlkeTanim
        {
            get { return ulkeTanim; }
            set
            {
                SetPropertyValue(nameof(UlkeTanim), ref ulkeTanim, value);
                if (!IsLoading)
                {
                    SehirTanim = null;
                }
            }
        }

        [VisibleInListView(false)]
        [XafDisplayName("Şehir Adı"), ToolTip("Lütfen Şehir Adını Giriniz")]
        [DataSourceProperty("UlkeTanim.SehirTanims", DataSourcePropertyIsNullMode.SelectAll)]

        public SehirTanim SehirTanim
        {
            get => sehirTanim;
            set => SetPropertyValue(nameof(SehirTanim), ref sehirTanim, value);
        }

        [XafDisplayName("Tam Adresi")]
        public string TamAdresAdi
        {
            get
            {
                if (adres1 == null)
                    return null;
                else
                    return ObjectFormatter.Format(
                        "{Adres1}, {Adres2}, {PostaKodu}, {SehirTanim}, {UlkeTanim}", this);
            }
        }
    }
}