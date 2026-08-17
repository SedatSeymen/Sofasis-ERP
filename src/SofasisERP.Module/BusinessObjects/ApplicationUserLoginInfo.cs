/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ApplicationUserLoginInfo.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : ApplicationUser için giriş sağlayıcı (login provider) bilgisi
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects
{
    [DeferredDeletion(false)]
    [Persistent("PermissionPolicyUserLoginInfo")]
    public class ApplicationUserLoginInfo : BaseObject, ISecurityUserLoginInfo
    {
        string loginProviderName;
        ApplicationUser user;
        string providerUserKey;
        public ApplicationUserLoginInfo(Session session) : base(session) { }

        [Indexed("ProviderUserKey", Unique = true)]
        [Appearance("PasswordProvider", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
        public string LoginProviderName
        {
            get { return loginProviderName; }
            set { SetPropertyValue(nameof(LoginProviderName), ref loginProviderName, value); }
        }

        [Appearance("PasswordProviderUserKey", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
        public string ProviderUserKey
        {
            get { return providerUserKey; }
            set { SetPropertyValue(nameof(ProviderUserKey), ref providerUserKey, value); }
        }

        [Association("User-LoginInfo")]
        public ApplicationUser User
        {
            get { return user; }
            set { SetPropertyValue(nameof(User), ref user, value); }
        }

        object ISecurityUserLoginInfo.User => User;
    }
}
