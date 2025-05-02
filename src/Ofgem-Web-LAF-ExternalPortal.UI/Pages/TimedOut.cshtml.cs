using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ofgem_Web_LAF_ExternalPortal.Pages
{
    public class TimedOutModel : PageModel
    {
        public void OnGet()
        {
            if (HttpContext is null) return;

            // Invalidate the old session. A new session will be started.
            HttpContext.Session.Clear();

            // Clear the session ID from the client side. 
            var allCookies = HttpContext.Request.Cookies.Keys;

            if (allCookies is null) return;

            foreach (var domainCookie in allCookies)
            {
                HttpContext.Response.Cookies.Delete(domainCookie);
            }
        }
    }
}
