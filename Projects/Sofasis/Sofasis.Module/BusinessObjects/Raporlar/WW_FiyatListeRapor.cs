using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using FileSystemData.BusinessObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Fiyat Listeleri Raporu")]
    public class WW_FiyatListeRapor : XPLiteObject
    {
        public WW_FiyatListeRapor(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        DateTime bitisTarihi;
        DateTime baslangicTarihi;
        string dovizAdi;
        string dovizKodu;
        decimal birimFiyat;
        decimal listeFiyati;
        string stokAdiIngilizce;
        string stokAdi;
        string stokModelAdiIngilizce;
        string stokModelAdi;
        decimal karOrani;
        decimal listeOrani;
        string fiyatListeAdi;
        string masterKeyID;
        string detayKeyID;

        [Key, Persistent]
        [Size(13)]
        public string DetayKeyID
        {
            get => detayKeyID;
            set => SetPropertyValue(nameof(DetayKeyID), ref detayKeyID, value);
        }


        [Size(13)]
        public string MasterKeyID
        {
            get => masterKeyID;
            set => SetPropertyValue(nameof(MasterKeyID), ref masterKeyID, value);
        }


        [Size(100)]
        [XafDisplayName("Fiyat Liste Adı")]
        public string FiyatListeAdi
        {
            get => fiyatListeAdi;
            set => SetPropertyValue(nameof(FiyatListeAdi), ref fiyatListeAdi, value);
        }

        [XafDisplayName("Liste Oranı")]
        public decimal ListeOrani
        {
            get => listeOrani;
            set => SetPropertyValue(nameof(ListeOrani), ref listeOrani, value);
        }

        [XafDisplayName("Kar Oranı")]
        public decimal KarOrani
        {
            get => karOrani;
            set => SetPropertyValue(nameof(KarOrani), ref karOrani, value);
        }


        [Size(100)]
        [XafDisplayName("Model Adı")]

        public string StokModelAdi
        {
            get => stokModelAdi;
            set => SetPropertyValue(nameof(StokModelAdi), ref stokModelAdi, value);
        }


        [Size(100)]
        [XafDisplayName("Model Adı İngilizce")]
        public string StokModelAdiIngilizce
        {
            get => stokModelAdiIngilizce;
            set => SetPropertyValue(nameof(StokModelAdiIngilizce), ref stokModelAdiIngilizce, value);
        }


        [Size(100)]
        [XafDisplayName("Stok Adı")]

        public string StokAdi
        {
            get => stokAdi;
            set => SetPropertyValue(nameof(StokAdi), ref stokAdi, value);
        }


        [Size(100)]
        [XafDisplayName("Stok Adı İngilizce")]
        public string StokAdiIngilizce
        {
            get => stokAdiIngilizce;
            set => SetPropertyValue(nameof(StokAdiIngilizce), ref stokAdiIngilizce, value);
        }

        [XafDisplayName("Liste Fiyatı")]
        public decimal ListeFiyati
        {
            get => listeFiyati;
            set => SetPropertyValue(nameof(ListeFiyati), ref listeFiyati, value);
        }

        [XafDisplayName("Birim Fiyatı")]
        public decimal BirimFiyat
        {
            get => birimFiyat;
            set => SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [XafDisplayName("Döviz Kodu")]

        public string DovizKodu
        {
            get => dovizKodu;
            set => SetPropertyValue(nameof(DovizKodu), ref dovizKodu, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [XafDisplayName("Döviz Adı")]
        public string DovizAdi
        {
            get => dovizAdi;
            set => SetPropertyValue(nameof(DovizAdi), ref dovizAdi, value);
        }

        [XafDisplayName("Başlangıç Tarihi")]
        public DateTime BaslangicTarihi
        {
            get => baslangicTarihi;
            set => SetPropertyValue(nameof(BaslangicTarihi), ref baslangicTarihi, value);
        }

        [XafDisplayName("Bitiş Tarihi")]
        public DateTime BitisTarihi
        {
            get => bitisTarihi;
            set => SetPropertyValue(nameof(BitisTarihi), ref bitisTarihi, value);
        }

        FileSystemStoreObject file;
        [XafDisplayName("Resim Yolu Ekle")]
        [DevExpress.Xpo.Aggregated, ExpandObjectMembers(ExpandObjectMembers.Never), ImmediatePostData]
        public FileSystemStoreObject File
        {
            get => file;
            set
            {
                if (file != null)
                {
                    file.Changed -= new ObjectChangeEventHandler(file_Changed);
                }
                SetPropertyValue<FileSystemStoreObject>("File", ref file, value);
                if (file != null)
                {
                    file.Changed += new ObjectChangeEventHandler(file_Changed);
                }
            }
        }

        private void file_Changed(object sender, ObjectChangeEventArgs e)
        {
            if (e.PropertyName == null)
            {
                OnChanged("Thumbnail");
            }
        }


        [XafDisplayName("Resim")]
        [ImageEditor(ListViewImageEditorCustomHeight = 75,
            DetailViewImageEditorFixedWidth = 200,
            DetailViewImageEditorFixedHeight = 150)]
        public byte[] Thumbnail
        {
            get { return Helper.GetImage(File); }

        }


    }
}