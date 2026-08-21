/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KDVTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : KDV oranı tanımı — eski projeden uyarlandı. FisTuruTanim
 *                    bazlı varsayılan değer hedefi olarak seçilebilmesi için
 *                    IFisTuruVarsayilanHedefi arayüzü uygulanır. KDVOrani bilerek
 *                    decimal değil int — Türkiye'de KDV oranları her zaman tam
 *                    sayıdır (%0/%1/%10/%20), decimal+ModelDefault EditMask/
 *                    DisplayFormat yalnızca PropertyEditor'ı biçimlendirir, kaydın
 *                    OTOMATİK ÜRETİLEN başlığını/lookup caption'ını (DefaultProperty
 *                    ToString()'i) ETKİLEMEZ — canlı testte "10,00000000" başlığı
 *                    olarak yakalandı. int kullanmak sorunu kökten (her render
 *                    yolunda) çözer, ayrıca maskeye gerek bırakmaz.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(KDVOrani))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("KDV Tanımlama")]
public class KDVTanim : BaseClassWithAudit, IFisTuruVarsayilanHedefi
{
    public KDVTanim(Session session) : base(session) { }

    bool isVarsayilan;
    int kDVOrani;

    [XafDisplayName("KDV Oranı"), ToolTip("KDV Oranını Giriniz")]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    public int KDVOrani
    {
        get => kDVOrani;
        set => SetPropertyValue(nameof(KDVOrani), ref kDVOrani, value);
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }

    protected override void OnSaving()
    {
        if (IsVarsayilan)
        {
            KDVTanim entity = Session.FindObject<KDVTanim>(
                CriteriaOperator.FromLambda<KDVTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
            if (entity != null)
            {
                entity.IsVarsayilan = false;
                entity.Save();
            }
        }
        base.OnSaving();
    }
}
