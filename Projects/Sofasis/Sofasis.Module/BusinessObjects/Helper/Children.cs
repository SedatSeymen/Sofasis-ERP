using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sofasis.Module.BusinessObjects;

public static class Children
{
    public static List<XPBaseCollection> GetChildrenEntity(this IXPObject obj)
    {
        List<XPBaseCollection> retvalue = new List<XPBaseCollection>();
        foreach (XPMemberInfo member in obj.ClassInfo.AssociationListProperties)
        {
            AssociationAttribute attribute = (AssociationAttribute)member.GetAttributeInfo(typeof(AssociationAttribute));
            XPBaseCollection children = (XPBaseCollection)member.GetValue(obj);
            retvalue.Add(children);
        }
        return retvalue;
    }
}
