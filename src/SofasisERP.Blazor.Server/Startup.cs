using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using DevExpress.ExpressApp.Xpo;
using SofasisERP.Blazor.Server.Services;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Services;

namespace SofasisERP.Blazor.Server;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
        // https://www.npgsql.org/doc/types/datetime.html#timestamps-and-timezones
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();

        // Döviz kuru: TCMB'den çekme + güncelleme servisleri, ve process başına
        // TEK çalışan periyodik arka plan worker'ı (bkz. DovizKuruGuncellemeWorker).
        services.AddScoped<IDovizKuruService, TcmbDovizKuruService>();
        services.AddScoped<IDovizKuruGuncellemeServisi, DovizKuruGuncellemeServisi>();
        services.AddHostedService<DovizKuruGuncellemeWorker>();
        // Rapor Controller'ları (ör. HesapEkstresiRaporuController) ile aşağıdaki
        // AddReports(options.Events.OnBeforeShowPreview) arasındaki köprü — bkz.
        // Services/ReportPreviewContext.cs.
        services.AddScoped<SofasisERP.Module.Services.ReportPreviewContext>();
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<SofasisERPBlazorApplication>();
            builder.Modules
                .AddConditionalAppearance()
                .AddReports(options => {
                    options.EnableInplaceReports = true;
                    options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportDataV2);
                    options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                    // Launching Controller'ın ReportPreviewContext'e koyduğu gizli Parameter
                    // değerlerini (Hesap Adı, Devreden Bakiye, vb.) ShowPreview'dan hemen önce
                    // rapora uygular — ShowPreview yalnızca bir "handle" string aldığı için
                    // Controller'dan XtraReport örneğine başka türlü erişilemiyor.
                    options.Events.OnBeforeShowPreview = context => {
                        var previewContext = context.ServiceProvider
                            .GetService<SofasisERP.Module.Services.ReportPreviewContext>();
                        if (previewContext == null) return;
                        if (previewContext.DataSource != null) {
                            context.Report.DataSource = previewContext.DataSource;
                        }
                        foreach (var kv in previewContext.ParameterValues) {
                            if (context.Report.Parameters[kv.Key] != null) {
                                context.Report.Parameters[kv.Key].Value = kv.Value;
                            }
                        }
                    };
                })
                .AddValidation(options => {
                    options.AllowValidationDetailsAccess = false;
                })
                .Add<SofasisERP.Module.SofasisERPModule>()
                .Add<SofasisERPBlazorModule>();
            builder.ObjectSpaceProviders
                .AddXpo((serviceProvider, options) => {
                    string connectionString = null;
                    if(Configuration.GetConnectionString("ConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("ConnectionString");
                    }
#if EASYTEST
                    if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                    }
#endif
                    ArgumentNullException.ThrowIfNull(connectionString);
                    options.ConnectionString = connectionString;
                    options.ThreadSafe = true;
                    options.UseSharedDataStoreProvider = true;
                })
                .AddNonPersistent();
            // Kullanıcı isteği (2026-08-21): standart parola güvenliği (kullanıcı adı+şifre
            // ile giriş, tuzlanmış/hash'lenmiş saklama, şifre değiştirme desteği). ApplicationUser/
            // ApplicationUserLoginInfo şablon kurulumundan zaten hazır (PermissionPolicyUser +
            // ISecurityUserWithLoginInfo/ISecurityUserLockout uyguluyor) — yalnızca Security System
            // devre dışıydı. İlk Admin kullanıcısı+rolü Updater.cs'de oluşturulur (gerçek parola +
            // ilk girişte parola değiştirme zorunluluğu ile, resmi DevExpress örneğindeki boş
            // parola YERİNE — bkz. Updater.cs).
            builder.Security
                .UseIntegratedMode(options => {
                    options.RoleType = typeof(PermissionPolicyRole);
                    options.UserType = typeof(ApplicationUser);
                    options.UserLoginInfoType = typeof(ApplicationUserLoginInfo);
                    // XPO tabanlı uygulamalarda resmi DevExpress deseni: bu kayıt olmadan
                    // Security Adapter (parola doğrulama dahil) düzgün devreye girmiyordu
                    // ("Cannot login with Standard authentication type" hatasının kök nedeni
                    // — DevExpress.ExpressApp.Security.Xpo paketi eksikti, şimdi eklendi).
                    options.Events.OnSecurityStrategyCreated += securityStrategy => {
                        ((SecurityStrategy)securityStrategy).RegisterXPOAdapterProviders();
                    };
                })
                .AddPasswordAuthentication(options => {
                    options.IsSupportChangePassword = true;
                    // Giriş ekranında Kullanıcı Adı varsayılan "Admin" gelsin (kullanıcı isteği
                    // 2026-08-21) — bkz. SofasisERPLogonParameters.cs açıklaması: LastLogonParametersRead
                    // event'i yerine bu daha güvenilir yöntem tercih edildi.
                    options.LogonParametersType = typeof(SofasisERPLogonParameters);
                });
        });

        // builder.Security.UseIntegratedMode(...) yalnızca XAF'ın kendi yetkilendirme
        // motorunu kaydeder; ASP.NET Core'un çerez tabanlı kimlik doğrulama şemasını
        // (DefaultChallengeScheme) AYRICA kaydetmek gerekiyor, aksi halde XAF'ın
        // SignInMiddleware'i giriş yapmamış kullanıcıyı /api/challenge'a yönlendirdiğinde
        // "No authenticationScheme was specified" hatası fırlatıyor (resmi DevExpress
        // Blazor Startup.cs örneği).
        // Çerez güvenlik bayrakları açıkça zorlanır (22.08.2026 denetimi G7) — HTTPS/HSTS
        // mevcut olsa da bunlar aksi belirtilmeden zorunlu değildir; oturum çerezi
        // sızıntısı/CSRF yüzeyini daraltır.
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = "/LoginPage";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            // Varsayılan HSTS süresi 30 gündür. Production senaryoları için bkz. https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        // Türkçe dil paketi (bin/tr) etkin olsun diye kültür tr-TR'ye sabitlendi
        // (appsettings.json'daki DevExpress:ExpressApp:Languages ile birlikte).
        // .NET'in yerleşik tr-TR kısa tarih deseni "d.MM.yyyy" — gün tek haneliyken
        // başında sıfır YOK (ör. "1.01.2026"). Kullanıcı kararı (2026-08-22): tüm
        // tarih alanlarında gün de her zaman 2 haneli görünmeli (dd.MM.yyyy) — bu
        // yüzden ShortDatePattern açıkça ezilir. Aynı CultureInfo NESNESİ hem
        // Supported*Cultures hem DefaultRequestCulture'a verilir, aksi halde
        // RequestCulture kendi taze (düzeltilmemiş) bir tr-TR kopyası oluşturur.
        var turkce = new System.Globalization.CultureInfo("tr-TR");
        turkce.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
        var desteklenenKulturler = new[] { turkce };
        app.UseRequestLocalization(new Microsoft.AspNetCore.Builder.RequestLocalizationOptions {
            DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(turkce, turkce),
            SupportedCultures = desteklenenKulturler,
            SupportedUICultures = desteklenenKulturler
        });
        app.UseStaticFiles();
        app.UseRouting();
        // Resmi DevExpress "Use the Security System" örneği: UseAuthentication/UseAuthorization
        // burada eksikti — services.AddAuthentication(...) yalnızca DI kaydı yapar, HttpContext.User'ı
        // gerçekten dolduran bu iki middleware olmadan XAF'ın Standard authentication akışı
        // "Cannot login with Standard authentication type" hatasıyla genel bir mesaja sarılıp başarısız oluyordu.
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
