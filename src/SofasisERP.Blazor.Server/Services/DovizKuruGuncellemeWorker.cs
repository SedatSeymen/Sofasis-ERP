/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DovizKuruGuncellemeWorker.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Uygulama açıkken periyodik olarak (ve ilk açılışta hemen)
 *                    döviz kuru güncellemesini tetikleyen arka plan servisi.
 *                    Kullanıcı isteğiyle WindowController YERİNE burada —
 *                    Blazor Server'da bir WindowController her kullanıcı
 *                    circuit'inde ayrı tetiklenir (N kullanıcı = N gereksiz
 *                    TCMB isteği); BackgroundService process başına TEK
 *                    örnek çalışır, doğru sorumluluk burada.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Services;

namespace SofasisERP.Blazor.Server.Services;

public sealed class DovizKuruGuncellemeWorker : BackgroundService
{
    static readonly TimeSpan KontrolAraligi = TimeSpan.FromHours(6);

    readonly IServiceProvider serviceProvider;

    public DovizKuruGuncellemeWorker(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(KontrolAraligi);
        do
        {
            GuncellemeyiDene();
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    void GuncellemeyiDene()
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            INonSecuredObjectSpaceFactory objectSpaceFactory =
                scope.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
            IDovizKuruGuncellemeServisi guncellemeServisi =
                scope.ServiceProvider.GetRequiredService<IDovizKuruGuncellemeServisi>();

            using IObjectSpace objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<DovizGunlukKurM>();
            guncellemeServisi.BugununKuruGerekirseGuncelle(objectSpace);
        }
        catch (Exception ex)
        {
            // Ağ erişimi yok / TCMB yayınlamadı / DB henüz hazır değil: sessizce atla,
            // bir sonraki periyotta tekrar denenir. Worker asla uygulamayı durdurmaz.
            Tracing.Tracer.LogError(ex);
        }
    }
}
