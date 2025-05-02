using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{

    public abstract class SoiPage() : PageModel
    {
        internal const string SoiDataKey = "StatementOfIntentData";
        internal const string LocalAuthorityAvailableKey = "LocalAuthoritiesAvailable";
        internal const string LocalAuthoritySelectedKey = "LocalAuthoritiesSelected";
    }
}
