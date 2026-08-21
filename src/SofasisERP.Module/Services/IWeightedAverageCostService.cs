/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : IWeightedAverageCostService.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Hareketli Ağırlıklı Ortalama Maliyet (moving weighted average
 *                    cost) hesabı — StokHareketleriD.ObjectSaving/ObjectDeleting
 *                    tarafından çağrılır. Matematik BO içine gömülmez, ayrı ve saf
 *                    (side-effect'siz) bir servistir — eski projeden birebir
 *                    taşındı (Faz 2).
 * ****************************************************************************
 */

namespace SofasisERP.Module.Services;

public interface IWeightedAverageCostService
{
    // Giriş hareketi: eskiMiktar==0 ise ortalama doğrudan giriş birim maliyetine eşitlenir,
    // aksi halde (eskiMiktar*eskiOrtalama + girisMiktari*girisBirimMaliyeti) / yeniMiktar.
    (decimal YeniMiktar, decimal YeniOrtalama) GirisUygula(
        decimal eskiMiktar, decimal eskiOrtalama, decimal girisMiktari, decimal girisBirimMaliyeti);

    // Çıkış hareketi: miktar azalır, ortalama hareketli ortalama yönteminin kuralı gereği DEĞİŞMEZ.
    (decimal YeniMiktar, decimal AyniOrtalama) CikisUygula(
        decimal eskiMiktar, decimal eskiOrtalama, decimal cikisMiktari);

    // Bir hareketin silinmesi sonrası tam geçmiş replay'i: sıfırdan başlayıp kalan tüm
    // hareketleri (CreatedDate sırasıyla, çağıran taraf sıralar) GirisUygula/CikisUygula
    // ile yeniden uygular. Yalnızca "son kaydı geri al" YETERSİZDİR (hareketli ortalama
    // sıraya duyarlıdır).
    (decimal ToplamMiktar, decimal Ortalama) YenidenHesapla(
        IEnumerable<(bool Giris, decimal Miktar, decimal BirimMaliyet)> hareketler);
}
