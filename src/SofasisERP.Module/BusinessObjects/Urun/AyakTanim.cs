/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : AyakTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Koltuk ayak tipi tanımı (Ahşap/Metal, kasalı mı). Kullanıcı
 *                    kararı (2026-08-18): StokGrubu'na indirgenmiyor, kendi
 *                    varlığı olarak kalıyor; Boya Rengi ile master-detay
 *                    (Aggregated) — bkz. AyakBoyaRengiTanim.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(AyakTipi))]
[XafDisplayName("Ayak Tanımlama")]
public class AyakTanim : BaseClassWithAudit
{
    public AyakTanim(Session session) : base(session) { }

    AyakTipi ayakTipi;
    bool kasaliMi;

    // Bileşik benzersizlik: AyakTipi tek başına değil, AyakTipi+KasaliMi birlikte
    // benzersiz olmalı — "Ahşap Kasalı" ve "Ahşap Kasasız" ikisi de geçerli, farklı
    // kayıtlar (denetim raporu N5, ModelSetTanim.SetAdi'deki tekil-alan deseninden
    // farklı olarak burada iki alanlı bileşik anahtar doğru çözüm).
    [Indexed(nameof(KasaliMi), Unique = true)]
    [XafDisplayName("Ayak Tipi")]
    public AyakTipi AyakTipi
    {
        get => ayakTipi;
        set => SetPropertyValue(nameof(AyakTipi), ref ayakTipi, value);
    }

    [XafDisplayName("Kasalı mı?"), ToolTip("Oturum altında saklama kasası var mı?")]
    public bool KasaliMi
    {
        get => kasaliMi;
        set => SetPropertyValue(nameof(KasaliMi), ref kasaliMi, value);
    }

    [XafDisplayName("Boya Rengi Tanımlama")]
    [Association("AyakTanim-AyakBoyaRenkleri"), Aggregated]
    public XPCollection<AyakBoyaRengiTanim> AyakBoyaRenkleri
        => GetCollection<AyakBoyaRengiTanim>(nameof(AyakBoyaRenkleri));
}
