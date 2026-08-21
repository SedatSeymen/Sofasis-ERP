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

using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
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
            if (mevcutlar.Count >= 1)
            {
                this.CancelEdit();
                StokParametre mevcut = mevcutlar[0];
                Session.DropChanges();
                Session.Reload(mevcut);
            }
            else
            {
                StokKoduUretimYontemi = StokKoduUretimYontemi.Jenerator;
                NegatifStokPolitikasi = NegatifStokPolitikasi.Uyar;
            }
        }
    }

    // Tek-satır garantisi: değer her zaman 1'dir; benzersiz indeks DB seviyesinde
    // ikinci bir satır eklenmesini engeller — bkz. GenelParametre.TekKayitAnahtari
    // (aynı desen, denetim raporu O4).
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

    StokKoduUretimYontemi stokKoduUretimYontemi;
    NegatifStokPolitikasi negatifStokPolitikasi;

    [XafDisplayName("Stok Kodu Üretim Yöntemi")]
    public StokKoduUretimYontemi StokKoduUretimYontemi
    {
        get => stokKoduUretimYontemi;
        set => SetPropertyValue(nameof(StokKoduUretimYontemi), ref stokKoduUretimYontemi, value);
    }

    // Bir Çıkış hareketi bakiyeyi negatife düşürdüğünde motorun tepkisi — bkz.
    // StokHareketleriD.ObjectSaving (Faz 2, Ağırlıklı Ortalama Maliyet motoru).
    [XafDisplayName("Negatif Stok Politikası")]
    public NegatifStokPolitikasi NegatifStokPolitikasi
    {
        get => negatifStokPolitikasi;
        set => SetPropertyValue(nameof(NegatifStokPolitikasi), ref negatifStokPolitikasi, value);
    }
}
