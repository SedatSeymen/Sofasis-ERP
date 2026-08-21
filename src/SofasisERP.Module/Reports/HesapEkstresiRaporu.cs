/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : HesapEkstresiRaporu.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari/Kasa/Banka Hesap Ekstresi — TEK rapor sınıfı, üçü için de
 *                    kullanılıyor. Eski ERP'de Cari ve Kasa/Banka için AYRI iki rapor
 *                    sınıfı gerekiyordu (ayrı business object tipleri); bu projede
 *                    Kasa/Cari/Banka zaten ortak bir taban tipten (Hesap) türediği ve
 *                    tek bir hareket tablosunu (KasaCariBankaHareketleri) paylaştığı
 *                    için tek rapor yeterli — hangi Hesap için çalıştığı Controller
 *                    tarafından kriter (CriteriaOperator) + gizli Parameter'larla
 *                    (bkz. HesapEkstresiRaporuControllerBase) belirlenir.
 * ****************************************************************************
 */

using DevExpress.XtraReports.UI;

namespace SofasisERP.Module.Reports;

public class HesapEkstresiRaporu : XtraReport
{
    public HesapEkstresiRaporu()
    {
        HesapEkstresiRaporuBuilder.Build(this);
    }
}
