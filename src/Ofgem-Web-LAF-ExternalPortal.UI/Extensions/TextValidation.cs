namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class TextValidation
    {
        private static readonly System.Text.RegularExpressions.Regex AcceptableValidator = new(
            @"[^a-zA-Z1234567890,_'-]+",
            System.Text.RegularExpressions.RegexOptions.Compiled |
            System.Text.RegularExpressions.RegexOptions.CultureInvariant |
            System.Text.RegularExpressions.RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(500));

        public static bool IsValidName(string name)
        {
            return AcceptableValidator.IsMatch(name);
        }
    }
}
