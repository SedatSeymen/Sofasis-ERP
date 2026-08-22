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
    // Process-genelinde önbellek (22.08.2026 denetimi G11): tek-satırlık GenelParametre
    // her DetailView/ListView açılışında ve her Döviz Kodu/Kuru değişiminde tekrar
    // sorgulanıyordu. GenelParametre.OnSaved bu önbelleği geçersiz kılar (bkz. o dosya).
    static readonly object kilit = new();
    static (int MiktarHane, int TutarHane, int KurHane)? onbellek;

    public static (int MiktarHane, int TutarHane, int KurHane) OndalikHaneleriniOku(IObjectSpace objectSpace)
    {
        lock (kilit)
        {
            if (onbellek.HasValue)
                return onbellek.Value;
        }

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

        var sonuc = (miktarHane, tutarHane, kurHane);
        lock (kilit) { onbellek = sonuc; }
        return sonuc;
    }

    public static void OnbellegiTemizle()
    {
        lock (kilit) { onbellek = null; }
    }
}
