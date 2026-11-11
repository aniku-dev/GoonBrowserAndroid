#if ANDROID
using Android.Webkit;
using Android.OS;

namespace GoonBrowserAndroid.AdBlock
{
    public class PopupBlockClient : WebChromeClient
    {
        private readonly AdBlockEngine _engine;

        public PopupBlockClient(AdBlockEngine engine)
        {
            _engine = engine;
        }

        public override bool OnCreateWindow(
        Android.Webkit.WebView? view,
        bool isDialog,
        bool isUserGesture,
        Message? resultMsg)
        {
            var currentUrl = view?.Url;

            if (currentUrl != null && _engine.ShouldBlockPopup(currentUrl))
            {
                // Popup blocked
                return false;
            }

            return false; // or implement opening the popup yourself
        }
    }
}
#endif