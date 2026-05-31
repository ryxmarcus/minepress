namespace erp.minepress.frameworks.Authentication;

/// <summary>
/// JWT configuration settings loaded from appsettings.json
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key used for signing JWT tokens (should be at least 32 characters)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (typically the application name or URL)
    /// </summary>
    public string Issuer { get; set; } = "MinePress";

    /// <summary>
    /// Token audience (typically the application domain)
    /// </summary>
    public string Audience { get; set; } = "MinePress";

    /// <summary>
    /// Access token expiration in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Whether to validate issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance for token validation
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 30;
}
