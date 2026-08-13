using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Blazor.Server.Controllers;

// Kasa/Banka Hareket ekranlarındaki "Kasa / Banka Hesap" lookup alanının inline "Ekle" (+)
// düğmesi, DevExpress'in genel KasaBankaTanim_DetailView'ini üst ekranla hiçbir bağlantısı
// olmayan ayrı bir BlazorWindow'da açıyor (Blazor'da bu popup NestedFrame DEĞİL — canlı testte
// doğrulandı). Bu yüzden yeni nesneye Kasa mı Banka mı olduğu (CariKasaBankaTipi) ve buna bağlı
// FisTuruTanim aktarılamıyor; kayıt "Lütfen Fiş Türünü Giriniz" doğrulama hatasıyla engelleniyordu.
// Doğru hesap tipini otomatik türetmenin güvenli bir yolu olmadığından, bu düğme gizlenir — yeni
// Kasa/Banka hesabı "Kasa Hesap Tanımlama" / "Banka Hesap Tanımlama" ekranlarından oluşturulur.
public class KasaBankaTanimLookupYeniGizleController : ViewController<DetailView>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        if (View.CurrentObject is KasaBankaHareketleri || View.CurrentObject is CariHesapHareketleri)
        {
            View.CustomizeViewItemControl<LookupPropertyEditor>(this,
                editor => editor.HideNewButton(),
                nameof(KasaBankaHareketleri.KasaBankaTanim));
        }
    }
}
