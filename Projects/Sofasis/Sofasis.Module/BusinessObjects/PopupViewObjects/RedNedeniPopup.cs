using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;

namespace Sofasis.Module.BusinessObjects;

// SatinAlmaTalebiOnayController'ın "Reddet" aksiyonu için kullanıcıdan red nedeni almak amacıyla
// açılan popup — DevExpress'in resmî "Ways to Show a Confirmation Dialog" (ConfirmationWindowParameters)
// deseninin birebir kopyası: plain POCO + [DomainComponent], NonPersistentObjectSpace ile gösterilir.
// NOT: [Size] XPO'ya özgü bir attribute'tur, bu POCO'nun arkasında bir DB kolonu olmadığından
// (XAF0025) kullanılmaz. Uzunluk sınırı burada YOKTUR — [Size(500)] zaten yalnızca DB kolon
// genişliği/istemci maxlength'idir, sunucu tarafında bir kısıt uygulamaz. Gerçek sunucu-taraflı
// 500-karakter sınırı SatinAlmaTalebiM.OnSaving()'de (RedNedeni atandıktan sonra commit anında)
// uygulanır — bkz. o sınıftaki RedNedeniMaxUzunluk kontrolü.
[DomainComponent]
public class RedNedeniPopup
{
    public RedNedeniPopup()
    {
    }

    [XafDisplayName("Red Nedeni")]
    [ModelDefault("RowCount", "3")]
    public string Neden { get; set; }
}
