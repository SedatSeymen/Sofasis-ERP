using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("StokAdi")]
    [XafDisplayName("Stok Tanımlama")]
    [DefaultClassOptions]

    public class StokTanim : BaseClassWithAuditAndDescription
    {
        public StokTanim(Session session)
            : base(session)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if ((Session.DataLayer != null) && Session.IsNewObject(this))
            {
                this.StokSiparisSeviyeKontrolu = StokSiparisSeviyeKontrolu.Yapılmasın;
                this.StokTipi = StokTipi.Hammadde;

                StokGrupTanim grup = Session.GetVarsayilan<StokGrupTanim>();
                if (grup != null) this.StokGrupTanim = grup;

                BirimTanim birim = Session.GetVarsayilan<BirimTanim>();
                if (birim != null) this.BirimTanim = birim;

                KDVTanim kDV = Session.GetVarsayilan<KDVTanim>();
                if (kDV != null) this.KDVTanim = kDV;

                DovizTanim Doviz = Session.GetVarsayilan<DovizTanim>();
                if (Doviz != null) this.DovizTanim = Doviz;

                DepoTanim Depo = Session.GetVarsayilan<DepoTanim>();
                if (Depo != null) this.GirisDeposu = Depo;

                StokKodu = Helper.ConstNewRecordText;
            }

        }



        string stokAdiIngilizce;
        StokHizmetMasrafTipi stokHizmetMasrafTipi;
        decimal metrekare;
        decimal metreKup;
        decimal agirlik;
        StokModelTanim stokModelTanim;
        StokTipi stokTipi;
        DepoTanim girisDeposu;
        decimal yukseklik;
        decimal boy;
        decimal en;
        DovizTanim dovizTanim;
        decimal sonAlisFiyati;
        decimal? alisFiyati;
        StokSiparisSeviyeKontrolu stokSiparisSeviyeKontrolu;
        KDVTanim kDVTanim;
        decimal siparisStokSeviyesi;
        decimal kritikStokSeviyesi;
        decimal maximumStokSeviyesi;
        decimal minimumStokSeviyesi;
        BirimTanim birimTanim;
        StokGrupTanim stokGrupTanim;
        string stokBarkodNo;
        string stokAdi;
        string stokKodu;
        FisTuruTanim fisTuruTanim;
        decimal toplamMiktar;
        decimal ortalamaMaliyet;

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [XafDisplayName("Stok/Hizmet/Masraf Tipi")]
        public StokHizmetMasrafTipi StokHizmetMasrafTipi
        {
            get => stokHizmetMasrafTipi;
            set => SetPropertyValue(nameof(StokHizmetMasrafTipi), ref stokHizmetMasrafTipi, value);
        }


        [Size(32)]
        [XafDisplayName("Stok Kodu")]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "StokKodu != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [RuleRequiredField("RuleRequired_StokTanim_StokKodu", DefaultContexts.Save, "Lütfen Kodu Giriniz...")]
        [Appearance("ED_StokTanim_StokKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public string StokKodu
        {
            get => stokKodu;
            set => SetPropertyValue(nameof(StokKodu), ref stokKodu, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [XafDisplayName("Stok Adı")]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [RuleRequiredField("RuleRequired_StokTanim_StokAdi", DefaultContexts.Save, "Lütfen Adını Giriniz...")]
        public string StokAdi
        {
            get => stokAdi;
            set
            {
                string OldValue = stokAdi;
                stokAdi = value;
                OnChanged(nameof(StokAdi), OldValue, StokAdi);
            }
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [XafDisplayName("Stok Adı İngilizce")]
        public string StokAdiIngilizce
        {
            get => stokAdiIngilizce;
            set => SetPropertyValue(nameof(StokAdiIngilizce), ref stokAdiIngilizce, value);
        }

        [XafDisplayName("Stok Tipi")]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]

        public StokTipi StokTipi
        {
            get => stokTipi;
            set
            {
                SetPropertyValue(nameof(StokTipi), ref stokTipi, value);
                if (!IsLoading)
                {
                    if (StokTipi == StokTipi.Mamul)
                    {
                        var parametre = Session.GetSingleton<StokParametre>();
                        if (parametre != null)
                            StokGrupTanim = parametre.KoltukStokGrubu;
                    }
                }
            }
        }

        [XafDisplayName("Model Adı")]
        [Appearance("IsVisible_StokTanim_ModelAdi", Visibility = ViewItemVisibility.Hide, Criteria = "!StokTipi==0", Context = "DetailView")]

        public StokModelTanim StokModelTanim
        {
            get => stokModelTanim;
            set => SetPropertyValue(nameof(StokModelTanim), ref stokModelTanim, value);
        }

        [Size(32)]
        [XafDisplayName("Barkod No")]
        [VisibleInListView(false)]
        public string StokBarkodNo
        {
            get => stokBarkodNo;
            set => SetPropertyValue(nameof(StokBarkodNo), ref stokBarkodNo, value);
        }

        [XafDisplayName("Grup Adı")]
        //[RuleRequiredField("RuleRequired_StokTanim_StokGrupTanim", DefaultContexts.Save, "Lütfen Stok Grubunu Giriniz...")]

        public StokGrupTanim StokGrupTanim
        {
            get => stokGrupTanim;
            set => SetPropertyValue(nameof(StokGrupTanim), ref stokGrupTanim, value);
        }

        [XafDisplayName("Birim Adı")]
        public BirimTanim BirimTanim
        {
            get => birimTanim;
            set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
        }

        [XafDisplayName("KDV Oranı")]
        [VisibleInListView(false)]
        public KDVTanim KDVTanim
        {
            get => kDVTanim;
            set => SetPropertyValue(nameof(KDVTanim), ref kDVTanim, value);
        }

        [XafDisplayName("Giriş Deposu")]
        [VisibleInListView(false)]
        public DepoTanim GirisDeposu
        {
            get => girisDeposu;
            set => SetPropertyValue(nameof(GirisDeposu), ref girisDeposu, value);
        }

        // NOT: Manuel/referans "liste" fiyatıdır — Faz 2'nin motor tarafından hesapladığı
        // OrtalamaMaliyet ile KARIŞTIRILMAMALI. Bu alanın maliyet motoruna bağlanıp
        // bağlanmayacağı (ör. Alış Faturası onayında otomatik güncelleme) Faz 3 kararıdır.
        [XafDisplayName("Alış Fiyatı")]
        [VisibleInListView(false)]

        public decimal? AlisFiyati
        {
            get => alisFiyati;
            set
            {
                if (AlisFiyati != value)
                {
                    decimal? OldValue = alisFiyati;
                    alisFiyati = value;
                    OnChanged(nameof(AlisFiyati), OldValue, AlisFiyati);
                }
            }
        }

        [XafDisplayName("Son Alış Fiyatı")]
        [VisibleInListView(false)]

        public decimal SonAlisFiyati
        {
            get => sonAlisFiyati;
            set
            {
                if (SonAlisFiyati != value)
                {
                    decimal OldValue = sonAlisFiyati;
                    sonAlisFiyati = value;
                    OnChanged(nameof(SonAlisFiyati), OldValue, SonAlisFiyati);
                }
            }
        }

        [XafDisplayName("Döviz Kodu")]
        [VisibleInListView(false)]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Sipariş Seviye Kontrolu")]
        [VisibleInListView(false)]

        public StokSiparisSeviyeKontrolu StokSiparisSeviyeKontrolu
        {
            get => stokSiparisSeviyeKontrolu;
            set => SetPropertyValue(nameof(StokSiparisSeviyeKontrolu), ref stokSiparisSeviyeKontrolu, value);
        }

        [XafDisplayName("Minimum Stok Seviyesi")]
        [VisibleInListView(false)]
        [Appearance("ED_StokTanim_MinimumStokSeviyesi", Enabled = false, Criteria = "StokSiparisSeviyeKontrolu = 1", Context = "DetailView")]

        public decimal MinimumStokSeviyesi
        {
            get => minimumStokSeviyesi;
            set => SetPropertyValue(nameof(MinimumStokSeviyesi), ref minimumStokSeviyesi, value);
        }

        [XafDisplayName("Maximum Stok Seviyesi")]
        [VisibleInListView(false)]
        [Appearance("ED_StokTanim_MaximumStokSeviyesi", Enabled = false, Criteria = "StokSiparisSeviyeKontrolu = 1", Context = "DetailView")]
        public decimal MaximumStokSeviyesi
        {
            get => maximumStokSeviyesi;
            set => SetPropertyValue(nameof(MaximumStokSeviyesi), ref maximumStokSeviyesi, value);
        }

        [XafDisplayName("Kritik Stok Seviyesi")]
        [VisibleInListView(false)]
        [Appearance("ED_StokTanim_KritikStokSeviyesi", Enabled = false, Criteria = "StokSiparisSeviyeKontrolu = 1", Context = "DetailView")]
        public decimal KritikStokSeviyesi
        {
            get => kritikStokSeviyesi;
            set => SetPropertyValue(nameof(KritikStokSeviyesi), ref kritikStokSeviyesi, value);
        }

        [XafDisplayName("Sipariş Stok Seviyesi")]
        [VisibleInListView(false)]
        [Appearance("ED_StokTanim_SiparisStokSeviyesi", Enabled = false, Criteria = "StokSiparisSeviyeKontrolu = 1", Context = "DetailView")]
        public decimal SiparisStokSeviyesi
        {
            get => siparisStokSeviyesi;
            set => SetPropertyValue(nameof(SiparisStokSeviyesi), ref siparisStokSeviyesi, value);
        }

        [XafDisplayName("En (Cm)")]
        [VisibleInListView(false)]
        public decimal En
        {
            get => en;
            set
            {
                if (En != value)
                {
                    decimal OldValue = en;
                    en = value;
                    OnChanged(nameof(En), OldValue, En);
                }
            }
        }

        [XafDisplayName("Boy (Cm)")]
        [VisibleInListView(false)]
        public decimal Boy
        {
            get => boy;
            set
            {
                if (Boy != value)
                {
                    decimal OldValue = boy;
                    boy = value;
                    OnChanged(nameof(Boy), OldValue, Boy);
                }
            }
        }

        [XafDisplayName("Yükseklik (Cm)")]
        [VisibleInListView(false)]
        public decimal Yukseklik
        {
            get => yukseklik;
            set
            {
                if (Yukseklik != value)
                {
                    decimal OldValue = yukseklik;
                    yukseklik = value;
                    OnChanged(nameof(Yukseklik), OldValue, Yukseklik);
                }
            }
        }
        [VisibleInListView(false)]
        [XafDisplayName("MetreKare")]
        [Appearance("ED_StokTanim_MetreKare", Enabled = false, Criteria = "", Context = "DetailView")]
        public decimal Metrekare
        {
            get => metrekare;
            set => SetPropertyValue(nameof(Metrekare), ref metrekare, value);
        }

        [VisibleInListView(false)]
        [XafDisplayName("MetreKüp")]
        [Appearance("ED_StokTanim_MetreKup", Enabled = false, Criteria = "", Context = "DetailView")]
        public decimal MetreKup
        {
            get => metreKup;
            set => SetPropertyValue(nameof(MetreKup), ref metreKup, value);
        }

        [VisibleInListView(false)]
        [XafDisplayName("Ağırlık")]
        public decimal Agirlik
        {
            get => agirlik;
            set => SetPropertyValue(nameof(Agirlik), ref agirlik, value);
        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_StokTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        // Faz 2: StokHareketleriD.ObjectSaving() (IWeightedAverageCostService aracılığıyla) tarafından
        // yazılır — kullanıcı tarafından elle değiştirilmez. Tüm depoların toplamıdır (ağırlıklı
        // ortalama maliyet stok bazında TEK tutulur; depo bazında miktar için bkz. StokBakiye).
        [DbType("decimal(18,4)")]
        [XafDisplayName("Toplam Miktar")]
        [VisibleInListView(false)]
        [Appearance("ED_StokTanim_ToplamMiktar", Enabled = false, Criteria = "", Context = "DetailView")]
        public decimal ToplamMiktar
        {
            get => toplamMiktar;
            set => SetPropertyValue(nameof(ToplamMiktar), ref toplamMiktar, value);
        }

        [DbType("decimal(28,6)")]
        [XafDisplayName("Ortalama Maliyet")]
        [VisibleInListView(false)]
        [Appearance("ED_StokTanim_OrtalamaMaliyet", Enabled = false, Criteria = "", Context = "DetailView")]
        public decimal OrtalamaMaliyet
        {
            get => ortalamaMaliyet;
            set => SetPropertyValue(nameof(OrtalamaMaliyet), ref ortalamaMaliyet, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                StokKodu == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                StokKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, DateTime.UtcNow);
            }
            base.OnSaving();
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            if (propertyName == "En" || propertyName == "Boy" || propertyName == "Yukseklik")
            {
                if (En != 0 && Boy != 0)
                    Metrekare = (En * Boy) / 10000;
                if (En != 0 && Boy != 0 && Yukseklik != 0)
                    MetreKup = (En * Boy * Yukseklik) / 1000000;
            }
        }

        protected override void OnDeleting()
        {
            try
            {
                // B1: Silme koruması aktif — kullanımdaki stok silinemez.
                if (new XPQuery<SatisSiparisD>(Session).Count(x => x.StokTanim != null && x.StokTanim == this) > 0)
                    throw new UserFriendlyException("Bu stok siparişlerde kullanılmış, silemezsiniz.");
                if (new XPQuery<SatisFiyatListeD>(Session).Count(x => x.StokTanim != null && x.StokTanim == this) > 0)
                    throw new UserFriendlyException("Bu stok fiyat listelerinde kullanılmış, silemezsiniz.");
                if (new XPQuery<ReceteTanimM>(Session).Count(x => x.StokTanim != null && x.StokTanim == this) > 0)
                    throw new UserFriendlyException("Bu stok reçetelerde kullanılmış, silemezsiniz.");
                if (new XPQuery<ReceteTanimD>(Session).Count(x => x.StokTanim != null && x.StokTanim == this) > 0)
                    throw new UserFriendlyException("Bu stok reçete satırlarında kullanılmış, silemezsiniz.");
                if (new XPQuery<StokHareketleriD>(Session).Count(x => x.StokTanim != null && x.StokTanim == this) > 0)
                    throw new UserFriendlyException("Bu stok hareket görmüş, silemezsiniz.");
                if (new XPQuery<StokBakiye>(Session).Count(x => x.StokTanim != null && x.StokTanim == this) > 0)
                    throw new UserFriendlyException("Bu stoğun bakiyesi bulunuyor, silemezsiniz.");
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