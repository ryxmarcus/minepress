using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace erp.minepress.frameworks.Authentication;

/// <summary>
/// JWT token generation and validation service
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }

    public string GenerateAccessToken(JwtUserIdentity user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("user_id", user.UserId.ToString()),
            new("user_code", user.UserCode),
            new("user_name", user.UserName),
            new("name", user.Name),
            new("email", user.Email ?? string.Empty),
            new("mobile", user.MobileNo ?? string.Empty),
            new("location_id", user.LocationId.ToString()),
            new("department_id", user.DepartmentId.ToString()),
            new("dept_code", user.DeptCode),
            new("dept_name", user.DeptName),
            new("employee_code", user.EmployeeCode ?? string.Empty),
            new("designation_id", user.DesignationId.ToString()),
            new("is_system_admin", user.IsSystemAdmin.ToString().ToLower()),
            new("is_production_user", user.IsProductionUser.ToString().ToLower()),
            new("is_approval_user", user.IsApprovalUser.ToString().ToLower()),
            new("user_type", user.UserType ?? string.Empty),
            new("company_id", user.CompanyId?.ToString() ?? string.Empty),
            new("ref_id", user.RefId?.ToString() ?? string.Empty),
            new("tenant_id", user.TenantId?.ToString() ?? string.Empty),
            new("tenant_key", user.TenantKey ?? string.Empty),
            new("login_at", user.LoginAt.ToString("O"))
        };

        // Add roles as individual claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions as JSON array claim
        claims.Add(new Claim("permissions", JsonSerializer.Serialize(user.Permissions)));

        // Add party roles as JSON array claim
        claims.Add(new Claim("party_roles", JsonSerializer.Serialize(user.PartyRoles)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public TokenResponse GenerateTokenPair(JwtUserIdentity user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
            TokenType = "Bearer"
        };
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = _settings.ValidateIssuer,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = _settings.ValidateAudience,
                ValidAudience = _settings.Audience,
                ValidateLifetime = _settings.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewSeconds)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public JwtUserIdentity? GetUserFromToken(string token)
    {
        var principal = ValidateAccessToken(token);
        return principal == null ? null : GetUserFromClaims(principal);
    }

    public JwtUserIdentity? GetUserFromClaims(ClaimsPrincipal principal)
    {
        try
        {
            var userIdClaim = principal.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                return null;

            var user = new JwtUserIdentity
            {
                UserId = userId,
                UserCode = principal.FindFirst("user_code")?.Value ?? string.Empty,
                UserName = principal.FindFirst("user_name")?.Value ?? string.Empty,
                Name = principal.FindFirst("name")?.Value ?? string.Empty,
                Email = principal.FindFirst("email")?.Value,
                MobileNo = principal.FindFirst("mobile")?.Value,
                LocationId = int.TryParse(principal.FindFirst("location_id")?.Value, out var locId) ? locId : 0,
                DepartmentId = long.TryParse(principal.FindFirst("department_id")?.Value, out var deptId) ? deptId : 0,
                DeptCode = principal.FindFirst("dept_code")?.Value ?? string.Empty,
                DeptName = principal.FindFirst("dept_name")?.Value ?? string.Empty,
                EmployeeCode = principal.FindFirst("employee_code")?.Value,
                DesignationId = long.TryParse(principal.FindFirst("designation_id")?.Value, out var desigId) ? desigId : 0,
                IsSystemAdmin = principal.FindFirst("is_system_admin")?.Value?.ToLower() == "true",
                IsProductionUser = principal.FindFirst("is_production_user")?.Value?.ToLower() == "true",
                IsApprovalUser = principal.FindFirst("is_approval_user")?.Value?.ToLower() == "true",
                UserType = principal.FindFirst("user_type")?.Value,
                CompanyId = int.TryParse(principal.FindFirst("company_id")?.Value, out var compId) ? compId : null,
                RefId = long.TryParse(principal.FindFirst("ref_id")?.Value, out var refId) ? refId : null,
                TenantId = Guid.TryParse(principal.FindFirst("tenant_id")?.Value, out var tenantId) ? tenantId : null,
                TenantKey = principal.FindFirst("tenant_key")?.Value,
                LoginAt = DateTime.TryParse(principal.FindFirst("login_at")?.Value, out var loginAt) ? loginAt : DateTime.UtcNow
            };

            // Parse roles from claims
            user.Roles = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Parse permissions from JSON claim
            var permissionsClaim = principal.FindFirst("permissions")?.Value;
            if (!string.IsNullOrEmpty(permissionsClaim))
            {
                user.Permissions = JsonSerializer.Deserialize<List<string>>(permissionsClaim) ?? [];
            }

            // Parse party roles from JSON claim
            var partyRolesClaim = principal.FindFirst("party_roles")?.Value;
            if (!string.IsNullOrEmpty(partyRolesClaim))
            {
                user.PartyRoles = JsonSerializer.Deserialize<List<string>>(partyRolesClaim) ?? [];
            }

            return user;
        }
        catch
        {
            return null;
        }
    }
}
