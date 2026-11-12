#if ANDROID
using Android.Views;
using Android.Widget;
using GoonBrowserAndroid.Enums;
using GoonBrowserAndroid.Models;
using GoonBrowserAndroid.Services.Download;
using WebView = Android.Webkit.WebView;
// make sure WebView isn't ambiguous becuz 2 references use same name??? WHY MICROSLOP FUCK U
// so just equal the webview to that full reference, or else C# cries like fucking bitch ce pula mea(wtf)

namespace GoonBrowserAndroid.Services.LongPress
{
    public class LongPressService : ILongPressService
    {
        /// <summary>
        /// Converts Android HitTestResult into something the app understands.
        /// </summary>
        public void ShowMenu(WebView browserView, HitResultModel hit, IDownloadService downloadService)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Long press: {hit.MediaType} - {hit.Url}");

            var popup = new PopupMenu(browserView.Context, browserView);

            // Add menu items depending on the hit type
            switch (hit.MediaType)
            {
                case MediaTypeEnum.Image:
                    popup.Menu.Add("Open Image");
                    popup.Menu.Add("Download Image");
                    popup.Menu.Add("Copy Image Link");
                    break;

                case MediaTypeEnum.File:
                    popup.Menu.Add("Open Link");
                    popup.Menu.Add("Download");
                    popup.Menu.Add("Copy Link");
                    break;

                default:
                    popup.Menu.Add("Copy");
                    break;
            }

            popup.MenuItemClick += async (_, args) =>
            {
                var item = args.Item.TitleFormatted?.ToString();

                switch (item)
                {
                    case "Download":
                    case "Download Image":
                        if (!string.IsNullOrWhiteSpace(hit.Url))
                            await downloadService.DownloadAsync(hit.Url);
                        break;

                    case "Copy Link":
                    case "Copy Image Link":
                        if (!string.IsNullOrWhiteSpace(hit.Url))
                        {
                            await Clipboard.Default.SetTextAsync(hit.Url);
                        }
                        break;

                    case "Open Image":
                    case "Open Link":
                        if (!string.IsNullOrWhiteSpace(hit.Url))
                        {
                            browserView.LoadUrl(hit.Url);
                        }
                        break;
                }
            };

            popup.Show();
            System.Diagnostics.Debug.WriteLine(
                $"popup show is called !!!!");
        }

        public static HitResultModel Parse(WebView.HitTestResult hit)
        {
            return ((HitTestTypeEnum)hit.Type) switch
            {

                HitTestTypeEnum.Image => new()
                {
                    MediaType = MediaTypeEnum.Image,
                    Url = hit.Extra
                },

                HitTestTypeEnum.SrcImageAnchor => new()
                {
                    MediaType = MediaTypeEnum.Image,
                    Url = hit.Extra
                },

                HitTestTypeEnum.SrcAnchor => new()
                {
                    MediaType = MediaTypeEnum.File,
                    Url = hit.Extra
                },

                _ => new()
                {
                    MediaType = MediaTypeEnum.Unknown,
                    Url = null
                }
            };
        }
    }
}
#endif