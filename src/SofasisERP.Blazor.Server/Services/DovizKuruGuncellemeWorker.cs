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
using Microsoft.Extensions.Hosting;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Services;

namespace SofasisERP.Blazor.Server.Services;

public sealed class DovizKuruGuncellemeWorker : BackgroundService
{
    static readonly TimeSpan KontrolAraligi = TimeSpan.FromHours(6);

    readonly IServiceProvider serviceProvider;
    readonly IHostApplicationLifetime hostApplicationLifetime;

    public DovizKuruGuncellemeWorker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime)
    {
        this.serviceProvider = serviceProvider;
        this.hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // BackgroundService.StartAsync, ExecuteAsync'i ateşleyip HEMEN döner; bu Task,
        // ASP.NET Core'un geri kalan host başlatma işiyle (Configure() pipeline'ındaki
        // app.UseXaf() içinde çalışan XAF modül kurulumu/TypesInfo kaydı DAHİL) YARIŞ
        // hâlindedir. Bu yarışta erken davranılırsa DovizTanim gibi iş nesneleri henüz
        // XafTypesInfo'ya kayıtlı olmadığından ArgumentException fırlar, sessizce
        // yutulur ve KontrolAraligi (6 saat) kadar bir daha denenmez — canlı testte
        // gözlemlenen kök neden buydu. ApplicationStarted, Configure() pipeline'ı
        // (UseXaf dahil) tamamen bittikten SONRA tetiklenir; asıl "hazır" sinyali budur.
        await WaitUntilApplicationStartedAsync(stoppingToken).ConfigureAwait(false);

        using PeriodicTimer timer = new(KontrolAraligi);
        do
        {
            await GuncellemeyiDeneAsync().ConfigureAwait(false);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    async Task WaitUntilApplicationStartedAsync(CancellationToken stoppingToken)
    {
        if (hostApplicationLifetime.ApplicationStarted.IsCancellationRequested)
            return;

        TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenRegistration startedRegistration =
            hostApplicationLifetime.ApplicationStarted.Register(() => tcs.TrySetResult());
        using CancellationTokenRegistration stoppingRegistration =
            stoppingToken.Register(() => tcs.TrySetCanceled(stoppingToken));
        await tcs.Task.ConfigureAwait(false);
    }

    async Task GuncellemeyiDeneAsync()
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            INonSecuredObjectSpaceFactory objectSpaceFactory =
                scope.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
            IDovizKuruGuncellemeServisi guncellemeServisi =
                scope.ServiceProvider.GetRequiredService<IDovizKuruGuncellemeServisi>();

            using IObjectSpace objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<DovizGunlukKurM>();
            await guncellemeServisi.BugununKuruGerekirseGuncelleAsync(objectSpace).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Ağ erişimi yok / TCMB yayınlamadı / DB henüz hazır değil: sessizce atla,
            // bir sonraki periyotta tekrar denenir. Worker asla uygulamayı durdurmaz.
            Tracing.Tracer.LogError(ex);
        }
    }
}
