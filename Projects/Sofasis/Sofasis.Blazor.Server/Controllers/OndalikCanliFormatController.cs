using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Blazor.Server.Controllers
{
    // Sofasis.Module.Controllers.OndalikFormatController'ın (platform-agnostic) tamamlayıcısı.
    // O controller "Tutar" kategorisinin dinamik para birimi sembolünü yalnızca DetailView AÇILIRKEN
    // (OnActivated'da) doğru kurar — kullanıcı canlı testte, ekran açıkken Döviz Kodu'nu değiştirdiğinde
    // (ki bu CariHesapHareketleri.OnChanged içinde DovizKuru'yu da otomatik günceller — bkz.
    // DovizKuruGuncelle) sembolün kaydet+yeniden aç olmadan ANINDA güncellenmesini istedi:
    // "sayfa kaydettikten sonra kurlar güncelleniyor.. mask güncellemesi dovizkuru onchange'de de
    // çalışmalı".
    //
    // Bu, Blazor'a ÖZGÜ bir çözüm gerektirir, bu yüzden platform-agnostic Module projesinde DEĞİL,
    // burada (host projesi) yaşar: DevExpress'in üst-seviye PropertyEditor.DisplayFormat'ı, kontrol
    // OLUŞTURULDUKTAN SONRA değiştirilirse UI'a yansımaz (resmi DX dokümantasyonunda doğrulanmış bir
    // sınırlama). Canlı yansımanın TEK yolu NumericPropertyEditor.ComponentModel'dir (DxSpinEditModel<T>,
    // doğrudan DxSpinEdit<T> Blazor bileşenine bağlı canlı proxy) — bu, Blazor'un kendi parametre-
    // değişimi/render mekanizmasını kullandığı için kontrol zaten oluşmuş olsa bile anında yansır.
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
}
