using System.Collections.Generic;

namespace Sofasis.Module.Services;

public interface IWeightedAverageCostService
{
    // Bir Giriş hareketinin (satın alma, üretim, sayım fazlası, transfer girişi...) mevcut
    // miktar/ortalama üzerindeki etkisi. Session/XPO'dan tamamen bağımsız, saf fonksiyon.
    (decimal YeniMiktar, decimal YeniOrtalamaMaliyet) GirisUygula(
        decimal mevcutMiktar, decimal mevcutOrtalamaMaliyet, decimal girisMiktari, decimal girisBirimMaliyet);

    // Bir Çıkış hareketinin etkisi — ortalama maliyeti HİÇ değiştirmez (ADR-005: çıkışlar o anki
    // ortalama ile değerlenir), yalnızca miktarı düşürür.
    (decimal YeniMiktar, decimal YeniOrtalamaMaliyet) CikisUygula(
        decimal mevcutMiktar, decimal mevcutOrtalamaMaliyet, decimal cikisMiktari);

    // Bir StokHareketleriD satırı silindiğinde (veya geçmiş yeniden kurulmak istendiğinde)
    // kullanılır: sıfırdan başlayıp verilen sırayla (kayıt/giriş sırası — FisTarihi DEĞİL,
    // bkz. StokHareketleriD.ObjectDeleting) GirisUygula/CikisUygula'yı katlayarak uygular.
    (decimal Miktar, decimal OrtalamaMaliyet) YenidenHesapla(
        IEnumerable<(bool Giris, decimal Miktar, decimal BirimMaliyet)> hareketlerKayitSirasiyla);
}
