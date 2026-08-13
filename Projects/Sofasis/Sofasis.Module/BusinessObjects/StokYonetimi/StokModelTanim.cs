using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("StokModelAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Stok Model Tanımlama")]

    [Appearance("StokModelTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]

    public class StokModelTanim : BaseClassWithAudit
    {
        public StokModelTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string stokModelAdiIngilizce;
        bool isVarsayilan;
        string stokModelAdi;


        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Stok Model Adı"), ToolTip("Model Adını Giriniz")]
        [RuleRequiredField("RuleRequired_StokModelTanim_ModelAdi", DefaultContexts.Save, "Lütfen Stok Model Adını Giriniz...")]
        public string StokModelAdi
        {
            get => stokModelAdi;
            set => SetPropertyValue(nameof(StokModelAdi), ref stokModelAdi, value);
        }


        [Size(50)]
        [XafDisplayName("Stok Model İngilizce Adı"), ToolTip("Model İngilizce Adını Giriniz")]
        public string StokModelAdiIngilizce
        {
            get => stokModelAdiIngilizce;
            set => SetPropertyValue(nameof(StokModelAdiIngilizce), ref stokModelAdiIngilizce, value);
        }

        [XafDisplayName("Varsayılan mı ?")]
        public bool IsVarsayilan
        {
            get => isVarsayilan;
            set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
        }

        // Model fotoğrafı: MediaDataObject (DevExpress Business Class Library) kullanılır — DB'de
        // saklanır ama HER ZAMAN gecikmeli (delayed) yüklenir ve tarayıcı tarafında sabit bir
        // MediaDataKey URL'i üzerinden servis edilip önbelleğe alınır (bkz. docs/CHANGELOG.md
        // 2026-08-13 — DX docs: "reduces traffic because images are cached in the browser cache").
        // Eski FileSystemStoreObject (disk) + elle yazılmış Thumbnail/IMemoryCache deseni bu
        // ekran için gereksizleşti; WW_FiyatListeRapor (rapor motoru satır satır okuduğundan
        // gecikmeli yükleme orada N+1 riski taşır) BİLEREK bu geçişin dışında bırakıldı.
        MediaDataObject resim;
        [XafDisplayName("Resim")]
        [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorCustomHeight = 32)]
        public MediaDataObject Resim
        {
            get => resim;
            set => SetPropertyValue(nameof(Resim), ref resim, value);
        }


        protected override void OnSaving()
        {
            if (IsVarsayilan)
            {
                StokModelTanim entity = Session.FindObject<StokModelTanim>(
                    new BinaryOperator(nameof(StokModelTanim.IsVarsayilan), true));
                if (entity != null)
                {
                    entity.IsVarsayilan = false;
                    entity.Save();
                }
            }
            base.OnSaving();
        }

        protected override void OnDeleting()
        {
            try
            {
                // B1: Silme koruması aktif — kullanımdaki model silinemez.
                if (new XPQuery<StokTanim>(Session).Count(x => x.StokModelTanim != null && x.StokModelTanim == this) > 0)
                    throw new UserFriendlyException("Bu model stok tanımlarında kullanılmış, silemezsiniz.");
                base.OnDeleting();
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
                throw;
            }

        }
    }
}