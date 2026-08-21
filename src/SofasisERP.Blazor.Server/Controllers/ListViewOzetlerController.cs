/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ListViewOzetlerController.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kullanıcı isteği (2026-08-18) — eski ERP'nin
 *                    SummaryController.cs'i (Temp/ klasörü, incelenip buraya
 *                    taşındı) birebir davranışıyla, yalnızca hariç tutulan
 *                    alan adları BU projenin gerçek Base sınıf üyeleriyle
 *                    (BaseClass/BaseClassWithAudit/BaseClassWithDescription)
 *                    eşleştirilerek uyarlandı — eski ERP'nin "OzelKod1/2",
 *                    "Aciklama", "CreateUserID/UpdateUserID", "CreateDate/
 *                    UpdateDate", "IsVarsayilan" adları burada sırasıyla
 *                    "CustomCode1/2", "Description", "CreatedBy/ModifiedBy",
 *                    "CreatedDate/ModifiedDate", "IsDefault" — ayrıca bu
 *                    projenin BaseClass'ında olup eski projede olmayan
 *                    "IntegrationCode"/"IntegrationSourceEntity" (tamamen
 *                    gizli entegrasyon alanları) listeye EKLENDİ.
 *
 *                    Her ListView'in araç çubuğuna "Özetler" adlı bir
 *                    SingleChoiceAction ekler — kullanıcı herhangi bir alanı
 *                    ve toplama fonksiyonunu (Toplam/En Küçük/En Büyük/Say/
 *                    Ortalama) seçip alt toplam satırına canlı ekleyebilir,
 *                    "Kaldır" ile tüm özetleri temizleyebilir.
 * ****************************************************************************
 */

using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Blazor.Editors.Models;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Utils;

namespace SofasisERP.Blazor.Server.Controllers;

public class ListViewOzetlerController : ViewController<ListView>
{
    static readonly string[] HaricTutulanAlanlar =
    {
        "Oid", "This", "Loading", "ClassInfo", "Session", "IsLoading", "IsDeleted", "KeyID",
        "IsSystemRecord", "IsDefault", "CreatedDate", "ModifiedDate", "CreatedBy", "ModifiedBy",
        "CustomCode1", "CustomCode2", "Description", "IntegrationCode", "IntegrationSourceEntity"
    };

    readonly SingleChoiceAction ozetlerAction;

    public ListViewOzetlerController()
    {
        ozetlerAction = new SingleChoiceAction(this, "ListViewOzetlerAction", PredefinedCategory.Edit)
        {
            Caption = "Özetler",
            ItemType = SingleChoiceActionItemType.ItemIsOperation
        };
        ozetlerAction.Execute += OzetlerAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        ozetlerAction.Items.Clear();
        Type tip = ((TypeInfo)View.ObjectTypeInfo).Type;
        foreach (var prop in tip.GetProperties())
        {
            if (Array.IndexOf(HaricTutulanAlanlar, prop.Name) >= 0
                || prop.PropertyType.IsVisible == false
                || prop.PropertyType.IsEnum
                || prop.PropertyType.Name.StartsWith("XPCollection"))
                continue;

            string displayName = string.Empty;
            foreach (var attr in prop.CustomAttributes)
            {
                if (attr.AttributeType.Name == "XafDisplayNameAttribute")
                {
                    displayName = attr.ConstructorArguments[0].Value.ToString();
                    break;
                }
            }

            var alanItem = new ChoiceActionItem(prop.Name, displayName, prop.Name);
            ozetlerAction.Items.Add(alanItem);
            OzetSecenekleriEkle(alanItem);
        }
        ozetlerAction.Items.Add(new ChoiceActionItem("Kaldır", "Tüm Alt Toplamları Temizle", null));
    }

    static void OzetSecenekleriEkle(ChoiceActionItem alanItem)
    {
        EnumDescriptor ed = new EnumDescriptor(typeof(GridSummaryItemType));
        foreach (object deger in ed.Values)
        {
            string ad = deger.ToString();
            if (ad == "Custom" || ad == "None") continue;

            string caption = ad switch
            {
                "Sum" => "Toplam",
                "Min" => "En Küçük",
                "Max" => "En Büyük",
                "Count" => "Say",
                "Avg" => "Ortalama",
                "Custom" => "Özel",
                "None" => "Hiçbiri",
                _ => ad
            };
            alanItem.Items.Add(new ChoiceActionItem(caption, deger));
        }
    }

    void OzetlerAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
    {
        if (e.SelectedChoiceActionItem.Items.Count > 0) return;
        if (View.Editor is not DxGridListEditor editor) return;

        if (e.SelectedChoiceActionItem.Id == "Kaldır")
        {
            editor.GridSummary.TotalSummary.Clear();
        }
        else if (e.SelectedChoiceActionItem.ParentItem?.Data != null)
        {
            string alanAdi = e.SelectedChoiceActionItem.ParentItem.Data.ToString();
            var ozetTipi = (GridSummaryItemType)e.SelectedChoiceActionItem.Data;

            // TotalSummary'deki mevcut öğelerin SummaryType'ı DevExpress.Data.SummaryItemType
            // (Blazor'a özgü GridSummaryItemType DEĞİL) — eski ERP'nin ConvertSummaryItemType
            // dönüştürücüsü bu yüzden gerekli, aynen taşındı. (Eski koddaki dedup kontrolü
            // FieldName'i ParentItem.Caption ile karşılaştırıyordu — FieldName aslında
            // ParentItem.Data'dan set edildiği için bu asla eşleşmezdi; burada alanAdi ile
            // karşılaştırılarak düzeltildi.)
            bool zatenVar = editor.GridSummary.TotalSummary
                .Any(s => s.SummaryType == OzetTipineDonustur(ozetTipi) && s.FieldName == alanAdi);
            if (!zatenVar)
            {
                editor.GridSummary.TotalSummary.Add(new DxGridSummaryItemWrapper(new DxGridSummaryItemModel
                {
                    SummaryType = ozetTipi,
                    FieldName = alanAdi,
                    FooterColumnName = alanAdi
                }));
            }
        }
        editor.Refresh();
    }

    static DevExpress.Data.SummaryItemType OzetTipineDonustur(GridSummaryItemType tip) => tip switch
    {
        GridSummaryItemType.Sum => DevExpress.Data.SummaryItemType.Sum,
        GridSummaryItemType.Min => DevExpress.Data.SummaryItemType.Min,
        GridSummaryItemType.Max => DevExpress.Data.SummaryItemType.Max,
        GridSummaryItemType.Count => DevExpress.Data.SummaryItemType.Count,
        GridSummaryItemType.Avg => DevExpress.Data.SummaryItemType.Average,
        GridSummaryItemType.Custom => DevExpress.Data.SummaryItemType.Custom,
        _ => DevExpress.Data.SummaryItemType.None
    };
}
