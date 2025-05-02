using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Ofgem.LAF.SharedLibrary.Models;
using System.Text;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public interface IBasicValidationService
    {
        Tuple<bool, List<string>, string> Validate(byte[] fileBytes);
    }

    [ExcludeFromCodeCoverage]
    public class BasicValidationService : IBasicValidationService
    {

        private readonly ILogger<BasicValidationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public BasicValidationService(ILogger<BasicValidationService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Tuple<bool, List<string>, string> Validate(byte[] fileBytes)
        {
            try
            {
                // convert csv to dtos
                List<RawDeclaration> rawDeclarations = Ofgem.LAF.SharedLibrary.Extensions.Declaration.ConvertToObject(fileBytes);

                // 1. blank or N/A urn - reject file
                if (BlankUrn_Rule(rawDeclarations).Any() || NotApplicableUrn_Rule(rawDeclarations).Any())
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [Constants.LA_DURN_001_DECLARATION_UNIQUE_REFERENCE_NUMBER_IS_BLANK],
                        string.Empty
                    );
                }

                // 2. Single Submitting Local Authorities Rule - reject file
                if (!Single_Submitting_Local_Authorities_Rule(rawDeclarations))
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [Constants.LA_DURN_004_SINGLE_DECLARATION_UNIQUE_REFERENCE_NUMBER],
                        string.Empty
                    );
                }

                // 3. duplicate URN - reject file
                var duplicateUrn = DuplicateUrn_Rule(rawDeclarations).ToList();

                if (duplicateUrn.Count > 0)
                {
                    return new Tuple<bool, List<string>, string>(false, duplicateUrn!, string.Empty);
                }

                // 4. invalid format urn - reject file
                if (InvalidUrnFormat_Rule(rawDeclarations) > 0)
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            "Incorrect Unique Reference Number format in column LA_Declaration_Unique_Reference_Number, please use the accepted format ANNNNNNNN-NNNNN"
                        ],
                        string.Empty
                    );
                }

                // 5. ONSCode is not known within the DB
                var urnExists = URN_Is_In_The_DB_Rule(rawDeclarations[0].LA_Declaration_Unique_Reference_Number);

                if (!urnExists.Result)
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            "Please note this file cannot be uploaded because the submitted Local Authority for this declaration does not exist in the Profile database. Please add this Local Authority in the Profile section and re-upload the declaration."
                        ],
                        string.Empty
                    );
                }

                // 6. duplicate Address - reject file
                var duplicateAddresses = DuplicateAddresses_Rule(rawDeclarations);

                if (duplicateAddresses.Count > 0)
                {
                    return new Tuple<bool, List<string>, string>(false, duplicateAddresses, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return new Tuple<bool, List<string>, string>(false, [], ex.Message);
            }

            return new Tuple<bool, List<string>, string>(true, [], string.Empty);
        }

        /// <summary>
        /// Valid format is A12345678
        /// Alpha 8 digits
        /// </summary>
        /// <param name="onsCode"></param>
        /// <returns>true or false</returns>
        public static bool OnsCodeIsValid(string? onsCode)
        {
            if (string.IsNullOrEmpty(onsCode)) return false;

            if (onsCode.Length != 9) return false;

            // verify that the first character is an alphabetic character
            char[] firstPart = onsCode.ToCharArray(0, 9);

            if (!char.IsLetter(firstPart[0])) return false;

            // verify that the last 8 characters of the first part are numeric
            for (var i = 1; i < firstPart.Length; i++)
            {
                if (!char.IsNumber(firstPart[i])) return false;
            }

            return true;
        }


        /// <summary>
        /// Valid format is A12345678-12345
        /// Alpha 8 digits hyphen 5 digits
        /// </summary>
        /// <param name="urn"></param>
        /// <returns>true or false</returns>
        private static bool ValidateUrnFormat(string? urn)
        {
            if (string.IsNullOrEmpty(urn)) return false;

            var splitUrn = urn.Split('-');
            if (splitUrn.Length != 2) return false;

            if (splitUrn[0].Length != 9) return false;

            // verify that the first character of the first part is an alphabetic character
            char[] firstPart = splitUrn[0].ToCharArray(0, 9);

            if (!char.IsLetter(firstPart[0])) return false;

            // verify that the last 8 characters of the first part are numeric
            for (var i = 1; i < firstPart.Length; i++)
            {
                if (!char.IsNumber(firstPart[i])) return false;
            }

            // verify second part has a length of 5
            if (splitUrn[1].Length != 5) return false;

            // numeric
            char[] secondPart = splitUrn[1].ToCharArray();

            for (var i = 0; i < secondPart.Length; i++)
            {
                if (!char.IsNumber(secondPart[i])) return false;
            }

            return true;
        }

        private async Task<bool> URN_Is_In_The_DB_Rule(string? urn)
        {
            if (string.IsNullOrEmpty(urn)) return false;

            string onsCode = urn.Substring(0, 9);

            try
            {
                var httpClient = _httpClientFactory.CreateClient(LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(LocalAuthorityApi.RouteOnsExists + onsCode);

                return httpResponseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload declaration - BasicValidationService - URN_Is_In_The_DB_Rule");
                return false;
            }
        }

        private static List<string> DuplicateAddresses_Rule(List<RawDeclaration> rawDeclarations)
        {
            return DuplicateCheck(rawDeclarations, "Addresses");
        }

        private static int InvalidUrnFormat_Rule(List<RawDeclaration> rawDeclarations)
        {
            var counter = 0;

            foreach (var t in rawDeclarations)
            {
                if (!ValidateUrnFormat(t.LA_Declaration_Unique_Reference_Number)) counter++;
            }

            return counter;
        }

        private static bool Single_Submitting_Local_Authorities_Rule(List<RawDeclaration> rawDeclarations)
        {
            var multipleUrns = rawDeclarations
                .Select(x => x.LA_Declaration_Unique_Reference_Number!.Substring(0, 10))
                .Distinct().ToList();

            return multipleUrns.Count == 1;
        }

        private static List<string> DuplicateUrn_Rule(List<RawDeclaration> rawDeclarations)
        {
            return DuplicateCheck(rawDeclarations, "URN");
        }

        class DeclarationGroup
        {
            public List<RawDeclaration>? rows;
        }

        private static List<string> DuplicateCheck(List<RawDeclaration> rawDeclarations, string fieldName)
        {
            var genericMessageOverride = false;
            var duplicates = new List<DeclarationGroup>();
            var errors = new List<string>();

            switch (fieldName)
            {
                case "URN":
                    duplicates =rawDeclarations.GroupBy(x => x.LA_Declaration_Unique_Reference_Number)
                        .Where(group => group.Count() > 1)
                        .Select(group => new DeclarationGroup()
                        {
                            rows = group.ToList()
                        }).ToList();
                    break;
                case "Addresses":
                    duplicates = rawDeclarations.GroupBy(x => $"{x.Address_Line_1}:{x.Address_Line_2}")
                        .Where(group => group.Count() > 1)
                        .Select(group => new DeclarationGroup()
                        {
                            rows = group.ToList()
                        }).ToList();
                    break;
            }

            var duplicateDetail = new StringBuilder();

            foreach (var rows in duplicates.Select(group => group.rows))
            {
                switch (rows?.Count)
                {
                    case 2:
                        duplicateDetail.AppendLine($"Duplicate {fieldName} found in rows {rows[0].RowNumber + 1} and {rows[1].RowNumber + 1} of the CSV file, file is invalid");
                        break;
                    case 3:
                        duplicateDetail.AppendLine($"Duplicate {fieldName} found in rows {rows[0].RowNumber + 1}, {rows[1].RowNumber + 1} and {rows[2].RowNumber + 1} of the CSV file, file is invalid");
                        break;
                    case 4:
                        duplicateDetail.AppendLine($"Duplicate {fieldName} found in rows {rows[0].RowNumber + 1}, {rows[1].RowNumber + 1}, {rows[2].RowNumber + 1} and {rows[3].RowNumber + 1} of the CSV file, file is invalid");
                        break;
                    default:
                        duplicateDetail.AppendLine($"Duplicate {fieldName} found in the CSV file, file is invalid.");
                        genericMessageOverride = true;
                        break;
                }
            }
            if (genericMessageOverride)
            {
                errors = [$"Duplicate {fieldName} identified in the CSV file, file is invalid."];
            }
            else
            {
                if (!string.IsNullOrEmpty(duplicateDetail.ToString()))
                {
                    errors = duplicateDetail.ToString().Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                        .ToList();
                    if (errors.Count > 5)
                    {
                        errors = [$"Duplicate {fieldName} identified in the CSV file, file is invalid."];
                    }
                }
            }

            return errors;
        }

        private static IEnumerable<string?> BlankUrn_Rule(List<RawDeclaration> rawDeclarations)
        {
            var blankUrns = rawDeclarations.GroupBy(x => x.LA_Declaration_Unique_Reference_Number)
                .Where(group => string.IsNullOrWhiteSpace(group.Key))
                .Select(group => group.Key);

            return blankUrns;
        }

        private static IEnumerable<string?> NotApplicableUrn_Rule(List<RawDeclaration> rawDeclarations)
        {
            var notApplicableUrns = rawDeclarations.GroupBy(x => x.LA_Declaration_Unique_Reference_Number)
                .Where(group => group.Key == "N/A")
                .Select(group => group.Key);

            return notApplicableUrns;
        }
    }
}
