/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FisTuruVarsayilanHedefTipiConverter.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : FisTuruVarsayilanDegeri.HedefTip alanının Tip seçim
 *                    kutusunu, TÜM iş nesnelerine değil, yalnızca
 *                    IFisTuruVarsayilanHedefi arayüzünü uygulayan sınıflara
 *                    daraltan TypeConverter (eski projeden aynen taşındı).
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Utils;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

public class FisTuruVarsayilanHedefTipiConverter : TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        Type[] hedefTipleri = XafTypesInfo.Instance.PersistentTypes
            .Where(t => !t.IsAbstract && t.Implements<IFisTuruVarsayilanHedefi>())
            .Select(t => t.Type)
            .OrderBy(t => CaptionHelper.GetClassCaption(t.FullName))
            .ToArray();
        return new StandardValuesCollection(hedefTipleri);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
            return value is Type type ? CaptionHelper.GetClassCaption(type.FullName) : string.Empty;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
