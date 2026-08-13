using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using System.Linq;

namespace Sofasis.Module.Services;

public sealed class SatinAlmaTeklifServisi : ISatinAlmaTeklifServisi
{
    public void Karsilastir(SatinAlmaTalebiM talep)
    {
        SatinAlmaTeklifD[] tumSatirlar = talep.SatinAlmaTeklifleri
            .SelectMany(teklif => teklif.SatinAlmaTeklifDs)
            .Where(satir => satir.KaynakTalepD != null)
            .ToArray();

        if (tumSatirlar.Length == 0)
            throw new UserFriendlyException("Bu talebe ait karşılaştırılacak teklif bulunamadı.");

        // Önce tüm satırlar sıfırlanır — daha önce en düşük işaretli bir satır güncellenmiş/silinmiş
        // olabilir, bayrak bilgisi yeniden hesaplanmadan eskisi kalmasın.
        foreach (SatinAlmaTeklifD satir in tumSatirlar)
            satir.EnDusukFiyatMi = false;

        foreach (var grup in tumSatirlar.GroupBy(satir => satir.KaynakTalepD))
        {
            decimal enDusukFiyat = grup.Min(satir => satir.TeklifBirimFiyat);
            foreach (SatinAlmaTeklifD satir in grup.Where(s => s.TeklifBirimFiyat == enDusukFiyat))
                satir.EnDusukFiyatMi = true;
        }
    }

    public SatinAlmaTeklifM TalepdenTeklifTaslagiOlustur(SatinAlmaTalebiM talep)
    {
        if (talep == null)
            throw new UserFriendlyException("Teklif oluşturmak için bir Talep gereklidir.");

        // KaynakTalep setter'ı (SatinAlmaTeklifM.cs) talebin tüm kalemlerini otomatik teklif
        // kalemi olarak ön-dolduruyor — burada tekrar edilmez.
        return new SatinAlmaTeklifM(talep.Session) { KaynakTalep = talep };
    }
}
