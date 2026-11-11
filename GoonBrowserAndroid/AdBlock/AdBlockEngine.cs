using System.Diagnostics;
using System.Text;

namespace GoonBrowserAndroid.AdBlock
{
    public class AdBlockEngine
    {
        private readonly List<string> _blockedDomains = new();
        private readonly List<string> _cssSelectors = new();
        private readonly List<string> _popupRules = new();
        public IReadOnlyList<string> CssSelectors => _cssSelectors;
        public async Task UpdateListsAsync()
        {
            using var client = new HttpClient();

            var easyList = await client.GetStringAsync(
                "https://easylist.to/easylist/easylist.txt"
            );

            foreach (var line in easyList.Split('\n'))
            {
                var rule = line.Trim();

                if (rule.StartsWith("||"))
                {
                    var domain = rule.Substring(2) .Split('^')[0];

                    _blockedDomains.Add(domain);
                }

                if (rule.Contains("##"))
                {
                    var selector = rule.Split("##")[1];

                    if (!selector.StartsWith("+js"))
                    {
                        _cssSelectors.Add(selector);
                    }
                }

            }
        }

        public bool ShouldBlock(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            var host = uri.Host;

            var blocked = _blockedDomains.Any(domain => url.Contains(domain));

            if (blocked)
            {
                Debug.WriteLine($"BLOCKED: {url}");
            }

            return _blockedDomains.Any(domain =>
                host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase));
        }

        // worst function in whole fucking app TRUST
        public string GetCosmeticScript()
        {
            var stringBuilder = new StringBuilder();

            System.Diagnostics.Debug.WriteLine("GetCosmeticScript: STARTED!");
            stringBuilder.AppendLine(@"
(function() {

    // kill window popups
    window.open = function() {
        return null;
    };

    function clean() {
");

            foreach (var selector in _cssSelectors)
            {
                stringBuilder.AppendLine($@"
        document.querySelectorAll('{selector}')
            .forEach(x => x.remove());
        ");
            }

            stringBuilder.AppendLine(@"

        // remove common interstitial patterns
        document.querySelectorAll(
            '[class*=""overlay""],
             [class*=""modal""],
             [class*=""interstitial""],
             [id*=""overlay""],
             [id*=""modal""]'
        ).forEach(x => {
            x.remove();
        });

    }

    clean();

    new MutationObserver(() => {
        clean();
    }).observe(
        document.documentElement,
        {
            childList:true,
            subtree:true
        }
    );

})();
");

            return stringBuilder.ToString();
        }


        public bool ShouldBlockPopup(string url)
        {
            foreach (var rule in _popupRules)
            {
                if (!rule.StartsWith("||"))
                {
                    continue;
                }

                var domain = rule.Substring(2).Split('^')[0];

                if (url.Contains(domain))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
