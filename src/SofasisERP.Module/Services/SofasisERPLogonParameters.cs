/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SofasisERPLogonParameters.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Giriş ekranında Kullanıcı Adı'nı varsayılan "Admin" ile
 *                    dolu getirir. XafApplication.LastLogonParametersRead event'i
 *                    resmi dokümanların önerdiği yöntem olsa da, yeni Blazor
 *                    /LoginPage akışında (klasik PopupWindowShowAction akışından
 *                    farklı) güvenilmez şekilde tetikleniyor — kullanıcı bunu canlı
 *                    testte doğruladı (bazen dolu geliyor, çoğunlukla boş). Bunun
 *                    yerine LogonParameters nesnesinin KENDİSİNDE varsayılan değer
 *                    atamak, event zamanlamasına bağlı olmadığı için her zaman
 *                    çalışır (bkz. Startup.cs — options.LogonParametersType).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.Security;

namespace SofasisERP.Module.Services;

public class SofasisERPLogonParameters : AuthenticationStandardLogonParameters
{
    public SofasisERPLogonParameters()
    {
        UserName = "Admin";
    }
}
