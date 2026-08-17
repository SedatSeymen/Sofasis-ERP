/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : GenelParametre.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Sistem geneli parametreler (singleton). Şimdilik yalnızca
 *                    miktar/tutar/kur ondalık basamak ayarlarını içerir — eski
 *                    projeden uyarlandı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[XafDisplayName("Genel Parametre Tanımlama")]
public class GenelParametre : BaseClass
{
    public GenelParametre(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            XPCollection<GenelParametre> mevcutlar = new XPCollection<GenelParametre>(Session);
            if (mevcutlar.Count == 1)
            {
                this.CancelEdit();
                GenelParametre mevcut = mevcutlar[0];
                Session.DropChanges();
                Session.Reload(mevcut);
            }
        }
    }

    OndalikBasamakSayisi miktarOndalikMaski = OndalikBasamakSayisi.Basamak4;
    OndalikBasamakSayisi tutarOndalikMaski = OndalikBasamakSayisi.Basamak2;
    OndalikBasamakSayisi kurOndalikMaski = OndalikBasamakSayisi.Basamak6;

    [XafDisplayName("Miktar Ondalık Maskı")]
    public OndalikBasamakSayisi MiktarOndalikMaski
    {
        get => miktarOndalikMaski;
        set => SetPropertyValue(nameof(MiktarOndalikMaski), ref miktarOndalikMaski, value);
    }

    [XafDisplayName("Tutar Ondalık Maskı")]
    public OndalikBasamakSayisi TutarOndalikMaski
    {
        get => tutarOndalikMaski;
        set => SetPropertyValue(nameof(TutarOndalikMaski), ref tutarOndalikMaski, value);
    }

    [XafDisplayName("Kur Ondalık Maskı")]
    public OndalikBasamakSayisi KurOndalikMaski
    {
        get => kurOndalikMaski;
        set => SetPropertyValue(nameof(KurOndalikMaski), ref kurOndalikMaski, value);
    }
}
