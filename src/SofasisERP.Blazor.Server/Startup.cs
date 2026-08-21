using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
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

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<SofasisERPBlazorApplication>();
            builder.Modules
                .AddConditionalAppearance()
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
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = "/LoginPage";
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        // Türkçe dil paketi (bin/tr) etkin olsun diye kültür tr-TR'ye sabitlendi
        // (appsettings.json'daki DevExpress:ExpressApp:Languages ile birlikte).
        var desteklenenKulturler = new[] { new System.Globalization.CultureInfo("tr-TR") };
        app.UseRequestLocalization(new Microsoft.AspNetCore.Builder.RequestLocalizationOptions {
            DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR"),
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
