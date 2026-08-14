using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System;
using System.ComponentModel;

namespace Sofasis.Module.Controllers
{
    // SatinAlmaMalKabulKaydetController'ın yerini alır (bkz. docs/CHANGELOG.md 2026-08-14). Popup
    // "Kaydet"e basıldığında bu, PAYLAŞILAN ObjectSpace'i yalnızca dış (Sipariş) ObjectSpace'e
    // MERGE eder — GERÇEK DB commit'i yalnızca dış ekranın KENDİ "Kaydet"i ile olur (2 adımlı
    // commit deseni). Bu yüzden servis çağrısı burada DEĞİL, paylaşılan ObjectSpace'in Committing
    // event'inde yapılır. IRALIS (normal İrsaliye → TeslimatiIsle) ve IRALID (İade İrsaliyesi →
    // IadeyiIsle) İKİSİNİ de kapsar — StokHareketleriM'in TEK sınıfla Giriş/Çıkış yönetmesiyle aynı
    // desen (ADR-017): ayrı bir "IrsaliyeIadeKaydetController" AÇILMADI.
    public class IrsaliyeKaydetController : ObjectViewController<DetailView, IrsaliyeM>
    {
        readonly ISatinAlmaIrsaliyeServisi irsaliyeServisi = new SatinAlmaIrsaliyeServisi();
        IrsaliyeM irsaliyeFisi;
        bool iade;
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
            string fisTuruKodu = ViewCurrentObject?.FisTuruTanim?.FisTuruKodu;
            if ((fisTuruKodu == "IRALIS" || fisTuruKodu == "IRALID") && ObjectSpace.IsNewObject(ViewCurrentObject))
            {
                irsaliyeFisi = ViewCurrentObject;
                iade = fisTuruKodu == "IRALID";
                abonelikObjectSpace = View.ObjectSpace;
                abonelikObjectSpace.Committing += ObjectSpace_Committing;
                abonelikObjectSpace.Committed += ObjectSpace_Committed;
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
            // ⚠ Abonelikten BURADA ÇIKILMAZ (4-ajan turunda bulundu): IadeyiIsle/TeslimatiIsle
            // gerçek bir throw fırlatabilir (ör. "kalan miktardan fazla" doğrulaması). Unsubscribe
            // servis çağrısından ÖNCE yapılırsa, commit başarısız olup kullanıcı tekrar Kaydet'e
            // bastığında artık kimse dinlemiyor — belge, iş mantığı HİÇ ÇALIŞMADAN DB'ye yazılabilir
            // (Sipariş TeslimEdilenMiktar/Durum güncellenmeden). Unsubscribe yalnızca Committed'da
            // (gerçekten başarılı commit sonrası) yapılır — böylece başarısız denemeler güvenle
            // yeniden denenebilir, ama başarılı bir commit sonrası aynı View'de tetiklenecek
            // ALAKASIZ bir sonraki commit'te (ADR-017'nin orijinal bug'ı) ikinci kez çalışmaz.
            if (iade)
                irsaliyeServisi.IadeyiIsle(irsaliyeFisi);
            else
                irsaliyeServisi.TeslimatiIsle(irsaliyeFisi);
        }

        void ObjectSpace_Committed(object sender, EventArgs e)
        {
            abonelikObjectSpace.Committing -= ObjectSpace_Committing;
            abonelikObjectSpace.Committed -= ObjectSpace_Committed;
        }
    }
}
