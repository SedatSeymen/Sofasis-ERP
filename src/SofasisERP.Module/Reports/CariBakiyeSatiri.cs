/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariBakiyeSatiri.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Tüm Cariler Borç/Alacak Raporu"nun her satırını temsil eden
 *                    salt-veri sınıf — rapor doğrudan canlı bir XPO sorgusuna değil,
 *                    Controller'ın önceden hesapladığı bu listeye bağlanır (bkz.
 *                    TumCarilerBakiyeRaporuController, ReportPreviewContext.DataSource).
 * ****************************************************************************
 */

namespace SofasisERP.Module.Reports;

public class CariBakiyeSatiri
{
    public string CariHesapKodu { get; set; }
    public string CariHesapAdi { get; set; }
    public string DovizKodu { get; set; }
    public decimal DevredenBakiye { get; set; }
    public decimal DonemBorc { get; set; }
    public decimal DonemAlacak { get; set; }
    public decimal KapanisBakiyesi { get; set; }
}
