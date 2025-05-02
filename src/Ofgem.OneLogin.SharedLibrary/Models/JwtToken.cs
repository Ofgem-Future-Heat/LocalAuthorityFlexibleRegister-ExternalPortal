namespace Ofgem.OneLogin.SharedLibrary.Models;

public class JwtToken
{
    public string? Subject { get; set; }
    public string? Issuer { get; set; }
    public IEnumerable<string>? Audience { get; set; }
    public DateTime? IssuedAt { get; set; }
}