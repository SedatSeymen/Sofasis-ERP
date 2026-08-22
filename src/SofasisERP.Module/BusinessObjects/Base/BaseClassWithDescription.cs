/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BaseClassWithDescription.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Açıklama alanları içeren temel sınıf (audit bilgisi olmadan)
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;


namespace SofasisERP.Module.BusinessObjects
{
    [NonPersistent]
    public class BaseClassWithDescription : BaseClass
    {
        public BaseClassWithDescription() : base()
        {
        }

        public BaseClassWithDescription(Session session) : base(session)
        {
        }

        // 4-ajanlı testte (2026-08-22, yazılım mühendisi bulgusu) tespit edildi: CustomCode1/
        // CustomCode2/Description alanları BaseClassWithAuditAndDescription.cs'de BİREBİR
        // tekrarlanıyor. Bu, C#'ın tekli kalıtım kısıtının kaçınılmaz sonucu — "Audit'siz
        // Description" ve "Audit'li Description" ayrı dallarda var olduğundan (BaseClassWithAudit
        // ARADA), alanları TEK bir ortak atadan miras almak, ya BaseClassWithAudit'i (Description
        // istemeyen tüm kullanıcılarına da bu alanları zorlayarak) ya da bu sınıfı (Audit
        // istemeyen kullanıcılarına Audit alanlarını zorlayarak) DEĞİŞTİRMEYİ gerektirir — her
        // ikisi de DÜZİNELERCE var olan tablonun şemasını (ve üretim verisini) etkiler. Composition
        // (Aggregated ilişkili nesne) ile çözmek de aynı şekilde şema/veri taşıma riski taşır.
        // Bu yüzden BİLİNÇLİ OLARAK duplike bırakıldı — DEĞİŞTİRİRSEN, BaseClassWithAuditAndDescription.
        // cs'deki birebir kopyayı da GÜNCELLE.
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
