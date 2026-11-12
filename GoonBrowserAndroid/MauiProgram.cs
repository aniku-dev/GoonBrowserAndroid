using CommunityToolkit.Maui;
using GoonBrowserAndroid.AdBlock;
using GoonBrowserAndroid.Services.Download;
using GoonBrowserAndroid.Services.LongPress;
using GoonBrowserAndroid.Services.Settings;
using GoonBrowserAndroid.Services.Tab;
using GoonBrowserAndroid.ViewModels;
using GoonBrowserAndroid.Views;
using Microsoft.Extensions.Logging;
#if ANDROID
using GoonBrowserAndroid.Platforms.Handlers;
#endif

namespace GoonBrowserAndroid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Ubuntu-Regular.ttf", "UbuntuRegular");
                    fonts.AddFont("Ubuntu-Bold.ttf", "UbuntuBold");
                })
                .UseMauiCommunityToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                #if ANDROID
                    handlers.AddHandler<
                        Microsoft.Maui.Controls.WebView,
                        BrowserWebViewHandler>();
                #endif
                });
            // Views
            builder.Services.AddSingleton<WebviewPage>();
            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<TabsPage>();

            // ViewModels
            builder.Services.AddSingleton<WebviewViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<TabsViewModel>();

            // Services
            builder.Services.AddSingleton<ITabService, TabService>();
            builder.Services.AddSingleton<SettingsService>();

#if ANDROID // Android specific stuff here
            builder.Services.AddHttpClient<IDownloadService, DownloadService>();
            builder.Services.AddSingleton<ILongPressService, LongPressService>();
            builder.Services.AddSingleton<AdBlockClient>();
            builder.Services.AddSingleton<AdBlockEngine>();
            builder.Services.AddSingleton<PopupBlockClient>();
#endif

            // debugging block
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
