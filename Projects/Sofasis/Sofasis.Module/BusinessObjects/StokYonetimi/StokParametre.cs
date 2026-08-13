using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]

    [XafDisplayName("Stok Parametre Tanımlama")]
    public class StokParametre : BaseClass
    {
        public StokParametre(Session session)
            : base(session)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                XPCollection<StokParametre> entityList = new XPCollection<StokParametre>(Session);
                int count = entityList.Count;
                if (count == 1)
                {
                    this.CancelEdit();
                    StokParametre entity = entityList[0];
                    Session.DropChanges();
                    Session.Reload(entity);


                }
            }
        }



        StokGrupTanim ayakStokGrubu;
       StokGrupTanim kirlentStokGrubu;
        StokGrupTanim sungerStokGrubu;
        StokGrupTanim koltukStokGrubu;
        StokGrupTanim kumasStokGrubu;
        NegatifStokPolitikasi negatifStokPolitikasi = NegatifStokPolitikasi.Uyar;

        // Bir depoda mevcut bakiyeyi aşan bir çıkış yapılmaya çalışıldığında izlenecek politika.
        // Bu alan grup-varsayılanlarının aksine (bkz. yukarıdaki [Appearance Enabled=false]) her
        // zaman düzenlenebilir kalır — işletme büyüdükçe politika sıkılaştırılabilmeli.
        [XafDisplayName("Negatif Stok Politikası")]
        public NegatifStokPolitikasi NegatifStokPolitikasi
        {
            get => negatifStokPolitikasi;
            set => SetPropertyValue(nameof(NegatifStokPolitikasi), ref negatifStokPolitikasi, value);
        }

        [XafDisplayName("Koltuk Stok Grubu")]
        [Appearance("ED_StokParametre_KoltukStokGrubu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public StokGrupTanim KoltukStokGrubu
        {
            get => koltukStokGrubu;
            set => SetPropertyValue(nameof(KoltukStokGrubu), ref koltukStokGrubu, value);
        }

        [XafDisplayName("Kumaş Stok Grubu")]
        [Appearance("ED_StokParametre_KumasStokGrubu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public StokGrupTanim KumasStokGrubu
        {
            get => kumasStokGrubu;
            set => SetPropertyValue(nameof(KumasStokGrubu), ref kumasStokGrubu, value);
        }

        [XafDisplayName("Sünger Stok Grubu")]
        [Appearance("ED_StokParametre_SungerStokGrubu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public StokGrupTanim SungerStokGrubu
        {
            get => sungerStokGrubu;
            set => SetPropertyValue(nameof(SungerStokGrubu), ref sungerStokGrubu, value);
        }

        [XafDisplayName("Kırlent Stok Grubu")]
        [Appearance("ED_StokParametre_KirlentStokGrubu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public StokGrupTanim KirlentStokGrubu
        {
            get => kirlentStokGrubu;
            set => SetPropertyValue(nameof(KirlentStokGrubu), ref kirlentStokGrubu, value);
        }

        [XafDisplayName("Ayak Stok Grubu")]
        [Appearance("ED_StokParametre_AyakStokGrubu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public StokGrupTanim AyakStokGrubu
        {
            get => ayakStokGrubu;
            set => SetPropertyValue(nameof(AyakStokGrubu), ref ayakStokGrubu, value);
        }

    }
}