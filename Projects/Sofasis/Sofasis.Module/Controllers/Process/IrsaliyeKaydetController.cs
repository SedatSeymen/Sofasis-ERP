using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System.ComponentModel;

namespace Sofasis.Module.Controllers
{
    // SatinAlmaMalKabulKaydetController'ın yerini alır (bkz. docs/CHANGELOG.md 2026-08-14). Popup
    // "Kaydet"e basıldığında bu, PAYLAŞILAN ObjectSpace'i yalnızca dış (Sipariş) ObjectSpace'e
    // MERGE eder — GERÇEK DB commit'i yalnızca dış ekranın KENDİ "Kaydet"i ile olur (2 adımlı
    // commit deseni). Bu yüzden servis çağrısı burada DEĞİL, paylaşılan ObjectSpace'in Committing
    // event'inde yapılır.
    public class IrsaliyeKaydetController : ObjectViewController<DetailView, IrsaliyeM>
    {
        readonly ISatinAlmaIrsaliyeServisi irsaliyeServisi = new SatinAlmaIrsaliyeServisi();
        IrsaliyeM irsaliyeFisi;
        IObjectSpace abonelikObjectSpace;

        protected override void OnActivated()
        {
            base.OnActivated();
            // KRİTİK: yalnızca HENÜZ KAYDEDİLMEMİŞ (yeni) bir İrsaliye için abone ol. Bu koşul
            // olmadan, zaten kaydedilmiş bir İrsaliye'yi salt görüntülerken (ör. İrsaliyeler
            // listesinden açılan ekran) bu controller yine de aktifleşir; üzerinde açılan bağımsız
            // bir işlemin (ör. Fatura Oluştur popup'ı) tetiklediği GERÇEK commit'te bu ESKİ abonelik
            // de tetiklenip TeslimatiIsle'i İKİNCİ kez çalıştırır — KalanMiktar zaten 0 olduğundan
            // yanlış "kalan miktardan fazla" hatası fırlatır (canlı testte yakalandı, bkz.
            // docs/CHANGELOG.md 2026-08-14).
            if (ViewCurrentObject?.FisTuruTanim?.FisTuruKodu == "IRALIS" && ObjectSpace.IsNewObject(ViewCurrentObject))
            {
                irsaliyeFisi = ViewCurrentObject;
                abonelikObjectSpace = View.ObjectSpace;
                abonelikObjectSpace.Committing += ObjectSpace_Committing;
            }
        }

        protected override void OnDeactivated()
        {
            // BİLEREK burada abonelikten ÇIKILMAZ — gerçek commit popup/View kapandıktan SONRA
            // (dış Sipariş'in kendi Kaydet'i ile) gerçekleşir; OnDeactivated'da unsubscribe etmek
            // olayı hiç yakalayamamasına yol açardı (bkz. SatinAlmaMalKabulKaydetController'daki
            // aynı desenin canlı testte kanıtlanmış gerekçesi).
            base.OnDeactivated();
        }

        void ObjectSpace_Committing(object sender, CancelEventArgs e)
        {
            abonelikObjectSpace.Committing -= ObjectSpace_Committing;
            irsaliyeServisi.TeslimatiIsle(irsaliyeFisi);
        }
    }
}
