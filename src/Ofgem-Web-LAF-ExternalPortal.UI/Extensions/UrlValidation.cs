namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class UrlValidation
    {
        public static bool IsLinkValid(string? link)
        {
            if (link == null) return false;

            return !IsLinkEmpty(link) && link.StartsWith("https://");
        }

        public static bool IsLinkEmpty(string? link)
        {
            return string.IsNullOrEmpty(link);
        }
    }
}
