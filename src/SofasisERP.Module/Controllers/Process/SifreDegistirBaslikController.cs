/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SifreDegistirBaslikController.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Kullanıcı Detaylarım" ekranındaki parola değiştirme butonu
 *                    otomatik tr-TR çevirisiyle "Değişmek benim parola" görünüyordu.
 *                    Model.DesignedDiffs.xafml üzerinden ActionDesign/Actions ile
 *                    Caption override'ı DENENDİ (hem "ChangeMyPassword" hem doğru Id
 *                    olan "ChangePasswordByUser" ile, IsNewNode olan/olmayan) — hiçbiri
 *                    etkili olmadı; ChangePasswordController muhtemelen Caption'ı
 *                    kendi OnActivated'ında kod tarafında yeniden atıyor, Model
 *                    değerini eziyor. Bu yüzden AYNI noktada (OnActivated) kod
 *                    tarafından doğrudan üzerine yazılıyor — garantili çalışan tek yol.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;

namespace SofasisERP.Module.Controllers.Process;

public class SifreDegistirBaslikController : ViewController
{
    protected override void OnActivated()
    {
        base.OnActivated();
        ChangePasswordController changePasswordController = Frame.GetController<ChangePasswordController>();
        if (changePasswordController?.ChangeMyPasswordAction != null)
            changePasswordController.ChangeMyPasswordAction.Caption = "Şifre Değiştir";
    }
}
