/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapParametre.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap modülü sistem parametreleri (singleton — tek kayıt).
 *                    StokParametre ile BİREBİR aynı desen: yeni kayıt açılırsa ve
 *                    zaten bir kayıt varsa, yeni kayıt iptal edilip mevcut kayıt
 *                    yüklenir (kullanıcı hep aynı tek satırı düzenler).
 * ****************************************************************************
 */

using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Cari Hesap Parametreleri")]
public class CariHesapParametre : BaseClass
{
    public CariHesapParametre(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            XPCollection<CariHesapParametre> mevcutlar = new XPCollection<CariHesapParametre>(Session);
            if (mevcutlar.Count >= 1)
            {
                this.CancelEdit();
                CariHesapParametre mevcut = mevcutlar[0];
                Session.DropChanges();
                Session.Reload(mevcut);
            }
            else
            {
                CariHesapKoduUretimYontemi = CariHesapKoduUretimYontemi.Jenerator;
            }
        }
    }

    // Tek-satır garantisi: değer her zaman 1'dir; benzersiz indeks DB seviyesinde
    // ikinci bir satır eklenmesini engeller — bkz. StokParametre.TekKayitAnahtari.
    int tekKayitAnahtari = 1;

    [Indexed(Unique = true)]
    [Browsable(false)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [VisibleInDashboards(false)]
    public int TekKayitAnahtari
    {
        get => tekKayitAnahtari;
        set => SetPropertyValue(nameof(TekKayitAnahtari), ref tekKayitAnahtari, value);
    }

    CariHesapKoduUretimYontemi cariHesapKoduUretimYontemi;

    [XafDisplayName("Cari Hesap Kodu Üretim Yöntemi")]
    public CariHesapKoduUretimYontemi CariHesapKoduUretimYontemi
    {
        get => cariHesapKoduUretimYontemi;
        set => SetPropertyValue(nameof(CariHesapKoduUretimYontemi), ref cariHesapKoduUretimYontemi, value);
    }
}
