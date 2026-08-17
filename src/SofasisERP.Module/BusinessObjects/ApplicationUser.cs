/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ApplicationUser.cs
 * Oluşturma Tarihi : 2026-08-17
 * Oluşturan        : Sofasis Development Team
 * Son Güncelleme   : 2026-08-17
 * Son Güncelleyen  : Sofasis Development Team
 * Açıklama         : XAF Security System kullanıcı sınıfı
 * ****************************************************************************
 */

using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace Sofasis.Module.BusinessObjects
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    [DefaultProperty(nameof(UserName))]
    public class ApplicationUser : PermissionPolicyUser, ISecurityUserWithLoginInfo
    {
        public ApplicationUser(Session session) : base(session)
        {
        }

        IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins
            => new List<ISecurityUserLoginInfo>();

        ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey)
        {
            return null;
        }
    }
}
