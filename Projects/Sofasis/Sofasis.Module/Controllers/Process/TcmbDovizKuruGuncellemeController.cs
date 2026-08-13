using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // Ana pencere aktifleştiğinde (kullanıcı girişinden hemen sonra, oturum başına bir kez)
    // bugüne ait TCMB kuru eksik olan (hiç çekilmemiş VEYA sonradan tanımlanan bir döviz
    // için satırı henüz olmayan) dövizleri tamamlar. Ağ hatası girişi ASLA engellemez veya
    // bir istisna ile kullanıcıya yansımaz; yalnızca izlenir (Tracing) ve sessizce atlanır —
    // bir sonraki girişte tekrar denenir.
    public class TcmbDovizKuruGuncellemeController : WindowController
    {
        readonly IDovizKuruService dovizKuruService = new TcmbDovizKuruService();

        public TcmbDovizKuruGuncellemeController()
        {
            TargetWindowType = WindowType.Main;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            try
            {
                BugununKuruGerekirseGuncelle();
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
            }
        }

        void BugununKuruGerekirseGuncelle()
        {
            DateTime bugun = DateTime.UtcNow.Date;
            using IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(DovizGunlukKurM));

            // TRY taban birimdir; TCMB kur listesinde yayınlanmaz, kur girilmez.
            List<DovizTanim> tumDovizler = objectSpace.GetObjects<DovizTanim>()
                .Where(x => x.DovizKodu != "TRY")
                .ToList();

            DovizGunlukKurM master = objectSpace.FindObject<DovizGunlukKurM>(
                CriteriaOperator.FromLambda<DovizGunlukKurM>(x => x.KurTarihi == bugun));

            HashSet<string> mevcutKodlar = master == null
                ? new HashSet<string>()
                : master.DovizGunlukKurDetails.Select(x => x.DovizTanim.DovizKodu).ToHashSet();

            bool eksikVar = tumDovizler.Any(d => !mevcutKodlar.Contains(d.DovizKodu));
            if (!eksikVar)
            {
                return; // Bugüne ait, tanımlı tüm dövizlerin kuru zaten var; TCMB'ye tekrar sorma.
            }

            var kurlar = dovizKuruService.KurlariCek(bugun);
            if (kurlar.Count == 0)
            {
                return; // TCMB'ye ulaşılamadı; sessizce vazgeç, sonraki girişte tekrar denenir.
            }
            Dictionary<string, DovizKuruDto> kurSozluk = kurlar.ToDictionary(x => x.DovizKodu);

            master ??= objectSpace.CreateObject<DovizGunlukKurM>();
            master.KurTarihi = bugun;
            master.KurSaati = DateTime.UtcNow;

            foreach (DovizTanim dovizTanim in tumDovizler)
            {
                if (mevcutKodlar.Contains(dovizTanim.DovizKodu))
                {
                    continue; // Bu döviz için bugünün satırı zaten var.
                }
                if (!kurSozluk.TryGetValue(dovizTanim.DovizKodu, out DovizKuruDto kur))
                {
                    continue; // TCMB bugün bu kodu yayınlamıyor — atla.
                }

                var detay = objectSpace.CreateObject<DovizGunlukKurD>();
                detay.DovizGunlukKurMaster = master;
                detay.DovizTanim = dovizTanim;
                detay.KurTarihi = bugun;
                detay.DovizAlis = kur.DovizAlis;
                detay.DovizSatis = kur.DovizSatis;
                detay.EfektifDovizAlis = kur.EfektifAlis;
                detay.EfektifDovizSatis = kur.EfektifSatis;
            }

            objectSpace.CommitChanges();
        }
    }
}
