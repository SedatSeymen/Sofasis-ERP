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
using SofasisERP.Module.BusinessObjects;
using System;

namespace SofasisERP.Module.Services;

public interface INumberSequenceService
{
    // Fiş türü/prefix/format bağımsız, sade artan tamsayı sıra numarası.
    // Aynı SequenceGenerator alt yapısını, ayrı bir sabit kriterle
    // (sequenceAnahtari başına tek sayaç) kullanır.
    int SonrakiSiraNo(Session session, string sequenceAnahtari);

    // Eski projeden aynen taşınan fiş-türü-önekli/GÜNLÜK sıfırlanan numara üretimi
    // (bkz. StokKoduJeneratoru — StokTanim/Hizmet/Masraf "Otomatik" kod üretimi).
    // Format: "{FisTuruKodu}-{yyMMdd}{sıra:D3}". Sayaç kriteri
    // "{sequenceAnahtari}-{FisTuruKodu}-{yyMMdd}" olduğundan her (çağıran tip +
    // fiş türü + gün) kombinasyonu kendi sayacına sahiptir, sıra GÜNLÜK sıfırlanır.
    string SonrakiNumara(Session session, string sequenceAnahtari, FisTuruTanim fisTuruTanim, DateTime belgeTarihi);
}
