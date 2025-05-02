namespace Ofgem_Web_LAF_ExternalPortal;

public class HeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public HeaderHandler(DelegatingHandler innerHandler, IHttpContextAccessor httpContextAccessor) : base(innerHandler)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null) return await base.SendAsync(request, cancellationToken);

        var user = _httpContextAccessor.HttpContext.User;

        if (user is null) return await base.SendAsync(request, cancellationToken);
        if (user.Claims is null) return await base.SendAsync(request, cancellationToken);

        var claim = user.Claims.FirstOrDefault(x => x.Type == "name");

        if (claim is null) return await base.SendAsync(request, cancellationToken);

        string userName = claim.Value;

        if(string.IsNullOrEmpty(userName)) return await base.SendAsync(request, cancellationToken);

        request.Headers.Add("X-UserName", userName);

        return await base.SendAsync(request, cancellationToken);
    }
}
