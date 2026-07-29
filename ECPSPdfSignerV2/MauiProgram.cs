using CommunityToolkit.Maui;
using ECPSPdfSignerV2.Data;
using ECPSPdfSignerV2.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace ECPSPdfSignerV2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder
               .UseMauiApp<App>()
               .UseMauiCommunityToolkit();

            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            }).UseMauiCommunityToolkit();
            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif


            IServiceCollection services = builder.Services;
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(
                    new LoggerConfiguration()
                        .WriteTo.Debug()
                        .WriteTo.File(
                            formatter: new CompactJsonFormatter(),
                            path: Path.Combine(FileSystem.Current.AppDataDirectory, "log.txt"), // C:\Users\ASUS\AppData\Local\Packages\com.dohatec.PQSigner_9zz4h110yvjzm\LocalState
                            rollingInterval: RollingInterval.Month,
                            rollOnFileSizeLimit: true)
                        .CreateLogger());
            });

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthenticationStateProvider>());
            builder.Services.AddScoped<APIService>();
            builder.Services.AddSingleton<DatabaseLocal>();
            builder.Services.AddScoped<DataService>();
            builder.Services.AddSingleton<AppState>();
      
            return builder.Build();
        }
    }
}