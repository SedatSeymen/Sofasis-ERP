using System;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

public interface INumberSequenceService
{
    // sequenceAnahtari: çağıran iş nesnesinin GetType().FullName'i (per-tip izolasyon korunur).
    // fisTuruTanim: prefix (FisTuruKodu) buradan okunur.
    // belgeTarihi: numaraya gömülen tarih segmenti (yyMMdd) BELGENİN kendi tarihidir (çağrı
    // anının değil). Dönen değer: "{FisTuruKodu}-{yyMMdd}{sıra:000}" — 16 karakter, günlük sıfırlanır.
    string SonrakiNumara(Session session, string sequenceAnahtari, FisTuruTanim fisTuruTanim, DateTime belgeTarihi);

    // Fiş türü/prefix/format bağımsız, sade artan tamsayı sıra numarası (ör. OperasyonSiraNo).
    // Aynı SequenceGenerator alt yapısını, ayrı bir sabit kriterle (sequenceAnahtari başına tek sayaç) kullanır.
    int SonrakiSiraNo(Session session, string sequenceAnahtari);
}
