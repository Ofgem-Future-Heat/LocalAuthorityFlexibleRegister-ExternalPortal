using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class Declaration
    {
        public Guid DeclarationId { get; init; }
        public string? Urn { get; init; }
        public string? LocalAuthority { get; init; }
        public DateTime? CreatedDate { get; init; }
        public string? Route { get; init; }
        public string? ReferralMadeOutsideOfLAsRemit { get; set; }

        // ReSharper disable once InconsistentNaming
        public string? ECO4orGreatBritishInsulationSchemeFlexReferralRoute { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public string? Route2Proxies { get; set; }

        public string? AdditionalRoute2Proxies { get; set; }
        public string? Route4ApplicationNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PostCode { get; set; }

        // ReSharper disable once InconsistentNaming
        public string? LAAreaCode { get; set; }

        public string? DateOfHouseholderEligibility { get; set; }

        // ReSharper disable once InconsistentNaming
        public string? LAWasConsultedPriorToInstallationCompletion { get; set; }

        public string? DateOfStatementOfIntentPublication { get; set; }
        public string? StatementOfIntentLink { get; set; }
        public string? StatementOfIntentPublishedFor { get; set; }
        public Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus Status { get; init; }
        public Ofgem.LAF.SharedLibrary.Enums.RecordStatus RecordStatus { get; set; }
        public Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2 SoiStatus { get; set; }
        public Ofgem.LAF.SharedLibrary.Enums.SoiCategory SoiCategory { get; set; }

        public List<Ofgem.LAF.SharedLibrary.Models.DeclarationError>? DeclarationErrors { get; set; }

        public List<DeclarationNote>? DeclarationNotes { get; set; }

        public bool Selected { get; set; }

        // defaulted to false, the information will be updated by the page to identify if the record is SafeToShow
        public bool SafeToShow { get; set; }
        


        /// <summary>
        /// Display ONLY version of the date
        /// </summary>
        public string DisplayCreatedDate => CreatedDate.HasValue ? (LafLocalTimezone.ToLocalTime((DateTime)CreatedDate).ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT)) : "";


        /// <summary>
        /// Display ONLY version of the date
        /// </summary>
        public string DateOfHouseholderEligibilityForView => string.IsNullOrEmpty(DateOfHouseholderEligibility)
            ? ""
            : DateOfHouseholderEligibility;

        /// <summary>
        /// Display ONLY version of the date
        /// </summary>
        public string DateOfStatementOfIntentPublicationForView => string.IsNullOrEmpty(DateOfStatementOfIntentPublication)
            ? ""
            : DateOfStatementOfIntentPublication;


        /// <summary>
        /// Display ONLY version of the address
        /// </summary>
        public string FullAddress => string.Join(", ", new[] { AddressLine1, AddressLine2, PostCode }.Where(s => !string.IsNullOrEmpty(s)));




        public static Ofgem.LAF.SharedLibrary.Models.RawDeclaration MapToRawDeclaration(Declaration declaration)
        {
            var rawDeclaration = new Ofgem.LAF.SharedLibrary.Models.RawDeclaration
            {
                Additional_Route_2_Proxies = declaration.AdditionalRoute2Proxies,
                Address_Line_1 = declaration.AddressLine1,
                Address_Line_2 = declaration.AddressLine2,
                Date_Of_Householder_Eligibility = declaration.DateOfHouseholderEligibility?.Replace(" ", "/"),
                Date_Of_Statement_Of_Intent_Publication = declaration.DateOfStatementOfIntentPublication?.Replace(" ", "/"),
                ECO4_or_Great_British_Insulation_Scheme_Flex_Referral_Route = declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute,
                LA_Area_Code = declaration.LAAreaCode,
                LA_Declaration_Unique_Reference_Number = declaration.Urn,
                LA_Was_Consulted_Prior_To_Installation_Completion = declaration.LAWasConsultedPriorToInstallationCompletion,
                Post_Code = declaration.PostCode,
                Referral_Made_Outside_Of_LAs_Remit = declaration.ReferralMadeOutsideOfLAsRemit,
                Route_2_Proxies = declaration.Route2Proxies,
                Route_4_Application_Number = declaration.Route4ApplicationNumber,
                RowNumber = 0,
                Statement_Of_Intent_Link = declaration.StatementOfIntentLink,
                Statement_Of_Intent_Published_For = declaration.StatementOfIntentPublishedFor
            };

            return rawDeclaration;
        }


        public static List<Ofgem.LAF.SharedLibrary.Models.DeclarationError> MapToDeclarationError(Ofgem.LAF.SharedLibrary.Models.RunRulesResponse result)
        {
            var declarationErrors = new List<Ofgem.LAF.SharedLibrary.Models.DeclarationError>();

            if (result.Errors is null) return declarationErrors;
            
            foreach (var resultError in result.Errors)
            {
                // these errors are to be ignored these are errors that CANNOT be corrected via the inline edit
                if (resultError.Message
                    is "The Unique Reference Number is a duplicate."
                    or "'Address_Line_1' is a required field. Please confirm the Address Line 1"
                    or "'Address_Line_2' is a required field. Please confirm the Address Line 2"
                    or "'Post_Code' is a required field. Please confirm the Post Code")
                    continue;

                var newError = new Ofgem.LAF.SharedLibrary.Models.DeclarationError
                {
                    FailingRuleDetails = resultError.FailingRuleDetails,
                    Message = resultError.Message,
                    Rule = resultError.Rule
                };

                declarationErrors.Add(newError);
            }

            return declarationErrors;
        }

        public static Declaration MapFromDtoDeclaration(Ofgem.LAF.SharedLibrary.Models.Declaration declaration)
        {
            var rawDeclaration = new Declaration
            {
                CreatedDate = declaration.CreatedDate,
                AdditionalRoute2Proxies = declaration.AdditionalRoute2Proxies,
                AddressLine1 = declaration.AddressLine1,
                AddressLine2 = declaration.AddressLine2,
                DateOfHouseholderEligibility = declaration.DateOfHouseholderEligibility,
                DateOfStatementOfIntentPublication = declaration.DateOfStatementOfIntentPublication,
                DeclarationId = declaration.DeclarationId,
                ECO4orGreatBritishInsulationSchemeFlexReferralRoute = declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute,
                LAAreaCode = declaration.LAAreaCode,
                Route = declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute,
                Urn = declaration.Urn,
                LAWasConsultedPriorToInstallationCompletion = declaration.LAWasConsultedPriorToInstallationCompletion,
                PostCode = declaration.PostCode,
                ReferralMadeOutsideOfLAsRemit = declaration.ReferralMadeOutsideOfLAsRemit,
                Route2Proxies = declaration.Route2Proxies,
                Route4ApplicationNumber = declaration.Route4ApplicationNumber,
                StatementOfIntentLink = declaration.StatementOfIntentLink,
                StatementOfIntentPublishedFor = declaration.StatementOfIntentPublishedFor,
                Status = declaration.Status,
                SafeToShow = false
            };

            return rawDeclaration;
        }
    }
}
