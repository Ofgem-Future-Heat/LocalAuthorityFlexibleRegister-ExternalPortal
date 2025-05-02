using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_ExternalPortal.Enums;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    /// <summary>
    /// Single declaration journey definition used to store the data in TempData 
    /// </summary>
    /// <code>
    ///
    /// TempData.Put(TempDataKeys.SingleDeclarationData, data);
    /// var data = TempData.Get &lt; SingleDeclaration &gt; (TempDataKeys.SingleDeclarationData);
    ///
    /// </code>
    public class SingleDeclarationJourney
    {
        public AvailableLocalAuthority? BaseLocalAuthority { get; set; }

        public List<AvailableLocalAuthority> LocalAuthorityList { get; set; } = [];


        public string OnsCode { get; set; } = string.Empty;
        public string OnsCodeDetails { get; set; } = string.Empty;

        public Guid? StatementOfIntentId { get; set; }
        public string StatementOfIntentDetails { get; set; } = string.Empty;

        public string UrnNumber { get; set; } = string.Empty;


        public string HouseholdOnsCode { get; set; } = string.Empty;

        public string HouseholdOnsCodeName { get; set; } = string.Empty;

        public string HouseholdOnsCodeDetails { get; set; } = string.Empty;


        public ReferralRoutes Route { get; set; } = ReferralRoutes.Unknown;
        public string RouteDetail { get; set; } = string.Empty;


        public string Route2ProxyHouseholdReferred { get; set; } = string.Empty;


        public string Route2ProxyHouseholdReferredAdditional { get; set; } = string.Empty;


        public string Route4ApplicationNumber { get; set; } = string.Empty;

        public RouteProxies SelectedProxy { get; set; } = RouteProxies.Unknown;


        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;

        public string FullAddress => string.Join(", ", new[] { Address1, Address2, Postcode }.Where(f => !string.IsNullOrEmpty(f)));

        public string? DateOfHouseholderEligibility { get; set; }
        
        public bool? WasLocalAuthorityConsultedPriorToInstallation { get; set; }
        public ForSchemeEnum ForScheme { get; set; }
        public DateTime? DateAdded { get; set; }

        public class AvailableLocalAuthority
        {
            public Guid LocalAuthorityId { get; set; }

            public string OnsCode { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public bool IsBase { get; set; }

            public List<AvailableSoi> AvailableSoiList { get; set; } = [];
        }

        public class AvailableSoi
        {
            public Guid StatementOfIntentId { get; set; }

            public string VersionNumber { get; set; } = string.Empty;

            public SoiStatusV2 SoiStatus { get; set; }

            public DateTime? DateAdded { get; set; }
            public ForSchemeEnum ForScheme { get; set; }
        }
    }
}
