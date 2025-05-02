using System.Text.RegularExpressions;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class AddressValidation
    {
        public const string NotApplicable = "N/A";

        // rules ADDRESS-001 and ADDRESS-002
        public static bool IsAddressLineValid(string? addressLine)
        {
            if (addressLine != null && 
                string.Equals(addressLine.Replace(" ", string.Empty).Trim().ToUpper(), NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !IsAddressLineEmpty(addressLine);
        }

        public static bool IsAddressLineEmpty(string? addressLine)
        {
            return string.IsNullOrEmpty(addressLine);
        }

        // rules ADDRESS-003 and ADDRESS-004
        public static bool IsPostcodeValid(string? postcode)
        {
            if (postcode != null && 
                string.Equals(postcode.Replace(" ", string.Empty).Trim().ToUpper(), NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return !IsPostcodeEmpty(postcode);
        }

        public static bool IsPostcodeEmpty(string? postcode)
        {
            return string.IsNullOrEmpty(postcode);
        }

        public static bool IsPostcodeCorrect(string? postcode)
        {
            var result = postcode != null && Regex.IsMatch(
                postcode,
                "^(([A-Z][0-9]{1,2})|(([A-Z][A-HJ-Y][0-9]{1,2})|(([A-Z][0-9][A-Z])|([A-Z][A-HJ-Y][0-9]?[A-Z])))) [0-9][A-Z]{2}$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(500)
            );

            return result;
        }

    }
}
