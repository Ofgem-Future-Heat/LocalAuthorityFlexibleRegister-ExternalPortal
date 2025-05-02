using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_ExternalPortal.Enums;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions;

public static class SingleDeclarationJourneyExtensions
{
    public static Declaration ToDeclaration(this SingleDeclarationJourney singleDeclaration)
    {
        var routeName = singleDeclaration.Route switch
        {
            ReferralRoutes.HouseHoldIncome => Constants.Route1Text,
            ReferralRoutes.ProxyTargeting => Constants.Route2Text,
            ReferralRoutes.NhsReferrals => Constants.Route3Text,
            ReferralRoutes.BespokeTargeting => Constants.Route4Text,
            _ => throw new ArgumentException("Invalid Referral Route")
        };

        var statementOfIntentPublishedFor = singleDeclaration.ForScheme switch
        {
            ForSchemeEnum.ECO4 => StatementOfIntentPublishedForDescriptions.StatementOfIntentForEco4Flex,
            ForSchemeEnum.ECO4andGBIS => StatementOfIntentPublishedForDescriptions.StatementOfIntentForEco4AndGbiSchemeFlex,
            _ => "No Scheme"
        };

        if (!singleDeclaration.DateAdded.HasValue) throw new ArgumentException("Missing DateAdded property");


        return new Declaration
        {
            AddressLine1 = singleDeclaration.Address1,
            AddressLine2 = singleDeclaration.Address2,
            CreatedDate = DateTime.Now,
            DateOfHouseholderEligibility = singleDeclaration.DateOfHouseholderEligibility,
            DateOfStatementOfIntentPublication = $"{singleDeclaration.DateAdded:dd/MM/yyyy}",
            ECO4orGreatBritishInsulationSchemeFlexReferralRoute = routeName,
            LAWasConsultedPriorToInstallationCompletion
                = singleDeclaration.WasLocalAuthorityConsultedPriorToInstallation != null &&
                  singleDeclaration.WasLocalAuthorityConsultedPriorToInstallation.Value
                    ? "Yes"
                    : "No",
            LAAreaCode = singleDeclaration.HouseholdOnsCode,
            LocalAuthority = singleDeclaration.HouseholdOnsCodeName,
            PostCode = singleDeclaration.Postcode,
            ReferralMadeOutsideOfLAsRemit
                = singleDeclaration.OnsCode != singleDeclaration.HouseholdOnsCode
                    ? "Yes"
                    : "No",
            Route = singleDeclaration.RouteDetail,
            Route4ApplicationNumber = singleDeclaration.Route4ApplicationNumber,
            StatementOfIntentLink = singleDeclaration.StatementOfIntentDetails,
            StatementOfIntentPublishedFor = statementOfIntentPublishedFor,
            Urn = singleDeclaration.UrnNumber
        };
    }
}