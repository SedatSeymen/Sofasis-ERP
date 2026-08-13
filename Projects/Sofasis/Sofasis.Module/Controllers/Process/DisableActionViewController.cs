using System;
using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Blazor.Server.Controllers
{
    public partial class DisableActionViewController : ViewController
    {
        IObjectSpace os = null;

        // OnActivated yalnızca view AÇILIRKEN bir kez çalışır — aynı sekmede kalınıp ilk kayıt
        // kaydedildiğinde (listeye dönmeden/yeniden açmadan) "Yeni" butonunun da anında kapanması
        // için ObjectSpace.Committed olayına abone olunur ve aynı kontrol yeniden çalıştırılır
        // (canlı testte bulunan gerçek bug: bu olmadan aynı ListView'de 2. kayıt engellenmeden
        // oluşturulabiliyordu — tek seferlik OnActivated kontrolü kaydedilen ilk kayıttan sonra
        // hiç yeniden değerlendirilmiyordu).
        Action recomputeSingletonLock = null;

        public DisableActionViewController()
        {
            InitializeComponent();
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            os = View.ObjectSpace;

            if (View.Id == "CariHesapHareketleri_ListView" || View.Id == "CariHesapHareketleri_DetailView")
            {
                base.View.AllowNew[View.Id] = false;
                base.View.AllowDelete[View.Id] = false;
            }
            if (View.Id == "SatinAlmaParametre_ListView" || View.Id == "SatinAlmaParametre_DetailView")
            {
                recomputeSingletonLock = () => ApplySingletonLock(() =>
                    Application.CreateObjectSpace(typeof(SatinAlmaParametre)).GetObjects<SatinAlmaParametre>().Count);
            }
            if (View.Id == "SatisSiparisParametre_ListView" || View.Id == "SatisSiparisParametre_DetailView")
            {
                recomputeSingletonLock = () => ApplySingletonLock(() =>
                    Application.CreateObjectSpace(typeof(SatisSiparisParametre)).GetObjects<SatisSiparisParametre>().Count);
            }
            // Eksikti — StokParametre ve MaliyetParametre de tekil (singleton) parametre
            // sınıfları, diğer parametre sınıflarıyla aynı korumayı taşımalı.
            if (View.Id == "StokParametre_ListView" || View.Id == "StokParametre_DetailView")
            {
                recomputeSingletonLock = () => ApplySingletonLock(() =>
                    Application.CreateObjectSpace(typeof(StokParametre)).GetObjects<StokParametre>().Count);
            }
            if (View.Id == "MaliyetParametre_ListView" || View.Id == "MaliyetParametre_DetailView")
            {
                recomputeSingletonLock = () => ApplySingletonLock(() =>
                    Application.CreateObjectSpace(typeof(MaliyetParametre)).GetObjects<MaliyetParametre>().Count);
            }
            if (View.Id == "GenelParametre_ListView" || View.Id == "GenelParametre_DetailView")
            {
                recomputeSingletonLock = () => ApplySingletonLock(() =>
                    Application.CreateObjectSpace(typeof(GenelParametre)).GetObjects<GenelParametre>().Count);
            }

            if (recomputeSingletonLock != null)
            {
                recomputeSingletonLock();
                View.ObjectSpace.Committed += ObjectSpace_Committed;
            }
        }

        void ObjectSpace_Committed(object sender, EventArgs e) => recomputeSingletonLock?.Invoke();

        void ApplySingletonLock(Func<int> countOfExisting)
        {
            bool locked = countOfExisting() > 0;
            base.View.AllowNew[View.Id] = !locked;
            base.View.AllowDelete[View.Id] = !locked;
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
        }
        protected override void OnDeactivated()
        {
            if (recomputeSingletonLock != null)
            {
                View.ObjectSpace.Committed -= ObjectSpace_Committed;
                recomputeSingletonLock = null;
            }
            base.OnDeactivated();
        }
    }
}
