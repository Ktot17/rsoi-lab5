namespace GatewayServer.Handlers;

public class AuthTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var incoming = httpContextAccessor.HttpContext?
            .Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(incoming))
        {
            request.Headers.TryAddWithoutValidation("Authorization", incoming);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
