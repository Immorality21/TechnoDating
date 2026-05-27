using System.Net;
using System.Net.Http.Headers;

namespace TechnoDating.Services;

public class AuthMessageHandler(IAuthStateService auth) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AttachBearer(request, auth.AccessToken);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var refreshed = await auth.RefreshAsync(cancellationToken);
        if (!refreshed)
        {
            await auth.LogoutAsync(cancellationToken);
            return response;
        }

        response.Dispose();

        var retry = await CloneAsync(request);
        AttachBearer(retry, auth.AccessToken);
        return await base.SendAsync(retry, cancellationToken);
    }

    private static void AttachBearer(HttpRequestMessage request, string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version,
        };

        if (original.Content is not null)
        {
            var stream = new MemoryStream();
            await original.Content.CopyToAsync(stream);
            stream.Position = 0;
            clone.Content = new StreamContent(stream);
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in original.Headers)
        {
            if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
