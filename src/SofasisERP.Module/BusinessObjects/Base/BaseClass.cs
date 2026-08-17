/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BaseClass.cs
 * Oluşturma Tarihi : [İlk oluşturma tarihi korunacaktır]
 * Oluşturan        : [İlk oluşturan korunacaktır]
 * Son Güncelleme   : [Dosya değişiklik tarihi]
 * Son Güncelleyen  : [Dosyayı değiştiren]
 * Açıklama         : BaseClass sınıfı
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using System.ComponentModel;
using System.Drawing;
using System.Linq;


namespace Sofasis.Module.BusinessObjects;

[NonPersistent]
[DeferredDeletion(enabled: false)]
// Sistem kaydı ise liste görünümünde kırmızı renk uygula
[Appearance("SystemRecord_Color", AppearanceItemType = "ViewItem",
TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "ListView",
    FontColor = "Red")]
// Varsayılan kayıt ise turuncu ve kalın yazı tipi uygula
[Appearance("IsDefaultColor", AppearanceItemType = "ViewItem",
TargetItems = "*", Criteria = "IsDefault = 1", Context = "",
    FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold)]
public abstract class BaseClass : BaseObject
{
    public BaseClass() : base()
    {
    }

    public BaseClass(Session session) : base(session)
    {
    }

    Type integrationSourceEntity;
    bool isDefault;
    bool isSystemRecord;
    Guid? integrationCode;

    [Indexed]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInDashboards(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [HideInUI(HideInUI.All)]
    [Browsable(false)]
    [XafDisplayName("Entegrasyon Kodu")]
    public Guid? IntegrationCode
    {
        get => integrationCode;
        set => SetPropertyValue(nameof(IntegrationCode), ref integrationCode, value);
    }

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInDashboards(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [HideInUI(HideInUI.All)]
    [Browsable(false)]
    [XafDisplayName("Entegrasyon Yapan Nesne")]
    [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
    [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
    public Type IntegrationSourceEntity
    {
        get => integrationSourceEntity;
        set => SetPropertyValue(nameof(IntegrationSourceEntity), ref integrationSourceEntity, value);
    }

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInDashboards(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [Browsable(false)]
    [XafDisplayName("Sistem Kaydı mı?")]
    public bool IsSystemRecord
    {
        get => isSystemRecord;
        set => SetPropertyValue(nameof(IsSystemRecord), ref isSystemRecord, value);
    }

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInDashboards(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [XafDisplayName("Varsayılan mı?")]
    public bool IsDefault
    {
        get => isDefault;
        set => SetPropertyValue(nameof(IsDefault), ref isDefault, value);
    }

    protected override void OnDeleting()
    {
        try
        {
            var deletedObject = this;
            if (deletedObject == null) return;

            // Sistem kaydı silinemez
            object value = deletedObject.GetMemberValue("IsSystemRecord");
            if (value != null && Convert.ToBoolean(value) == true)
            {
                throw new UserFriendlyException("Bu Bir Sistem kaydıdır !!! Silemezsiniz");
            }
            else
            {
                ObjectDeleting();
                base.OnDeleting();
            }
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

    protected override void OnSaving()
    {
        try
        {
            // IsDefault alanı varsa diğer varsayılan kaydı sıfırla
            XPMemberInfo info = this.ClassInfo.FindMember("IsDefault");
            if (info != null)
            {
                object value = this.GetMemberValue("IsDefault");

                if (value != null && Convert.ToBoolean(value) == true)
                {
                    var entity = Session.FindObject(this.GetType(),
                        new BinaryOperator("IsDefault", true));

                    // Aynı nesnenin kendini sıfırlamasını engelle
                    if (entity != null && !ReferenceEquals(entity, this))
                    {
                        ((XPBaseObject)entity).SetMemberValue("IsDefault", false);
                        ((XPBaseObject)entity).Save();
                    }
                }
            }

            // IsDefault durumundan bağımsız olarak her zaman çağrılmalı
            ObjectSaving();
            base.OnSaving();
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

    public virtual void ObjectDeleting()
    {
    }

    public virtual void ObjectSaving()
    {
    }
}
