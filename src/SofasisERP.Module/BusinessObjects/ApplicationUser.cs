/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ApplicationUser.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : XAF Security System kullanıcı sınıfı
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    [DefaultProperty(nameof(UserName))]
    public class ApplicationUser : PermissionPolicyUser, ISecurityUserWithLoginInfo, ISecurityUserLockout
    {
        int accessFailedCount;
        DateTime lockoutEnd;
        MediaDataObject resim;

        public ApplicationUser(Session session) : base(session) { }

        // Kullanıcı fotoğrafı — ModelTanim.Resim ile aynı desen (MediaDataObject): DB'de
        // saklanır, gecikmeli yüklenir, tarayıcı tarafında önbelleklenir.
        [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorCustomHeight = 32)]
        public MediaDataObject Resim
        {
            get => resim;
            set => SetPropertyValue(nameof(Resim), ref resim, value);
        }

        [Browsable(false)]
        public int AccessFailedCount
        {
            get { return accessFailedCount; }
            set { SetPropertyValue(nameof(AccessFailedCount), ref accessFailedCount, value); }
        }

        [Browsable(false)]
        public DateTime LockoutEnd
        {
            get { return lockoutEnd; }
            set { SetPropertyValue(nameof(LockoutEnd), ref lockoutEnd, value); }
        }

        [Browsable(false)]
        [NonCloneable]
        [Aggregated, Association("User-LoginInfo")]
        public XPCollection<ApplicationUserLoginInfo> LoginInfo
        {
            get { return GetCollection<ApplicationUserLoginInfo>(nameof(LoginInfo)); }
        }

        IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => LoginInfo.OfType<ISecurityUserLoginInfo>();

        ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey)
        {
            ApplicationUserLoginInfo result = new ApplicationUserLoginInfo(Session);
            result.LoginProviderName = loginProviderName;
            result.ProviderUserKey = providerUserKey;
            result.User = this;
            return result;
        }
    }
}
