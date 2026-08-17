/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SequenceGenerator.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Sıralı numara üretimi için veritabanı kayıt sınıfı
 * ****************************************************************************
 */

using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[OptimisticLocking(true)]
public class SequenceGenerator : BaseClass
{
    public SequenceGenerator(Session session) : base(session) { }

    private string _businessObjectName;
    // Nesne adı ve kriter ikilisi benzersiz olmalı
    [Indexed("SequenceCriteria", Unique = true)]
    [Size(250)]
    public string BusinessObjectName
    {
        get => _businessObjectName;
        set => SetPropertyValue(nameof(BusinessObjectName), ref _businessObjectName, value);
    }

    private string _sequenceCriteria;
    // SQL index alabilmesi için sabit uzunluk tanımlandı (Unlimited yerine)
    [Size(250)]
    public string SequenceCriteria
    {
        get => _sequenceCriteria;
        set => SetPropertyValue(nameof(SequenceCriteria), ref _sequenceCriteria, value);
    }

    private long _nextSequence;
    // Sıradaki kullanılacak numara
    public long NextSequence
    {
        get => _nextSequence;
        set => SetPropertyValue(nameof(NextSequence), ref _nextSequence, value);
    }
}
