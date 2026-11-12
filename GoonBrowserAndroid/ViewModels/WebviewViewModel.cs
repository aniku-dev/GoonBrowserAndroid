using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoonBrowserAndroid.Models;
using GoonBrowserAndroid.Services.Download;
using GoonBrowserAndroid.Services.LongPress;
using GoonBrowserAndroid.Services.Tab;
using GoonBrowserAndroid.Views;
using System.Windows.Input;

namespace GoonBrowserAndroid.ViewModels
{
    public partial class WebviewViewModel : ObservableObject
    {
        // services
        private readonly IDownloadService _downloadService;
        private readonly ILongPressService _longPressService;
        private readonly ITabService _tabService;
        public ITabService TabService => _tabService;

        [ObservableProperty] string urlInput;

        // buttons
        public ICommand GoBackCmd { get; }
        public ICommand GoForwardCmd { get; }
        public ICommand RefreshCmd { get;  }
        public ICommand GoToTabsCmd { get; }

        public WebviewViewModel(IDownloadService downloadService, ILongPressService longPressService, ITabService tabsService)
        {
            _downloadService = downloadService;
            _longPressService = longPressService;
            _tabService = tabsService;

            GoBackCmd = new Command<object>(GoBack);
            GoForwardCmd = new Command<object>(GoForward);
            RefreshCmd = new Command<object>(RefreshPage);
            GoToTabsCmd = new Command(async () => await GoToTabs());
        }

        [RelayCommand]
        public void GoBack(object WebView)
        {
            if (WebView is WebView browserView)
            {
                browserView.GoBack();
            }
        }

        [RelayCommand]
        public void GoForward(object WebView)
        {
            if (WebView is WebView browserView)
            {
                browserView.GoForward();
            }
        }

        [RelayCommand]
        public void RefreshPage(object WebView)
        {
            if (WebView is WebView browserView)
            {
                browserView.Reload();
            }
        }

        async Task GoToTabs()
        {
            await Shell.Current.GoToAsync(nameof(TabsPage));
            // just go to fucking tab
        }

        [RelayCommand]
        public async Task SearchAsync(Microsoft.Maui.Controls.WebView browserView)
        {
            if (browserView == null)
            {
                return;
            }

            var url = UrlInput?.Trim(); // .Trim trims any remaining newline/chars

            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            /* if a url address doesn't start with either http:// or https://, it basically apends
             * said prefix to the url automatically without the user having to put it in.
             * this is convenient. also, HTTPS superiority!
             */

            if (!url.StartsWith("http://") && !url.StartsWith("https://")) 
            {
                url = "https://" + url;
            }

            browserView.Source = url;

            await Task.CompletedTask;

            UrlInput = url;
        }

        [RelayCommand]
        public async Task DownloadMedia(string url)
        {
            await _downloadService.DownloadAsync(url);
        }
    }
}
