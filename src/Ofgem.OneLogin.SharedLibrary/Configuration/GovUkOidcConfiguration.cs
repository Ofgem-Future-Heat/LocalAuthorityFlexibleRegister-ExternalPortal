namespace Ofgem.OneLogin.SharedLibrary.Configuration;

public class GovUkOidcConfiguration
{
    public required string BaseUrl { get; set; }
    public required string ClientId { get; set; }
    public required string KeyVaultIdentifier { get; set; }
    public required string EnableMfa { get; set; }
}

public static class GovUkConstants
{
    public const string StubAuthCookieName = "LAF.StubAuthCookie";
    public const string AuthCookieName = "LAF.AuthCookie";
}