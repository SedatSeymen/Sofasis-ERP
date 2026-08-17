/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BaseClassWithAudit.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Oluşturma ve değiştirme bilgilerini tutan temel sınıf
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;

namespace SofasisERP.Module.BusinessObjects
{
    [NonPersistent]
    public class BaseClassWithAudit : BaseClass
    {
        ApplicationUser user { get; set; }

        public BaseClassWithAudit() : base()
        {
        }

        public BaseClassWithAudit(Session session) : base(session)
        {
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            user = GetCurrentUser();

            if (user != null)
            {
                // Yeni kayıt ise oluşturma bilgilerini doldur
                if (Session.IsNewObject(this))
                {
                    CreatedDate = DateTime.Now;
                    CreatedBy = user;
                }
                else
                {
                    // Mevcut kayıt ise değiştirme bilgilerini güncelle
                    ModifiedDate = DateTime.Now;
                    ModifiedBy = user;
                }
            }
        }

        // NOT: DevExpress'in kendi analizörü (XAF0035) "SecuritySystem" statik
        // üyelerinin (ve dolayısıyla eski "CurrentUserId()" criteria fonksiyonunun)
        // XAF Blazor'da KULLANILMAMASINI söylüyor - ValueManager ASP.NET Core
        // pipeline'ında garanti başlatılmamış olabiliyor, runtime hatasına yol
        // açabiliyor. Resmi önerilen yöntem: Session.ServiceProvider üzerinden
        // DI ile ISecurityStrategyBase.UserId almak. Security System henüz
        // bu projede aktif değil (bkz. PRODUCT_EDITIONS.md - Faz 7); bu yüzden
        // servis bulunamazsa (henüz .AddSecurity() çağrılmadıysa) sessizce null
        // döner - CreatedBy/ModifiedBy o zamana kadar boş kalır, Security
        // kurulunca ek değişiklik gerekmeden çalışmaya başlar.
        ApplicationUser GetCurrentUser()
        {
            var securityStrategy = Session.ServiceProvider?.GetService<ISecurityStrategyBase>();
            if (securityStrategy?.UserId == null) return null;
            return Session.GetObjectByKey<ApplicationUser>(securityStrategy.UserId);
        }

        DateTime modifiedDate;
        DateTime createdDate;
        ApplicationUser modifiedBy;
        ApplicationUser createdBy;

        [Size(32)]
        [VisibleInListView(false)]
        [VisibleInDashboards(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [ModelDefault("AllowEdit", "False")]
        [XafDisplayName("Oluşturan Kullanıcı")]
        public ApplicationUser CreatedBy
        {
            get => createdBy;
            set => SetPropertyValue(nameof(CreatedBy), ref createdBy, value);
        }

        [Size(32)]
        [VisibleInListView(false)]
        [VisibleInDashboards(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [ModelDefault("AllowEdit", "False")]
        [XafDisplayName("Değiştiren Kullanıcı")]
        public ApplicationUser ModifiedBy
        {
            get => modifiedBy;
            set => SetPropertyValue(nameof(ModifiedBy), ref modifiedBy, value);
        }

        [VisibleInListView(false)]
        [VisibleInDashboards(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [ModelDefault("AllowEdit", "False"), ModelDefault("DisplayFormat", "G")]
        [XafDisplayName("Oluşturma Tarihi")]
        public DateTime CreatedDate
        {
            get => createdDate;
            set => SetPropertyValue(nameof(CreatedDate), ref createdDate, value);
        }

        [VisibleInListView(false)]
        [VisibleInDashboards(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [ModelDefault("AllowEdit", "False"), ModelDefault("DisplayFormat", "G")]
        [XafDisplayName("Değiştirme Tarihi")]
        public DateTime ModifiedDate
        {
            get => modifiedDate;
            set => SetPropertyValue(nameof(ModifiedDate), ref modifiedDate, value);
        }
    }
}
