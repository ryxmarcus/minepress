using System.Security.Claims;

namespace erp.minepress.frameworks.Authentication;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for the given user identity
    /// </summary>
    string GenerateAccessToken(JwtUserIdentity user);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Generates both access and refresh tokens
    /// </summary>
    TokenResponse GenerateTokenPair(JwtUserIdentity user);

    /// <summary>
    /// Validates an access token and returns the claims principal
    /// </summary>
    ClaimsPrincipal? ValidateAccessToken(string token);

    /// <summary>
    /// Extracts user identity from a valid token
    /// </summary>
    JwtUserIdentity? GetUserFromToken(string token);

    /// <summary>
    /// Extracts user identity from ClaimsPrincipal
    /// </summary>
    JwtUserIdentity? GetUserFromClaims(ClaimsPrincipal principal);
}
