/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ReportPreviewContext.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Rapor Controller'ları (ör. HesapEkstresiRaporuController) ile
 *                    Startup.cs'deki AddReports(options.Events.OnBeforeShowPreview)
 *                    arasındaki köprü — eski ERP'den (D:\2025\SofasisERP\...\Sofasis.
 *                    Module\Services\ReportPreviewContext.cs) aynen taşındı. Launching
 *                    Controller, ShowPreview(handle, criteria) çağrısından ÖNCE
 *                    hesapladığı gizli Parameter değerlerini (ör. Hesap Adı, Devreden
 *                    Bakiye) buraya koyar; ShowPreview yalnızca bir "handle" string
 *                    aldığından Controller'dan XtraReport örneğine doğrudan erişilemez.
 *                    Scoped DI ömrü (Blazor Server circuit/istek başına) — kullanıcılar
 *                    arası veri karışmasını önler.
 * ****************************************************************************
 */

using System.Collections.Generic;

namespace SofasisERP.Module.Services;

public class ReportPreviewContext
{
    public object DataSource { get; set; }
    public Dictionary<string, object> ParameterValues { get; } = new();
}
