using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Sofasis.Module.BusinessObjects;

[DefaultClassOptions]
[XafDisplayName("Satın Alma Parametre Tanımlama")]
public class SatinAlmaParametre : BaseClassWithAudit
{ 
    public SatinAlmaParametre(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            XPCollection<SatinAlmaParametre> entityList = new XPCollection<SatinAlmaParametre>(Session);
            int count = entityList.Count;
            if (count == 1)
            {
                this.CancelEdit();
                SatinAlmaParametre entity = entityList[0];
                Session.DropChanges();
                Session.Reload(entity);
            }
        }
    }


    int? gecikenSiparisSuresi;
    int? teslimSuresi;
    int? terminSuresi;
    bool onayGerekliMi = true;
    bool kendiTalebiniOnaylayamaz = true;
    UcluEslestirmePolitikasi ucluEslestirmePolitikasi = UcluEslestirmePolitikasi.Uyar;
    decimal eslestirmeToleransYuzdesi;


    [XafDisplayName("Maksimum Termin Süresi")]
    [RuleRequiredField("R_SatinAlmaParametre_TerminSuresi", DefaultContexts.Save, "Lütfen Maksimum Termin Süresini Giriniz...")]
    [ModelDefault("DisplayFormat", "{0:N0}")]
    [ModelDefault("EditMask", "N0")]
    public int? TerminSuresi
    {
        get => terminSuresi;
        set => SetPropertyValue(nameof(TerminSuresi), ref terminSuresi, value);
    }

    [XafDisplayName("Maksimum Teslim Süresi")]
    [RuleRequiredField("R_SatinAlmaParametre_TeslimSuresi", DefaultContexts.Save, "Lütfen Maksimum Teslim Süresini Giriniz...")]
    [ModelDefault("DisplayFormat", "{0:N0}")]
    [ModelDefault("EditMask", "N0")]
    public int? TeslimSuresi
    {
        get => teslimSuresi;
        set => SetPropertyValue(nameof(TeslimSuresi), ref teslimSuresi, value);
    }

    [XafDisplayName("Geciken Süresi")]
    [RuleRequiredField("R_SatinAlmaParametre_GecikenSiparisSuresi", DefaultContexts.Save, "Lütfen Geciken Süresini Giriniz...")]
    [ModelDefault("DisplayFormat", "{0:N0}")]
    [ModelDefault("EditMask", "N0")]
    public int? GecikenSiparisSuresi
    {
        get => gecikenSiparisSuresi;
        set => SetPropertyValue(nameof(GecikenSiparisSuresi), ref gecikenSiparisSuresi, value);
    }

    private Color gecikenSiparisRengi;
    [ValueConverter(typeof(ColorValueConverter))]
    public Color GecikenSiparisRengi
    {
        get { return gecikenSiparisRengi; }
        set { SetPropertyValue(nameof(GecikenSiparisRengi), ref gecikenSiparisRengi, value); }
    }

    // Kapalı olduğunda SatinAlmaOnayServisi.Gonder() talebi doğrudan Onaylandı'ya geçirir
    // (OnayBekliyor adımı atlanır) — küçük şirket senaryosunda onay akışının tamamen
    // kapatılabilmesi için kaçış kapısı (bkz. plan: SA-6).
    [XafDisplayName("Onay Gerekli mi?")]
    public bool OnayGerekliMi
    {
        get => onayGerekliMi;
        set => SetPropertyValue(nameof(OnayGerekliMi), ref onayGerekliMi, value);
    }

    // Segregation-of-duties guard'ının kapatılabilmesi için — SatinAlmaOnayServisi.Onayla() bunu okur.
    [XafDisplayName("Kendi Talebini Onaylayamaz")]
    public bool KendiTalebiniOnaylayamaz
    {
        get => kendiTalebiniOnaylayamaz;
        set => SetPropertyValue(nameof(KendiTalebiniOnaylayamaz), ref kendiTalebiniOnaylayamaz, value);
    }

    // SA-5 (Alış Faturası) kapsamında ISatinAlmaFaturaServisi'nin üçlü eşleştirme kontrolünde
    // kullanılacak — bu fazda henüz tüketen kod yok, şema hazırlığı.
    [XafDisplayName("Üçlü Eşleştirme Politikası")]
    public UcluEslestirmePolitikasi UcluEslestirmePolitikasi
    {
        get => ucluEslestirmePolitikasi;
        set => SetPropertyValue(nameof(UcluEslestirmePolitikasi), ref ucluEslestirmePolitikasi, value);
    }

    [DbType("decimal(9,4)")]
    [XafDisplayName("Eşleştirme Tolerans Yüzdesi")]
    [ModelDefault("DisplayFormat", "{0:n4} %")]
    [ModelDefault("EditMask", "n4")]
    public decimal EslestirmeToleransYuzdesi
    {
        get => eslestirmeToleransYuzdesi;
        set => SetPropertyValue(nameof(EslestirmeToleransYuzdesi), ref eslestirmeToleransYuzdesi, value);
    }

}