using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ofgem_Web_LAF_ExternalPortal.Pages
{
    [AllowAnonymous]
    public class CookiesModel : PageModel
    {
        private const string No = "No";
        private const string Yes = "Yes";
        private const string Unspecified = "Unspecified";
        private const string AnalyticsConsentCookie = "cookies.analytics";
        private const string EssentialCookies = "cookies.essential";

        [BindProperty] public string AnalyticsCookie { get; set; } = Unspecified;

        public string[] AnalyticsCookies // NOSONAR
        {
            get
            {
                return new[]
                {
                    Yes, No
                };
            }
        }

        public void OnGet()
        {
            var analyticsCookies = Request.Cookies[AnalyticsConsentCookie];

            if (analyticsCookies == null)
            {
                return;
            }

            AnalyticsCookie = analyticsCookies.Equals(true.ToString(), StringComparison.OrdinalIgnoreCase) ? Yes : No;
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            var options = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) };
            Response.Cookies.Append(AnalyticsConsentCookie, AnalyticsCookie.Equals(No, StringComparison.OrdinalIgnoreCase) ? false.ToString().ToLower() : true.ToString().ToLower(), options);
            Response.Cookies.Append(EssentialCookies, true.ToString().ToLower(), options);
        }
    }
}
