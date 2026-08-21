/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : OndalikFormatController.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : OndalikAlanKatalogu'ndaki (Miktar/Tutar/Yerel/Maliyet/Kur)
 *                    kategorileri DetailView/ListView'a uygular — eski projeden
 *                    aynen uyarlandı (bkz. ilerideki DetailView özelleştirmelerinde
 *                    aynı desen kullanılmalı: Module.cs SetupComplete-zamanlı Model
 *                    mutasyonu DetailView PropertyEditor/ListView Column
 *                    seviyesindeki formatı garanti ETMEZ — bu controller'lar
 *                    OnActivated'da doğrudan yazarak son sözü söyler).
 * ****************************************************************************
 */

using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Controllers.Process;

public class OndalikFormatController : ViewController<DetailView>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        FormatlariUygula();
    }

    void FormatlariUygula()
    {
        object nesne = View.CurrentObject;
        if (nesne == null) return;

        Type tip = nesne.GetType();
        string kisaSinifAdi = tip.Name;

        (int miktarHane, int tutarHane, int kurHane) = GenelParametreOkuyucu.OndalikHaneleriniOku(View.ObjectSpace);

        DovizTanim doviz = null;
        bool dovizAranmisti = false;

        foreach (PropertyInfo prop in tip.GetProperties())
        {
            if (prop.PropertyType != typeof(decimal) && prop.PropertyType != typeof(decimal?))
                continue;
            if (!OndalikAlanKatalogu.Kategoriler.TryGetValue((kisaSinifAdi, prop.Name), out string kategori))
                continue;
            if (View.FindItem(prop.Name) is not PropertyEditor editor)
                continue;

            string sembol;
            int hane;
            switch (kategori)
            {
                case "Miktar":
                    sembol = string.Empty;
                    hane = miktarHane;
                    break;
                case "Tutar":
                    if (!dovizAranmisti)
                    {
                        doviz = OndalikAlanKatalogu.DovizTaniminiCoz(nesne, tip);
                        dovizAranmisti = true;
                    }
                    sembol = doviz?.Sembol ?? "₺";
                    hane = tutarHane;
                    break;
                case "Yerel":
                    sembol = "₺";
                    hane = tutarHane;
                    break;
                case "Maliyet":
                    sembol = "₺";
                    hane = miktarHane;
                    break;
                case "Kur":
                    // Sembolsüz — bir para tutarı değil, oran (Miktar ile aynı muamele).
                    sembol = string.Empty;
                    hane = kurHane;
                    break;
                default:
                    continue;
            }

            string mask = "N" + hane;
            editor.EditMask = mask;
            editor.DisplayFormat = sembol + "{0:" + mask + "}";
        }
    }
}

// ListView sütunları için eşdeğeri. Model.Views koleksiyonu SetupComplete anında
// henüz ziyaret edilmemiş (tembel/lazy oluşturulan) ListView düğümlerini
// içermeyebilir — bu controller, ListView AÇILDIĞINDA (OnActivated'da) doğrudan
// gerçek Column düğümlerine yazar, DetailView'daki ile birebir aynı mantık,
// yalnızca hedef PropertyEditor değil IModelColumn.
//
// Tutar kategorisinde satır-bazlı DİNAMİK sembol (her satırın kendi DovizTanim'ine
// göre farklı sembol) bir grid sütununda TEK bir statik DisplayFormat string'i ile
// mümkün değil — bilinçli olarak ileride "özel hücre şablonu" işi olarak ertelenir;
// bu yüzden Tutar sütunlarına da (Maliyet/Yerel gibi) sabit ₺ uygulanır, yalnızca
// hane sayısı garanti edilir.
public class OndalikFormatListController : ViewController<ListView>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        FormatlariUygula();
    }

    void FormatlariUygula()
    {
        if (View.Model is not IModelListView modelListView) return;
        Type tip = View.ObjectTypeInfo.Type;
        string kisaSinifAdi = tip.Name;

        (int miktarHane, int tutarHane, int kurHane) = GenelParametreOkuyucu.OndalikHaneleriniOku(View.ObjectSpace);

        foreach (PropertyInfo prop in tip.GetProperties())
        {
            if (prop.PropertyType != typeof(decimal) && prop.PropertyType != typeof(decimal?))
                continue;
            if (!OndalikAlanKatalogu.Kategoriler.TryGetValue((kisaSinifAdi, prop.Name), out string kategori))
                continue;
            IModelColumn column = modelListView.Columns[prop.Name];
            if (column == null) continue;

            string sembol;
            int hane;
            switch (kategori)
            {
                case "Miktar":
                    sembol = string.Empty;
                    hane = miktarHane;
                    break;
                case "Tutar":
                case "Yerel":
                    sembol = "₺";
                    hane = tutarHane;
                    break;
                case "Maliyet":
                    sembol = "₺";
                    hane = miktarHane;
                    break;
                case "Kur":
                    sembol = string.Empty;
                    hane = kurHane;
                    break;
                default:
                    continue;
            }

            string mask = "N" + hane;
            column.EditMask = mask;
            column.DisplayFormat = sembol + "{0:" + mask + "}";
        }
    }
}
