using System;
using System.Linq;
using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // Whitelist'teki (Resources/Seed/ondalik-alan-listesi.csv, OndalikAlanKatalogu) HER kategoriyi
    // (Miktar/Tutar/Yerel/Maliyet) doğrudan DetailView'ın PropertyEditor'üne uygular.
    //
    // NEDEN Module.cs'in SetupComplete-zamanlı Model mutasyonu YETMEDİ (bu controller'ın var olma
    // sebebi): kullanıcı canlı testte GenelParametre.MiktarOndalikMaski=2 iken StokTanim.En/Boy/
    // Yükseklik/ToplamMiktar'ın hâlâ 4 hane gösterdiğini yakaladı. Kök neden: bu alanların C#
    // tarafında (artık kaldırılmış) kendi `[ModelDefault("DisplayFormat",...)]` attribute'leri
    // vardı — bu, DetailView'ın PropertyEditor (view-item) düğümünde Module.cs'in BOModel-üyesi
    // seviyesindeki değerinden DAHA SPESİFİK bir değer oluşturuyordu ve XAF'ın Model düğüm
    // hiyerarşisinde daha spesifik düğüm kazanıyordu — SIRA meselesi değil, SPESİFİKLİK meselesi.
    // Çakışan attribute'ler artık kaynağında kaldırıldı (bkz. CHANGELOG), ama bu controller EK bir
    // güvenlik katmanı: DetailView'ın GERÇEK PropertyEditor kontrolüne, Model çözümlemesi bittikten
    // SONRA (OnActivated'da) doğrudan yazar — hangi düğüm/attribute ne derse desin son söz burada.
    //
    // BONUS: Module.cs'in process-ömrü-boyunca-bir-kez mekanizmasının aksine, bu controller HER
    // DetailView açılışında GenelParametre'yi TAZE okur — GenelParametre değişiklikleri artık bu
    // controller'ın kapsadığı alanlarda SUNUCU YENİDEN BAŞLATMASI beklemeden bir sonraki ekran
    // açılışında yansır (Module.cs'in BOModel-seviyesi varsayımı, ListView sütunları gibi bu
    // controller'ın kapsam dışı bıraktığı yerler için hâlâ process-ömrü-boyunca-bir-kez çalışır).
    public class OndalikFormatController : ViewController<DetailView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            FormatlariUygula();
        }

        void FormatlariUygula()
        {
            object nesne = View.CurrentObject;
            if (nesne == null) return;

            Type tip = nesne.GetType();
            string kisaSinifAdi = tip.Name;

            int miktarHane = 4, tutarHane = 2;
            try
            {
                GenelParametre parametre = View.ObjectSpace.GetObjects<GenelParametre>().FirstOrDefault();
                if (parametre != null)
                {
                    miktarHane = (int)parametre.MiktarOndalikMaski;
                    tutarHane = (int)parametre.TutarOndalikMaski;
                }
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
            }

            DovizTanim doviz = null;
            bool dovizAranmisti = false;

            foreach (PropertyInfo prop in tip.GetProperties())
            {
                if (prop.PropertyType != typeof(decimal) && prop.PropertyType != typeof(decimal?))
                    continue;
                if (!OndalikAlanKatalogu.Kategoriler.TryGetValue((kisaSinifAdi, prop.Name), out string kategori))
                    continue;
                if (View.FindItem(prop.Name) is not PropertyEditor editor)
                    continue;

                string sembol;
                int hane;
                switch (kategori)
                {
                    case "Miktar":
                        sembol = string.Empty;
                        hane = miktarHane;
                        break;
                    case "Tutar":
                        if (!dovizAranmisti)
                        {
                            doviz = OndalikAlanKatalogu.DovizTaniminiCoz(nesne, tip);
                            dovizAranmisti = true;
                        }
                        sembol = doviz?.Sembol ?? "₺";
                        hane = tutarHane;
                        break;
                    case "Yerel":
                        sembol = "₺";
                        hane = tutarHane;
                        break;
                    case "Maliyet":
                        sembol = "₺";
                        hane = miktarHane;
                        break;
                    default:
                        continue;
                }

                string mask = "N" + hane;
                editor.EditMask = mask;
                editor.DisplayFormat = sembol + "{0:" + mask + "}";
            }
        }

    }

    // ListView sütunları için eşdeğeri. Module.cs'in SetupComplete-zamanlı Model mutasyonu bu
    // sütunlara ULAŞAMIYORDU — canlı testte StokBakiye_ListView'de kanıtlandı (sembol ₺ doğru
    // geliyordu ama hane hep 4 kalıyordu, Module.cs'in düzeltmesine RAĞMEN). Kök neden büyük
    // ihtimalle: `application.Model.Views` koleksiyonu SetupComplete anında henüz ziyaret
    // edilmemiş (tembel/lazy oluşturulan) ListView düğümlerini içermiyor. Bu controller, ListView
    // AÇILDIĞINDA (OnActivated'da) doğrudan gerçek Column düğümlerine yazdığı için bu sorunu
    // yaşamaz — DetailView'daki OndalikFormatController ile birebir aynı mantık, yalnızca hedef
    // PropertyEditor değil IModelColumn.
    //
    // Tutar kategorisinde satır-bazlı DİNAMİK sembol (her satırın kendi DovizTanim'ine göre farklı
    // sembol) bir grid sütununda TEK bir statik DisplayFormat string'i ile mümkün değil — bu,
    // kullanıcının ayrı ve daha kapsamlı "özel hücre şablonu" işi olarak bilinçli şekilde
    // ERTELEDİĞİ kapsamdır. Bu yüzden Tutar sütunlarına da (Maliyet/Yerel gibi) sabit ₺ uygulanır;
    // yalnızca hane sayısı garanti edilir, sembol ileride cell-template işiyle dinamikleşecektir.
    public class OndalikFormatListController : ViewController<ListView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            FormatlariUygula();
        }

        void FormatlariUygula()
        {
            if (View.Model is not IModelListView modelListView) return;
            Type tip = View.ObjectTypeInfo.Type;
            string kisaSinifAdi = tip.Name;

            int miktarHane = 4, tutarHane = 2;
            try
            {
                GenelParametre parametre = View.ObjectSpace.GetObjects<GenelParametre>().FirstOrDefault();
                if (parametre != null)
                {
                    miktarHane = (int)parametre.MiktarOndalikMaski;
                    tutarHane = (int)parametre.TutarOndalikMaski;
                }
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
            }

            foreach (PropertyInfo prop in tip.GetProperties())
            {
                if (prop.PropertyType != typeof(decimal) && prop.PropertyType != typeof(decimal?))
                    continue;
                if (!OndalikAlanKatalogu.Kategoriler.TryGetValue((kisaSinifAdi, prop.Name), out string kategori))
                    continue;
                IModelColumn column = modelListView.Columns[prop.Name];
                if (column == null) continue;

                string sembol;
                int hane;
                switch (kategori)
                {
                    case "Miktar":
                        sembol = string.Empty;
                        hane = miktarHane;
                        break;
                    case "Tutar":
                    case "Yerel":
                        sembol = "₺";
                        hane = tutarHane;
                        break;
                    case "Maliyet":
                        sembol = "₺";
                        hane = miktarHane;
                        break;
                    default:
                        continue;
                }

                string mask = "N" + hane;
                column.EditMask = mask;
                column.DisplayFormat = sembol + "{0:" + mask + "}";
            }
        }
    }
}
