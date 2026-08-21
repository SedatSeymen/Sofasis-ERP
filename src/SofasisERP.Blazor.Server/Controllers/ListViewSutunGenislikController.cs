/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ListViewSutunGenislikController.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kullanıcı isteği (2026-08-18): tüm ListView'larda sütun
 *                    genişliklerinin elle sürüklenerek değiştirilebilmesi.
 *                    DevExpress Blazor DxGrid'de ColumnResizeMode varsayılanı
 *                    "Disabled" — canlı testte doğrulandı (sütun kenarında
 *                    col-resize imleci hiç çıkmıyor, sürükleme hiçbir etki
 *                    yapmıyor). Resmi DevExpress dokümantasyonunda önerilen
 *                    tek yol budur (DxGridListEditor.GridModel.ColumnResizeMode).
 *                    Genişlik değişikliği kalıcılığı (ModelDifference, kullanıcı
 *                    bazlı) ayrıca SofasisERPBlazorModule'de kuruldu — bu
 *                    Controller SADECE sürükleme özelliğini AÇAR, kayıt işini
 *                    XAF'ın kendi Model Difference altyapısı yapar.
 *
 *                    2026-08-18 genişletme: kullanıcının eski ERP'den verdiği
 *                    GridListController.cs'deki iki ek ayar da (Temp/ klasörü,
 *                    incelenip buraya birleştirildi — ayrı dosya açılmadı,
 *                    aynı "grid davranışını özelleştir" kaygısı):
 *                    - RowClickMode.ProcessOnDouble: satıra TEK tıklama artık
 *                      detay ekranını AÇMIYOR, yalnızca ÇİFT tıklama açıyor —
 *                      düşük teknik seviyeli kullanıcı profilinde kayarken/
 *                      seçerken yanlışlıkla ekran açılmasını engeller.
 *                    - FooterDisplayMode.Always: alt toplam satırı (bkz.
 *                      ListViewOzetlerController) her zaman görünür, özet
 *                      eklenmemiş olsa bile satır sayısını gösterir.
 *                    (Eski dosyadaki "CssClass += \" table-row\"" satırı
 *                    BİLEREK taşınmadı — bu projede o CSS sınıfı hiç
 *                    tanımlı değil, etkisiz ölü kod olurdu.)
 * ****************************************************************************
 */

using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;

namespace SofasisERP.Blazor.Server.Controllers;

public class ListViewSutunGenislikController : ViewController<ListView>
{
    protected override void OnViewControlsCreated()
    {
        base.OnViewControlsCreated();

        if (View.Editor is DxGridListEditor gridListEditor)
        {
            gridListEditor.RowClickMode = RowClickMode.ProcessOnDouble;
            gridListEditor.GridModel.ColumnResizeMode = GridColumnResizeMode.ColumnsContainer;
            gridListEditor.GridModel.FooterDisplayMode = GridFooterDisplayMode.Always;
        }
    }
}
