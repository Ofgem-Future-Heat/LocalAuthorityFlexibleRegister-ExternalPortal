using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class DeclarationErrorFields
    {
        public DeclarationErrorFields() { }

        public DeclarationErrorFields(ICollection<DeclarationError>? declarationDeclarationErrors)
        {
            CanBeFixed = true;

            if (declarationDeclarationErrors is null)
            {
                return;
            }
            if (declarationDeclarationErrors.Count == 0)
            {
                return;
            }

            foreach (var declarationError in declarationDeclarationErrors)
            {
                if (declarationError.FailingRuleDetails is null) continue;

                foreach (var errorField in declarationError.FailingRuleDetails.Fields)
                {
                    switch (errorField)
                    {
                        case "Address_Line_1":
                            AddressOfHousehold = true;
                            AddressLine1ErrorId = $"id_{declarationError.DeclarationErrorId}";
                            AddressLine1ErrorMessage = declarationError.Message;
                            break;

                        case "Address_Line_2":
                            AddressOfHousehold = true;
                            AddressLine2ErrorId = $"id_{declarationError.DeclarationErrorId}";
                            AddressLine2ErrorMessage = declarationError.Message;
                            break;

                        case "Post_Code":
                            AddressOfHousehold = true;
                            PostCodeErrorId = $"id_{declarationError.DeclarationErrorId}";
                            PostCodeErrorMessage = declarationError.Message;
                            break;

                        case "LA_Was_Consulted_Prior_To_Installation_Completion":
                            LAWasConsultedPriorToInstallationCompletion = true;
                            LAWasConsultedPriorToInstallationCompletionErrorId =
                                $"id_{declarationError.DeclarationErrorId}";
                            LAWasConsultedPriorToInstallationCompletionErrorMessage = declarationError.Message;
                            break;

                        case "Date_Of_Householder_Eligibility":
                            DateOfHouseholderEligibility = true;
                            DateOfHouseholderEligibilityErrorId = $"id_{declarationError.DeclarationErrorId}";
                            DateOfHouseholderEligibilityErrorMessage = declarationError.Message;
                            break;

                        case "LA_Declaration_Unique_Reference_Number":
                            Urn = true;
                            UrnErrorId = $"id_{declarationError.DeclarationErrorId}";
                            UrnErrorMessage = declarationError.Message;
                            break;

                        case "LA_Area_Code":
                            LAAreaCode = true;
                            LAAreaCodeErrorId = $"id_{declarationError.DeclarationErrorId}";
                            LAAreaCodeErrorMessage = declarationError.Message;
                            break;

                        case "ECO4_or_Great_British_Insulation_Scheme_Flex_Referral_Route":
                            ECO4orGreatBritishInsulationSchemeFlexReferralRoute = true;
                            ECO4orGreatBritishInsulationSchemeFlexReferralRouteErrorId =
                                $"id_{declarationError.DeclarationErrorId}";
                            ECO4orGreatBritishInsulationSchemeFlexReferralRouteErrorMessage =
                                declarationError.Message;
                            break;

                        case "Route_2_Proxies":
                            Route2Proxies = true;
                            Route2ProxiesErrorId = $"id_{declarationError.DeclarationErrorId}";
                            Route2ProxiesErrorMessage = declarationError.Message;
                            break;

                        case "Additional_Route_2_Proxies":
                            AdditionalRoute2Proxies = true;
                            AdditionalRoute2ProxiesErrorId = $"id_{declarationError.DeclarationErrorId}";
                            AdditionalRoute2ProxiesErrorMessage = declarationError.Message;
                            break;

                        case "Route_4_Application_Number":
                            Route4ApplicationNumber = true;
                            Route4ApplicationNumberErrorId = $"id_{declarationError.DeclarationErrorId}";
                            Route4ApplicationNumberErrorMessage = declarationError.Message;
                            break;

                        case "Referral_Made_Outside_Of_LAs_Remit":
                            ReferralMadeOutsideOfLAsRemit = true;
                            ReferralMadeOutsideOfLAsRemitErrorId = $"id_{declarationError.DeclarationErrorId}";
                            ReferralMadeOutsideOfLAsRemitErrorMessage = declarationError.Message;
                            break;

                        case "Statement_Of_Intent_Link":
                            StatementOfIntentLink = true;
                            StatementOfIntentLinkErrorId = $"id_{declarationError.DeclarationErrorId}";
                            StatementOfIntentLinkErrorMessage = declarationError.Message;
                            break;

                        case "Statement_Of_Intent_Published_For":
                            StatementOfIntentPublishedFor = true;
                            StatementOfIntentPublishedForErrorId = $"id_{declarationError.DeclarationErrorId}";
                            StatementOfIntentPublishedForErrorMessage = declarationError.Message;
                            break;

                        case "Date_Of_Statement_Of_Intent_Publication":
                            DateOfStatementOfIntentPublication = true;
                            DateOfStatementOfIntentPublicationErrorId = $"id_{declarationError.DeclarationErrorId}";
                            DateOfStatementOfIntentPublicationErrorMessage = declarationError.Message;
                            break;

                        case "IsRoute1Accurate":
                        case "IsRoute2Accurate":
                        case "IsRoute3Accurate":
                        case "IsRoute4Accurate":
                            Route = true;
                            RouteErrorId = $"id_{declarationError.DeclarationErrorId}";
                            RouteErrorMessage = declarationError.Message;
                            break;
                    }
                }
            }
        }

        public bool Urn { get; set; }
        public bool LocalAuthority { get; set; }
        public bool Route { get; set; }
        public bool ReferralMadeOutsideOfLAsRemit { get; set; }
        public bool ECO4orGreatBritishInsulationSchemeFlexReferralRoute { get; set; }
        public bool Route2Proxies { get; set; }
        public bool AdditionalRoute2Proxies { get; set; }
        public bool Route4ApplicationNumber { get; set; }
        public bool AddressLine1 { get; set; }
        public bool AddressLine2 { get; set; }
        public bool PostCode { get; set; }
        public bool AddressOfHousehold { get; set; }

        public bool LAAreaCode { get; set; }
        public bool DateOfHouseholderEligibility { get; set; }
        public bool LAWasConsultedPriorToInstallationCompletion { get; set; }
        public bool DateOfStatementOfIntentPublication { get; set; }
        public bool StatementOfIntentLink { get; set; }
        public bool StatementOfIntentPublishedFor { get; set; }


        public string? UrnErrorId { get; set; }
        public string? LocalAuthorityErrorId { get; set; }
        public string? RouteErrorId { get; set; }
        public string? ReferralMadeOutsideOfLAsRemitErrorId { get; set; }
        public string? ECO4orGreatBritishInsulationSchemeFlexReferralRouteErrorId { get; set; }
        public string? Route2ProxiesErrorId { get; set; }
        public string? AdditionalRoute2ProxiesErrorId { get; set; }
        public string? Route4ApplicationNumberErrorId { get; set; }
        public string? AddressLine1ErrorId { get; set; }
        public string? AddressLine2ErrorId { get; set; }
        public string? PostCodeErrorId { get; set; }
        public string? LAAreaCodeErrorId { get; set; }
        public string? DateOfHouseholderEligibilityErrorId { get; set; }
        public string? LAWasConsultedPriorToInstallationCompletionErrorId { get; set; }
        public string? DateOfStatementOfIntentPublicationErrorId { get; set; }
        public string? StatementOfIntentLinkErrorId { get; set; }
        public string? StatementOfIntentPublishedForErrorId { get; set; }



        public string? UrnErrorMessage { get; set; }
        public string? LocalAuthorityErrorMessage { get; set; }
        public string? RouteErrorMessage { get; set; }
        public string? ReferralMadeOutsideOfLAsRemitErrorMessage { get; set; }
        public string? ECO4orGreatBritishInsulationSchemeFlexReferralRouteErrorMessage { get; set; }
        public string? Route2ProxiesErrorMessage { get; set; }
        public string? AdditionalRoute2ProxiesErrorMessage { get; set; }
        public string? Route4ApplicationNumberErrorMessage { get; set; }
        public string? AddressLine1ErrorMessage { get; set; }
        public string? AddressLine2ErrorMessage { get; set; }
        public string? PostCodeErrorMessage { get; set; }

        public string? LAAreaCodeErrorMessage { get; set; }
        public string? DateOfHouseholderEligibilityErrorMessage { get; set; }
        public string? LAWasConsultedPriorToInstallationCompletionErrorMessage { get; set; }
        public string? DateOfStatementOfIntentPublicationErrorMessage { get; set; }
        public string? StatementOfIntentLinkErrorMessage { get; set; }
        public string? StatementOfIntentPublishedForErrorMessage { get; set; }

        /// <summary>
        /// Identifies if the errors can be resolved in the UI
        /// </summary>
        public bool CanBeFixed { get; set; }
    }
}
