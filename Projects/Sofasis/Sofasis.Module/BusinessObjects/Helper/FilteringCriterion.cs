using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects;


[DefaultClassOptions, ImageName("Action_Filter")]
[XafDisplayName("Filre Oluşturucu")]
public class FilteringCriterion : BaseObject
{
    public FilteringCriterion(Session session) : base(session) { }

    [XafDisplayName("Açıklama")]
    public string Description
    {
        get { return GetPropertyValue<string>(nameof(Description)); }
        set { SetPropertyValue(nameof(Description), value); }
    }
    [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
    [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
    [XafDisplayName("Nesne Tipi")]
    public Type ObjectType
    {
        get { return GetPropertyValue<Type>(nameof(ObjectType)); }
        set
        {
            SetPropertyValue(nameof(ObjectType), value);
            Criterion = string.Empty;
        }
    }
    [CriteriaOptions("ObjectType"), Size(SizeAttribute.Unlimited)]
    [EditorAlias(EditorAliases.PopupCriteriaPropertyEditor)]
    [XafDisplayName("Filtre Kriteri")]
    public string Criterion
    {
        get { return GetPropertyValue<string>(nameof(Criterion)); }
        set { SetPropertyValue(nameof(Criterion), value); }
    }
}