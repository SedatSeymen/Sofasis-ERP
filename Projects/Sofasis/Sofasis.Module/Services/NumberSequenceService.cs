using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Services;

// DevExpress'in "Auto-Generate Unique Number Sequence" rehberinin (ORM-Level: Programmatic
// Transaction) daha sade bir uyarlaması: process-genelinde paylaşılan bir C# lock YOK, ayrı bir
// UnitOfWork/erken commit YOK. SequenceGenerator satırı çağıranla AYNI Session üzerinde
// değiştirilir ve bilerek COMMIT EDİLMEZ — dışarıdaki (çağıran iş nesnesinin OnSaving'ini
// tetikleyen) UnitOfWork.CommitChanges() ile aynı transaction'a dahil olur. Böylece: belge kaydı
// başarısız olup geri alınırsa üretilen numara da geri alınır (gerçek "boşluksuz" numaralandırma),
// ve DevExpress dokümantasyonunun uyardığı "OnSaving birden fazla kez çalışabilir" durumuna karşı
// çağıran taraf zaten "sadece hâlâ taslak/placeholder ise numara üret" koruması kullanıyor.
//
// Eşzamanlı iki kullanıcı aynı (sequenceAnahtari+FisTuruKodu[+yıl]) kombinasyonuna aynı anda
// yazarsa: SequenceGenerator [OptimisticLocking(true)] taşıdığından dıştaki commit sırasında XPO
// doğal olarak OptimisticLockingException fırlatır — bu, XAF'ın her yerde kullanıcıya gösterdiği
// standart "kayıt değişti, tekrar deneyin" akışına düşer (bilinçli kabul edilen sınır, otomatik
// sessiz retry yok).
public sealed class NumberSequenceService : INumberSequenceService
{
    // Aynı Session'da TEK CommitChanges() içinde birden fazla yeni master kaydedilirken (ör.
    // ISatinAlmaSiparisServisi'nin bir seferde birden fazla SatinAlmaSiparisiM oluşturması),
    // Session.GetObjectsToSave() bir önceki nesnenin OnSaving()'i SIRASINDA yeni oluşturulan
    // SequenceGenerator'ı güvenilir şekilde yansıtmıyor (canlı testte doğrulandı: iki nesne aynı
    // numarayı aldı, RuleUniqueValue commit'te patladı). Bu yüzden üretilen generator'lar Session
    // ömrü boyunca (ConditionalWeakTable ile — Session çöpe gidince otomatik temizlenir) önbelleğe
    // alınır; süreç genelinde paylaşılan bir kilit veya erken/ayrı commit YOK, dosya başındaki
    // tasarım hâlâ geçerli.
    static readonly ConditionalWeakTable<Session, ConcurrentDictionary<string, SequenceGenerator>> sessionCache = new();

    public string SonrakiNumara(Session session, string sequenceAnahtari, FisTuruTanim fisTuruTanim, DateTime belgeTarihi)
    {
        // Format: {FisTuruKodu}-{yyMMdd}{sıra:000} — toplam 16 karakter (6+1+6+3). Tarih numaranın
        // içine gömülü olduğundan sıra numarası GÜNLÜK sıfırlanır; yıl sonunda ayrıca sıfırlamaya
        // gerek yoktur (aynı gün+fiş türü+iş nesnesi kombinasyonu dışında çakışma oluşmaz).
        string tarihSegmenti = belgeTarihi.ToString("yyMMdd");
        string kriter = $"{sequenceAnahtari}-{fisTuruTanim.FisTuruKodu}-{tarihSegmenti}";

        SequenceGenerator generator = BulYadaOlustur(session, sequenceAnahtari, kriter);
        generator.NextSequence++;

        return $"{fisTuruTanim.FisTuruKodu}-{tarihSegmenti}{generator.NextSequence:D3}";
    }

    public int SonrakiSiraNo(Session session, string sequenceAnahtari)
    {
        const string kriter = "SiraNo";
        SequenceGenerator generator = BulYadaOlustur(session, sequenceAnahtari, kriter);
        generator.NextSequence++;
        return (int)generator.NextSequence;
    }

    static SequenceGenerator BulYadaOlustur(Session session, string sequenceAnahtari, string kriter)
    {
        ConcurrentDictionary<string, SequenceGenerator> cache = sessionCache.GetOrCreateValue(session);
        string onbellekAnahtari = sequenceAnahtari + "" + kriter;

        if (cache.TryGetValue(onbellekAnahtari, out SequenceGenerator onbellektekiGenerator))
            return onbellektekiGenerator;

        SequenceGenerator generator = session.GetObjectsToSave()
            .OfType<SequenceGenerator>()
            .FirstOrDefault(x => x.BusinessObjectName == sequenceAnahtari && x.SequenceCriteria == kriter)
            ?? session.FindObject<SequenceGenerator>(
                CriteriaOperator.FromLambda<SequenceGenerator>(
                    x => x.BusinessObjectName == sequenceAnahtari && x.SequenceCriteria == kriter));

        if (generator == null)
        {
            generator = new SequenceGenerator(session)
            {
                BusinessObjectName = sequenceAnahtari,
                SequenceCriteria = kriter,
                NextSequence = 0
            };
        }

        cache[onbellekAnahtari] = generator;
        return generator;
    }
}
