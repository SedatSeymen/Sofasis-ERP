/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TumCarilerBakiyeRaporu.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Tüm Cari hesapların Borç/Alacak/Bakiye durumunu tek tabloda
 *                    listeleyen özet rapor — isInplaceReport=false, özel Controller
 *                    (TumCarilerBakiyeRaporuController) ile tetiklenir (bkz. dosya
 *                    başı açıklaması, TumCarilerBakiyeRaporuBuilder.cs).
 * ****************************************************************************
 */

using DevExpress.XtraReports.UI;

namespace SofasisERP.Module.Reports;

public class TumCarilerBakiyeRaporu : XtraReport
{
    public TumCarilerBakiyeRaporu()
    {
        TumCarilerBakiyeRaporuBuilder.Build(this);
    }
}
