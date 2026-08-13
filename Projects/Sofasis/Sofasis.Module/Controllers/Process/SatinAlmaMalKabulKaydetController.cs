using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System.ComponentModel;

namespace Sofasis.Module.Controllers
{
    // Yalnızca fiş türü STSAGR (Satın Alma Mal Kabul Girişi) olan StokHareketleriM ekranlarında
    // aktif olur — Stok modülü (StokHareketleriM.cs) SatinAlma'yı hiç bilmez (katman ayrımı,
    // ADR-006), bu SatinAlma'ya özel izleme AYRI bir controller'da yaşar.
    // Committing (Committed DEĞİL) kullanılır: bu event ObjectSpace.CommitChanges() TAMAMLANMADAN
    // ÖNCE tetiklenir — TeslimatiIsle() içinde yapılan SatinAlmaSiparisiD.TeslimEdilenMiktar/
    // SatinAlmaSiparisiM.Durum değişiklikleri AYNI transaction'a dahil olur (ek bir commit
    // gerekmez, "commit sonrası commit" riski taşımaz). StokHareketleriM zaten kayıt sonrası
    // immutable (StokHareketleriMKayitliFisKilitleController) olduğundan bu fiş bir daha
    // düzenlenip tekrar kaydedilemez — TeslimatiIsle'nin en fazla BİR KEZ çalışacağı garantidir.
    public class SatinAlmaMalKabulKaydetController : ObjectViewController<DetailView, StokHareketleriM>
    {
        readonly ISatinAlmaMalKabulServisi malKabulServisi = new SatinAlmaMalKabulServisi();
        StokHareketleriM malKabulFisi;
        DevExpress.ExpressApp.IObjectSpace abonelikObjectSpace;

        protected override void OnActivated()
        {
            base.OnActivated();
            if (ViewCurrentObject?.FisTuruTanim?.FisTuruKodu == "STSAGR")
            {
                // Nesne ve ObjectSpace referansları BURADA (View henüz kesinlikle canlıyken)
                // yakalanır — asıl commit popup KAPANDIKTAN SONRA (dış Sipariş'in kendi Kaydet'iyle)
                // gerçekleştiğinden, handler'ın o an ViewCurrentObject/View'e (disposed olabilir)
                // DEĞİL bu yakalanmış referanslara erişmesi gerekir (canlı testte doğrulandı:
                // ViewCurrentObject'e handler içinde erişmek "Object reference not set to an
                // instance of an object" ile çöküyordu).
                malKabulFisi = ViewCurrentObject;
                abonelikObjectSpace = View.ObjectSpace;
                abonelikObjectSpace.Committing += ObjectSpace_Committing;
            }
        }

        protected override void OnDeactivated()
        {
            // BİLEREK burada abonelikten ÇIKILMAZ: bu popup (SatinAlmaMalKabulOlusturController'ın
            // "isRoot: false" ile paylaştığı ObjectSpace) kendi "Kaydet"iyle KAPANIR ama gerçek
            // CommitChanges() — ve dolayısıyla asıl Committing olayı — ancak kullanıcı DIŞARIDAKİ
            // Sipariş ekranının KENDİ "Kaydet"ine bastığında tetiklenir (canlı testte kanıtlandı:
            // popup Kaydet'i yalnızca StokHareketleriM.OnSaving'i tetikliyor/FisNo üretiyor, hiçbir
            // satır DB'ye yazılmıyor). OnDeactivated'da abonelikten çıkmak, tam da bu ASIL commit
            // gerçekleşmeden ÖNCE dinleyiciyi kaldırıp TeslimatiIsle'nin HİÇ ÇALIŞMAMASINA yol
            // açıyordu. Tekrar tetiklenme riski ObjectSpace_Committing içindeki self-unsubscribe ile
            // önlenir (bkz. aşağı).
            base.OnDeactivated();
        }

        void ObjectSpace_Committing(object sender, CancelEventArgs e)
        {
            // Bu ObjectSpace aynı Sipariş görünümü açıkken BAŞKA sebeplerle tekrar kaydedilebilir
            // (ör. kullanıcı başka bir alanı değiştirip yeniden Kaydet'e basarsa) — TeslimatiIsle'nin
            // AYNI satırlar için İKİNCİ KEZ çalışıp TeslimEdilenMiktar'ı mükerrer artırmaması için
            // ilk (ve tek geçerli) commit'te kendi kendine abonelikten çıkar.
            abonelikObjectSpace.Committing -= ObjectSpace_Committing;
            malKabulServisi.TeslimatiIsle(malKabulFisi);
        }
    }
}
