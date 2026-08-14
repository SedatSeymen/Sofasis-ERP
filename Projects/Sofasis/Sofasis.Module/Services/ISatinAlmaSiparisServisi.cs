using Sofasis.Module.BusinessObjects;
using System.Collections.Generic;

namespace Sofasis.Module.Services;

public interface ISatinAlmaSiparisServisi
{
    // Seçilen teklif kalemlerini (Teklif Karşılaştırma ekranından) Sipariş(ler)e dönüştürür.
    // Farklı tedarikçilerden veya farklı Taleplere ait satırlar seçilmişse, her (Tedarikçi, Talep)
    // ikilisi için AYRI bir Sipariş oluşturulur — bir Sipariş yalnızca tek bir tedarikçiye
    // gönderilebilir.
    List<SatinAlmaSiparisiM> TekliflerdenSiparisOlustur(IEnumerable<SatinAlmaTeklifD> secilenTeklifSatirlari);

    // 4-ajan turunda (Endüstri Mühendisi + Mali Müşavir) bağımsız olarak bulunan kritik boşluk:
    // yanlış girilmiş, HENÜZ hiç teslimat almamış (Durum=Verildi) bir Sipariş'in tek düzeltme yolu
    // veritabanına elle müdahaleydi — ADR-018'in İrsaliye/Fatura için çözdüğü sorunla aynı sınıf.
    // v1 kapsamı: yalnız Verildi durumundaki Siparişler iptal edilebilir (kısmi/tam teslimat almış
    // bir Sipariş'in iptali stok/muhasebe etkisini geri almayı gerektirir — kapsam dışı, YAGNI).
    void IptalEt(SatinAlmaSiparisiM siparis);
}
