using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
using System;
using System.Globalization;
using System.Collections;
using System.Linq;
using DevExpress.XtraRichEdit.Commands;
using DevExpress.Persistent.Base;

namespace Sofasis.Module.BusinessObjects;

[NonPersistent]
[XafDisplayName("Üretim Planlama")]
public class UretimPlanlamaPopup : XPLiteObject
{
    DateTime planlanacakGun;
    protected int ThisWeek;
    protected int NextWeek;
    public UretimPlanlamaPopup(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        ThisWeek = CurrentWeek();
        NextWeek = ThisWeek + 1;
    }

    [XafDisplayName("Planlanacak Gün")]
    public DateTime PlanlanacakGun
    {
        get => planlanacakGun;
        set => SetPropertyValue(nameof(PlanlanacakGun), ref planlanacakGun, value);
    }

    int CurrentWeek()
    {
        CultureInfo ciCurr = CultureInfo.CurrentCulture;
        int weekNum = ciCurr.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return weekNum;
    }
    
    IList NextWeekDayList()
    {
        var now = DateTime.Now;
        var currentDay = now.DayOfWeek;
        int days = (int)currentDay;
        DateTime sunday = now.AddDays(-days);
        var daysThisWeek = Enumerable.Range(0, 7)
            .Select(d => sunday.AddDays(d))
            .ToList();
        return daysThisWeek;
    }

}