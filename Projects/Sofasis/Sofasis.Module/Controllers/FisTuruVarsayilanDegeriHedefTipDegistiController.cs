/* ****************************************************************************
 * Proje     : Sofasis Erp Project
 * Dosya     : FisTuruVarsayilanDegeriHedefTipDegistiController.cs
 * Açıklama  : FisTuruVarsayilanDegeri.HedefTip her değiştiğinde (ImmediatePostData),
 *             VarsayilanDegerNesne alanının özel Blazor editörünü (dinamik
 *             ComboBox) yeniden okumaya zorlar — böylece "Seçili Değer" alanı
 *             seçilen yeni HedefTip'in kayıtlarıyla otomatik dolar.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers;

public class FisTuruVarsayilanDegeriHedefTipDegistiController : ObjectViewController<DetailView, FisTuruVarsayilanDegeri>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
    }

    protected override void OnDeactivated()
    {
        View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }

    void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    {
        if (ReferenceEquals(e.Object, ViewCurrentObject) && e.PropertyName == nameof(FisTuruVarsayilanDegeri.HedefTip))
            (View.FindItem(nameof(FisTuruVarsayilanDegeri.VarsayilanDegerNesne)) as PropertyEditor)?.Refresh();
    }
}
