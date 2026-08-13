/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BaseClassWithAuditAndDescription.cs
 * Oluşturma Tarihi : 12/30/2025 6:48:55 PM
 * Oluşturan        : Sedat Seymen
 * Açıklama         : Audit bilgileri ve açıklama alanları içeren temel sınıf
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;



namespace Sofasis.Module.BusinessObjects
{
    [NonPersistent]
    public class BaseClassWithAuditAndDescription : BaseClassWithAudit
    {
        public BaseClassWithAuditAndDescription() : base()
        {
        }

        public BaseClassWithAuditAndDescription(Session session) : base(session)
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
        }

        string customCode2;
        string customCode1;
        string description;

        [Size(32)]
        [VisibleInListView(false)]
        [XafDisplayName("Özel Kod 1")]
        public string CustomCode1
        {
            get => customCode1;
            set => SetPropertyValue(nameof(CustomCode1), ref customCode1, value);
        }

        [Size(32)]
        [VisibleInListView(false)]
        [XafDisplayName("Özel Kod 2")]
        public string CustomCode2
        {
            get => customCode2;
            set => SetPropertyValue(nameof(CustomCode2), ref customCode2, value);
        }

        [Size(200)]
        [VisibleInListView(false)]
        [ModelDefault("RowCount", "3")]
        [XafDisplayName("Açıklama")]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }
    }
}
