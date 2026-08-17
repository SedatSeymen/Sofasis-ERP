/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : NumberSequenceService.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : INumberSequenceService implementasyonu (eski projeden
 *                    uyarlandı).
 * ****************************************************************************
 */

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Services;

// DevExpress'in "Auto-Generate Unique Number Sequence" rehberinin (ORM-Level: Programmatic
// Transaction) daha sade bir uyarlaması: process-genelinde paylaşılan bir C# lock YOK, ayrı bir
// UnitOfWork/erken commit YOK. SequenceGenerator satırı çağıranla AYNI Session üzerinde
// değiştirilir ve bilerek COMMIT EDİLMEZ — dışarıdaki (çağıran iş nesnesinin OnSaving'ini
// tetikleyen) UnitOfWork.CommitChanges() ile aynı transaction'a dahil olur. Böylece: belge kaydı
// başarısız olup geri alınırsa üretilen numara da geri alınır (gerçek "boşluksuz" numaralandırma).
//
// Eşzamanlı iki kullanıcı aynı sequenceAnahtari'na aynı anda yazarsa: SequenceGenerator
// [OptimisticLocking(true)] taşıdığından dıştaki commit sırasında XPO doğal olarak
// OptimisticLockingException fırlatır — bilinçli kabul edilen sınır, otomatik sessiz retry yok.
public sealed class NumberSequenceService : INumberSequenceService
{
    // Aynı Session'da TEK CommitChanges() içinde birden fazla yeni master kaydedilirken,
    // Session.GetObjectsToSave() bir önceki nesnenin OnSaving()'i SIRASINDA yeni oluşturulan
    // SequenceGenerator'ı güvenilir şekilde yansıtmıyor (eski projede canlı testte doğrulanmış bir
    // sorun). Bu yüzden üretilen generator'lar Session ömrü boyunca (ConditionalWeakTable ile —
    // Session çöpe gidince otomatik temizlenir) önbelleğe alınır.
    static readonly ConditionalWeakTable<Session, ConcurrentDictionary<string, SequenceGenerator>> sessionCache = new();

    public int SonrakiSiraNo(Session session, string sequenceAnahtari)
    {
        const string kriter = "SiraNo";
        SequenceGenerator generator = BulYadaOlustur(session, sequenceAnahtari, kriter);
        generator.NextSequence++;
        return (int)generator.NextSequence;
    }

    public string SonrakiNumara(Session session, string sequenceAnahtari, FisTuruTanim fisTuruTanim, DateTime belgeTarihi)
    {
        if (fisTuruTanim == null)
            throw new ArgumentNullException(nameof(fisTuruTanim));

        string tarihSegmenti = belgeTarihi.ToString("yyMMdd");
        string kriter = $"{fisTuruTanim.FisTuruKodu}-{tarihSegmenti}";
        SequenceGenerator generator = BulYadaOlustur(session, sequenceAnahtari, kriter);
        generator.NextSequence++;
        return $"{fisTuruTanim.FisTuruKodu}-{tarihSegmenti}{generator.NextSequence:D3}";
    }

    static SequenceGenerator BulYadaOlustur(Session session, string sequenceAnahtari, string kriter)
    {
        ConcurrentDictionary<string, SequenceGenerator> cache = sessionCache.GetOrCreateValue(session);
        string onbellekAnahtari = sequenceAnahtari + "" + kriter;

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
