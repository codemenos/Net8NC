namespace Solucao.Service.API.Core.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;

public class AuthorizationService
{
    public Dictionary<string, StringValues> ParseOAuthParameters(HttpContext httpContext, List<string> excluding = null)
    {
        excluding ??= [];

        var parameters = GetParameters(httpContext, excluding);

        return parameters;
    }

    public string BuildRedirectUrl(HttpRequest request, IDictionary<string, StringValues> oAuthParameters)
    {
        var url = request.PathBase + request.Path + QueryString.Create(oAuthParameters);

        return url;
    }

    public bool IsAuthenticated(AuthenticateResult authenticateResult, OpenIddictRequest request)
    {
        if (!authenticateResult.Succeeded)
        {
            return false;
        }

        if (request.MaxAge.HasValue && authenticateResult.Properties != null)
        {
            var maxAgeSeconds = TimeSpan.FromSeconds(request.MaxAge.Value);

            var expired = !authenticateResult.Properties.IssuedUtc.HasValue ||
                          DateTimeOffset.UtcNow - authenticateResult.Properties.IssuedUtc > maxAgeSeconds;
            if (expired)
            {
                return false;
            }
        }

        return true;
    }

    public static List<string> GetDestinations(ClaimsIdentity identity, Claim claim)
    {
        List<string> destinations = [];

        if (claim.Type is OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Email)
        {
            destinations.Add(OpenIddictConstants.Destinations.AccessToken);
        }

        return destinations;
    }


    //Private

    private static Dictionary<string, StringValues> GetParameters(HttpContext httpContext, List<string> excluding)
    {
        if (httpContext.Request.HasFormContentType)
        {
            return GetFormParameters(httpContext, excluding);
        }
        else
        {
            return GetQueryParameters(httpContext, excluding);
        }
    }

    private static Dictionary<string, StringValues> GetFormParameters(HttpContext httpContext, List<string> excluding)
    {
        var result = httpContext.Request.Form
            .Where(v => !excluding.Contains(v.Key))
            .ToDictionary(v => v.Key, v => v.Value);

        return result;
    }

    private static Dictionary<string, StringValues> GetQueryParameters(HttpContext httpContext, List<string> excluding)
    {
        var result = httpContext.Request.Query
            .Where(v => !excluding.Contains(v.Key))
            .ToDictionary(v => v.Key, v => v.Value);

        return result; 
    }
}