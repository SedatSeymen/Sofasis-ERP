/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokParametre.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok modülü sistem parametreleri (singleton — tek kayıt).
 *                    Desen eski projeden uyarlandı: yeni kayıt açılırsa ve zaten
 *                    bir kayıt varsa, yeni kayıt iptal edilip mevcut kayıt
 *                    yüklenir (kullanıcı hep aynı tek satırı düzenler).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[XafDisplayName("Stok Parametreleri")]
public class StokParametre : BaseClass
{
    public StokParametre(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            XPCollection<StokParametre> mevcutlar = new XPCollection<StokParametre>(Session);
            if (mevcutlar.Count == 1)
            {
                this.CancelEdit();
                StokParametre mevcut = mevcutlar[0];
                Session.DropChanges();
                Session.Reload(mevcut);
            }
            else
            {
                StokKoduUretimYontemi = StokKoduUretimYontemi.Jenerator;
            }
        }
    }

    StokKoduUretimYontemi stokKoduUretimYontemi;

    [XafDisplayName("Stok Kodu Üretim Yöntemi")]
    public StokKoduUretimYontemi StokKoduUretimYontemi
    {
        get => stokKoduUretimYontemi;
        set => SetPropertyValue(nameof(StokKoduUretimYontemi), ref stokKoduUretimYontemi, value);
    }
}
