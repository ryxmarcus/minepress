using Microsoft.AspNetCore.Http;

namespace erp.minepress.frameworks.Authentication;

/// <summary>
/// Middleware that validates JWT tokens and populates HttpContext.Items with user identity
/// </summary>
public class JwtAuthGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _tokenService;

    public JwtAuthGuardMiddleware(RequestDelegate next, IJwtTokenService tokenService)
    {
        _next = next;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for excluded paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        // Try to get token from Authorization header
        var token = ExtractToken(context);

        if (!string.IsNullOrEmpty(token))
        {
            var user = _tokenService.GetUserFromToken(token);
            if (user != null)
            {
                // Store user identity in HttpContext.Items for easy access
                context.Items["JwtUser"] = user;
                context.Items["IsAuthenticated"] = true;
            }
        }

        await _next(context);
    }

    private static string? ExtractToken(HttpContext context)
    {
        // 1. Try Authorization header (Bearer token)
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..].Trim();
        }

        // 2. Try cookie (for web applications)
        if (context.Request.Cookies.TryGetValue("jwt_token", out var cookieToken))
        {
            return cookieToken;
        }

        // 3. Try query string (for special cases like WebSocket connections)
        if (context.Request.Query.TryGetValue("access_token", out var queryToken))
        {
            return queryToken.FirstOrDefault();
        }

        return null;
    }

    private static bool IsExcludedPath(string path)
    {
        var excludedPaths = new[]
        {
            "/account/login",
            "/account/logout",
            "/api/auth/login",
            "/api/auth/refresh",
            "/api/auth/register",
            "/health",
            "/favicon.ico"
        };

        return excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Extension methods for accessing JWT user from HttpContext
/// </summary>
public static class JwtHttpContextExtensions
{
    private const string JwtUserKey = "JwtUser";
    private const string IsAuthenticatedKey = "IsAuthenticated";

    /// <summary>
    /// Gets the authenticated JWT user from HttpContext
    /// </summary>
    public static JwtUserIdentity? GetJwtUser(this HttpContext context)
    {
        return context.Items.TryGetValue(JwtUserKey, out var user) ? user as JwtUserIdentity : null;
    }

    /// <summary>
    /// Checks if the request has a valid JWT token
    /// </summary>
    public static bool IsJwtAuthenticated(this HttpContext context)
    {
        return context.Items.TryGetValue(IsAuthenticatedKey, out var isAuth) && isAuth is true;
    }

    /// <summary>
    /// Sets the JWT user in HttpContext (typically called by middleware)
    /// </summary>
    public static void SetJwtUser(this HttpContext context, JwtUserIdentity user)
    {
        context.Items[JwtUserKey] = user;
        context.Items[IsAuthenticatedKey] = true;
    }
}
