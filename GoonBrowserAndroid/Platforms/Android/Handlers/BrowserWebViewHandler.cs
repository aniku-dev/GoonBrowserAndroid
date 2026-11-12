#if ANDROID
using Android.Views;
using Android.Webkit;
using GoonBrowserAndroid.Enums;
using GoonBrowserAndroid.Models;
using GoonBrowserAndroid.Services.Download;
using GoonBrowserAndroid.Services.LongPress;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;

namespace GoonBrowserAndroid.Platforms.Handlers;

public class BrowserWebViewHandler : WebViewHandler
{
    private LongClickListener? _longClickListener;

    protected override void ConnectHandler(Android.Webkit.WebView platformView)
    {
        base.ConnectHandler(platformView);

        var services = MauiContext!.Services;

        var longPressService =
            services.GetRequiredService<ILongPressService>();

        var downloadService =
            services.GetRequiredService<IDownloadService>();

        _longClickListener = new LongClickListener(
            platformView,
            longPressService,
            downloadService
        );

        platformView.SetOnLongClickListener(_longClickListener);
    }

    protected override void DisconnectHandler(Android.Webkit.WebView platformView)
    {
        platformView.SetOnLongClickListener(null);
        _longClickListener = null;

        base.DisconnectHandler(platformView);
    }
}

public class LongClickListener : Java.Lang.Object,
    Android.Views.View.IOnLongClickListener
{
    private readonly Android.Webkit.WebView _browserView;
    private readonly ILongPressService _longPressService;
    private readonly IDownloadService _downloadService;

    public LongClickListener(
        Android.Webkit.WebView browserView,
        ILongPressService longPressService,
        IDownloadService downloadService)
    {
        _browserView = browserView;
        _longPressService = longPressService;
        _downloadService = downloadService;
    }

    public bool OnLongClick(Android.Views.View? view)
    {
        var nativeHit = _browserView.GetHitTestResult();

        var hit = LongPressService.Parse(nativeHit);

        if (hit.MediaType == MediaTypeEnum.Unknown ||
            string.IsNullOrWhiteSpace(hit.Url))
        {
            return false;
        }

        _longPressService.ShowMenu(
            _browserView,
            hit,
            _downloadService
        );

        return true;
    }
}
#endif