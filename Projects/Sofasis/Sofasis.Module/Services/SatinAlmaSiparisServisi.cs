using DevExpress.ExpressApp;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sofasis.Module.Services;

public sealed class SatinAlmaSiparisServisi : ISatinAlmaSiparisServisi
{
    public List<SatinAlmaSiparisiM> TekliflerdenSiparisOlustur(IEnumerable<SatinAlmaTeklifD> secilenTeklifSatirlari)
    {
        SatinAlmaTeklifD[] satirlar = secilenTeklifSatirlari?.ToArray() ?? Array.Empty<SatinAlmaTeklifD>();
        if (satirlar.Length == 0)
            throw new UserFriendlyException("Siparişe dönüştürmek için en az bir teklif kalemi seçmelisiniz.");

        if (satirlar.Any(s => s.KaynakTalepD == null))
            throw new UserFriendlyException("Kaynak Talep Kalemi boş olan bir teklif satırı siparişe dönüştürülemez.");

        var siparisler = new List<SatinAlmaSiparisiM>();

        // (Tedarikçi, Talep) ikilisine göre grupla — bir Sipariş yalnızca tek tedarikçiye gider,
        // farklı Talep'lerden seçilen satırlar da (aynı tedarikçiye ait olsa bile) ayrı sipariş
        // olur ki her Sipariş'in KaynakTalep'i tek ve net kalsın.
        var gruplar = satirlar.GroupBy(s => new
        {
            Tedarikci = s.SatinAlmaTeklifM.Tedarikci,
            Talep = s.KaynakTalepD.SatinAlmaTalebiM
        });

        foreach (var grup in gruplar)
        {
            Session session = grup.First().Session;

            SatinAlmaSiparisiM siparis = new SatinAlmaSiparisiM(session)
            {
                Tedarikci = grup.Key.Tedarikci,
                KaynakTalep = grup.Key.Talep
            };

            // Tüm satırlar aynı teklife aitse KaynakTeklif doldurulur; karışık (farklı teklif,
            // aynı tedarikçi) bir seçimde belirsiz olacağından boş bırakılır.
            SatinAlmaTeklifM[] farkliTeklifler = grup.Select(s => s.SatinAlmaTeklifM).Distinct().ToArray();
            if (farkliTeklifler.Length == 1)
                siparis.KaynakTeklif = farkliTeklifler[0];

            foreach (SatinAlmaTeklifD satir in grup)
            {
                _ = new SatinAlmaSiparisiD(session)
                {
                    SatinAlmaSiparisiM = siparis,
                    StokTanim = satir.StokTanim,
                    Miktar = satir.Miktar,
                    BirimFiyat = satir.TeklifBirimFiyat,
                    KaynakTalepD = satir.KaynakTalepD,
                    KaynakTeklifD = satir
                };
            }

            // Siparişe dönüşen teklif(ler) "Seçildi" olarak işaretlenir — aynı Talebin diğer
            // (kaybeden) teklifleri kasıtlı olarak dokunulmadan bırakılır; onları reddetmek ayrı,
            // manuel bir karar (henüz bu modülde bir "Reddet" aksiyonu yok).
            foreach (SatinAlmaTeklifM teklif in farkliTeklifler)
                teklif.Durum = SatinAlmaTeklifDurumu.Secildi;

            siparisler.Add(siparis);
        }

        return siparisler;
    }
}
