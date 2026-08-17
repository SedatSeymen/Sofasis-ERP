/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : OndalikCanliFormatController.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : OndalikFormatController'ın (platform-agnostic, Module) Blazor'a
 *                    özgü tamamlayıcısı — kullanıcı ekran açıkken Döviz Kodu'nu
 *                    değiştirdiğinde "Tutar" kategorisinin sembolünü kaydet+yeniden
 *                    aç olmadan ANINDA günceller. Eski projeden aynen uyarlandı.
 *
 *                    NEDEN Blazor.Server'da (Module'de DEĞİL): DevExpress'in
 *                    üst-seviye PropertyEditor.DisplayFormat'ı, kontrol
 *                    OLUŞTURULDUKTAN SONRA değiştirilirse UI'a yansımaz (resmi DX
 *                    dokümantasyonunda doğrulanmış bir sınırlama). Canlı yansımanın
 *                    TEK yolu NumericPropertyEditor.ComponentModel'dir
 *                    (DxSpinEditModel<T>, doğrudan DxSpinEdit<T> Blazor bileşenine
 *                    bağlı canlı proxy) — kontrol zaten oluşmuş olsa bile anında
 *                    yansır. Bu, Blazor'a özgü bir API olduğu için platform-agnostic
 *                    Module projesinde yaşayamaz.
 * ****************************************************************************
 */

using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.Persistent.Base;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Blazor.Server.Controllers;

public class OndalikCanliFormatController : ViewController<DetailView>
{
    readonly Dictionary<string, NumericPropertyEditor> canliTutarEditorleri = new();

    protected override void OnActivated()
    {
        base.OnActivated();

        object nesne = View.CurrentObject;
        if (nesne == null) return;

        Type tip = nesne.GetType();
        string kisaSinifAdi = tip.Name;

        foreach (PropertyInfo prop in tip.GetProperties())
        {
            if (prop.PropertyType != typeof(decimal) && prop.PropertyType != typeof(decimal?))
                continue;
            if (!OndalikAlanKatalogu.Kategoriler.TryGetValue((kisaSinifAdi, prop.Name), out string kategori))
                continue;
            if (kategori != "Tutar")
                continue; // yalnızca dinamik sembollü kategori canlı güncelleme gerektirir

            string alanAdi = prop.Name;
            View.CustomizeViewItemControl<NumericPropertyEditor>(this,
                editor => canliTutarEditorleri[alanAdi] = editor, alanAdi);
        }

        View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
    }

    protected override void OnDeactivated()
    {
        View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        canliTutarEditorleri.Clear();
        base.OnDeactivated();
    }

    void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    {
        if (canliTutarEditorleri.Count == 0) return;
        if (e.PropertyName != "DovizTanim" && e.PropertyName != "DovizKuru") return;
        CanliSembolleriGuncelle();
    }

    void CanliSembolleriGuncelle()
    {
        object nesne = View.CurrentObject;
        if (nesne == null) return;
        Type tip = nesne.GetType();

        int tutarHane = 2;
        try
        {
            GenelParametre parametre = View.ObjectSpace.GetObjects<GenelParametre>().FirstOrDefault();
            if (parametre != null)
                tutarHane = (int)parametre.TutarOndalikMaski;
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError(ex);
        }

        DovizTanim doviz = OndalikAlanKatalogu.DovizTaniminiCoz(nesne, tip);
        string sembol = doviz?.Sembol ?? "₺";
        string displayFormat = sembol + "{0:N" + tutarHane + "}";

        foreach (NumericPropertyEditor editor in canliTutarEditorleri.Values)
            editor.ComponentModel.DisplayFormat = displayFormat;
    }
}
