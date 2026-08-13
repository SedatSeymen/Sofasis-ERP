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
}
