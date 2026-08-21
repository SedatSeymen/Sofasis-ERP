/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaBankaHesabi.cs
 * Oluşturma Tarihi : 08/19/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/19/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Faz 3 devamı (Çek/Senet) — Hesap ile KasaTanim/BankaHesabiTanim
 *                    arasına eklenen ara katman. Ek alan TAŞIMAZ; tek amacı, "hedef
 *                    nakit hesabı" seçimi gereken aksiyonlarda (Çek/Senet Tahsil Et/
 *                    Öde/Teminata Ver) parametre tipini bu sınıfa vererek XPO'nun
 *                    taban-tipli lookup polimorfizmasıyla SADECE Kasa+Banka'yı
 *                    listelemeyi sağlamak — Cari otomatik dışlanır, ekstra bir
 *                    DataSourceCriteria yazmaya gerek kalmaz (MuhasebeFisSatiri.Hesap
 *                    alanındaki taban-tipli lookup davranışının aynısı).
 * ****************************************************************************
 */

using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

public abstract class KasaBankaHesabi : Hesap
{
    protected KasaBankaHesabi(Session session) : base(session) { }
}
