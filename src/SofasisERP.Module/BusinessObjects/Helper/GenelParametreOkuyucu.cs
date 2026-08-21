/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : GenelParametreOkuyucu.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : GenelParametre'den ondalik hane sayılarını okuma mantığı
 *                    (varsayılan değerler + hatayı yutup loglama) üç ayrı
 *                    controller'da (OndalikFormatController, OndalikFormatListController,
 *                    OndalikCanliFormatController) kopyalanmıştı — tek kaynağa indirgendi.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;

namespace SofasisERP.Module.BusinessObjects;

public static class GenelParametreOkuyucu
{
    public static (int MiktarHane, int TutarHane, int KurHane) OndalikHaneleriniOku(IObjectSpace objectSpace)
    {
        int miktarHane = 4, tutarHane = 2, kurHane = 6;
        try
        {
            GenelParametre parametre = objectSpace.GetObjects<GenelParametre>().FirstOrDefault();
            if (parametre != null)
            {
                miktarHane = (int)parametre.MiktarOndalikMaski;
                tutarHane = (int)parametre.TutarOndalikMaski;
                kurHane = (int)parametre.KurOndalikMaski;
            }
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError(ex);
        }

        return (miktarHane, tutarHane, kurHane);
    }
}
