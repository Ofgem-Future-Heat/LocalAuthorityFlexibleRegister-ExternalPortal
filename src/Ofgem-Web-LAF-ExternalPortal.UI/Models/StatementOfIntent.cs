using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class StatementOfIntent
    {
        public Guid StatementOfIntentId { get; set; }

        public string? LocalAuthority { get; set; }

        public string? VersionNumber { get; set; }

        public string? SoiLink { get; set; }

        public ForSchemeEnum ForScheme { get; set; }

        public string? SoiTruncatedLink
        {
            get
            {

                var displaySoiLink = "Link not available";

                if (SoiLink != null)
                {
                    var segments = SoiLink.Split('/');
                    if (segments.Length > 2)
                    {
                        displaySoiLink = "..." + "/" + segments[^2] + "/" + segments[^1];
                    }
                }

                return displaySoiLink;
            }
        }

        public DateTime? DateAdded { get; set; }

        public SoiStatusV2 SoiStatus { get; set; }

        public List<DesignatedLa>? DesignatedLAs { get; set; } = new();

        public class DesignatedLa
        {
            public Guid? LocalAuthorityId { get; init; }

            public string? LocalAuthorityName { get; init; }

            public string? OnsCode { get; init; }
        }

        /// <summary>
        /// Display ONLY version of the date
        /// </summary>
        public string DisplayCreatedDate => DateAdded.HasValue ? (LafLocalTimezone.ToLocalTime((DateTime)DateAdded).ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT)) : "";


        public static StatementOfIntent MapFromDtoStatementOfIntent(Ofgem.LAF.SharedLibrary.Models.StatementOfIntent statementOfIntent)
        {
            

            var rawStatementODeclaration = new StatementOfIntent
            {
                StatementOfIntentId = statementOfIntent.StatementOfIntentId,
                LocalAuthority = statementOfIntent.LocalAuthority?.Name,
                VersionNumber = statementOfIntent.VersionNumber,
                SoiLink = statementOfIntent.StatementOfIntentLink,
                DateAdded = statementOfIntent.PublishedDate,
                SoiStatus = statementOfIntent.Status,
                ForScheme = statementOfIntent.ForScheme,
                DesignatedLAs = statementOfIntent.DesignatedLas?.Select(item => new DesignatedLa
                {
                    LocalAuthorityId = item?.LocalAuthority?.LocalAuthorityId,
                    LocalAuthorityName = item?.LocalAuthority?.Name,
                    OnsCode = item?.LocalAuthority?.OnsCode
                }).ToList() ?? new List<DesignatedLa>()
            };

            return rawStatementODeclaration;
        }
    }
}

