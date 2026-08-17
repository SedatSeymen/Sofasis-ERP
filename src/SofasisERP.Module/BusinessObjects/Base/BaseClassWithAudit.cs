/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BaseClassWithAudit.cs
 * Oluşturma Tarihi : [İlk oluşturma tarihi korunacaktır]
 * Oluşturan        : [İlk oluşturan korunacaktır]
 * Son Güncelleme   : [Dosya değişiklik tarihi]
 * Son Güncelleyen  : [Dosyayı değiştiren]
 * Açıklama         : BaseClassWithAudit sınıfı
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;



namespace Sofasis.Module.BusinessObjects
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

        // Oturum açmış kullanıcıyı getir
        ApplicationUser GetCurrentUser()
        {
            return Session.FindObject<ApplicationUser>(CriteriaOperator.Parse("Oid=CurrentUserId()"));
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
