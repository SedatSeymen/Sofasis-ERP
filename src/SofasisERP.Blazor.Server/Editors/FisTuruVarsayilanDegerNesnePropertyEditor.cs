/* ****************************************************************************
 * Proje     : Sofasis Erp Project
 * Dosya     : FisTuruVarsayilanDegerNesnePropertyEditor.cs
 * Açıklama  : FisTuruVarsayilanDegeri.VarsayilanDegerNesne alanı için, HedefTip'e
 *             göre veri kaynağı DİNAMİK olarak değişen bir ComboBox editörü
 *             (eski projeden aynen taşındı). HedefTip her değiştiğinde
 *             (ImmediatePostData ile) bu editör kendini yeniler ve seçili
 *             tipin kayıtlarını listeler — ayrı bir "Değer Seç" düğmesine
 *             gerek kalmaz.
 * ****************************************************************************
 */

using System;
using System.Linq;
using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Components.Models;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Utils;
using Microsoft.AspNetCore.Components;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Blazor.Server.Editors;

public class FisTuruVarsayilanDegerLookupItem
{
    public object Instance { get; set; }
    public string DisplayText { get; set; }
}

// isDefault=true: uygulamadaki TEK "object" tipli property (VarsayilanDegerNesne) olduğu
// için bunu global varsayılan yapmak güvenli — Model.xafml'de PropertyEditorType string'i
// ile hedefleme denendi ama tip string'i çözümlenmedi (View.FindItem hep DefaultPropertyEditor
// döndürdü); isDefault=true bu belirsizliği tamamen ortadan kaldırıyor.
[PropertyEditor(typeof(object), true)]
public class FisTuruVarsayilanDegerNesnePropertyEditor : BlazorPropertyEditorBase, IComplexViewItem
{
    IObjectSpace objectSpace;

    public FisTuruVarsayilanDegerNesnePropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model)
    {
    }

    public override DxComboBoxModel<FisTuruVarsayilanDegerLookupItem, object> ComponentModel
        => (DxComboBoxModel<FisTuruVarsayilanDegerLookupItem, object>)base.ComponentModel;

    protected override IComponentModel CreateComponentModel()
    {
        var model = new DxComboBoxModel<FisTuruVarsayilanDegerLookupItem, object>
        {
            ValueFieldName = nameof(FisTuruVarsayilanDegerLookupItem.Instance),
            TextFieldName = nameof(FisTuruVarsayilanDegerLookupItem.DisplayText),
            NullText = "(Seçilmedi)",
            ClearButtonDisplayMode = DataEditorClearButtonDisplayMode.Auto,
        };
        model.ValueChanged = EventCallback.Factory.Create<object>(this, value =>
        {
            model.Value = value;
            OnControlValueChanged();
            WriteValue();
        });
        return model;
    }

    // ObjectSpace, GetObjects(Type) çağrısı için burada tutulur. HedefTip değiştiğinde bu
    // editörün yenilenmesi (Refresh) IComplexViewItem üzerinden DEĞİL, SofasisERP.Module\
    // Controllers\FisTuruVarsayilanDegeriHedefTipDegistiController tarafından tetiklenir —
    // bu editör seviyesinde ObjectSpace.ObjectChanged aboneliği denendi ama bu nesnenin
    // açıldığı nested popup bağlamında güvenilir şekilde tetiklenmedi; Controller
    // seviyesindeki abonelik (aynı olay, View.ObjectSpace üzerinden) güvenilir çalışıyor.
    void IComplexViewItem.Setup(IObjectSpace objectSpace, XafApplication application)
    {
        this.objectSpace = objectSpace;
    }

    protected override void ReadValueCore()
    {
        base.ReadValueCore();
        Type hedefTip = (CurrentObject as FisTuruVarsayilanDegeri)?.HedefTip;
        ComponentModel.Data = hedefTip == null
            ? Enumerable.Empty<FisTuruVarsayilanDegerLookupItem>()
            : objectSpace.GetObjects(hedefTip).Cast<object>()
                .Select(o => new FisTuruVarsayilanDegerLookupItem { Instance = o, DisplayText = CaptionHelper.GetDisplayText(o) })
                .ToList();
        ComponentModel.Enabled = hedefTip != null;
        ComponentModel.Value = PropertyValue;
    }

    protected override object GetControlValueCore() => ComponentModel.Value;

    protected override void ApplyReadOnly()
    {
        base.ApplyReadOnly();
        if (ComponentModel != null)
            ComponentModel.ReadOnly = !AllowEdit;
    }
}
