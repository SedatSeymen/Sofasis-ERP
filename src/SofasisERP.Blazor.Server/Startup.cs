using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DevExpress.ExpressApp.Xpo;
using SofasisERP.Blazor.Server.Services;
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
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
