/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : INumberSequenceService.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Boşluksuz sıra numarası üretim servisi (eski projeden
 *                    uyarlandı — bkz. docs/architecture/00_DetailView_ve_
 *                    Servis_Konvansiyonlari.md §2).
 * ****************************************************************************
 */

using DevExpress.Xpo;

namespace SofasisERP.Module.Services;

public interface INumberSequenceService
{
    // Fiş türü/prefix/format bağımsız, sade artan tamsayı sıra numarası
    // (ör. StokKoduJeneratoru). Aynı SequenceGenerator alt yapısını, ayrı bir
    // sabit kriterle (sequenceAnahtari başına tek sayaç) kullanır.
    //
    // NOT: Eski projedeki fiş-türü-önekli/tarih segmentli "SonrakiNumara(...)"
    // overload'ı burada BİLEREK yok — FisTuruTanim bu projede henüz mevcut
    // değil (Faz 2/Satınalma'da eklenecek). O aşamada bu arayüze eklenecek.
    int SonrakiSiraNo(Session session, string sequenceAnahtari);
}
