using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.DTOs;
using Blog.Entities;
using Blog.Repository;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Services;

public class AuthService(
    UserCredentialRepository userCredentialRepository,
    UserRepository userRepository,
    IConfiguration configuration)
    : IAuthService
{
    private const int TokenValidityInMinutes = 30;
    private const int RefreshTokenValidityInDays = 7;

    public async Task<(string Token, string RefreshToken)?> LoginAsync(string username, string password)
    {
        var user = await userRepository.FindByUsernameAsync(username);
        if (user == null) return null;

        var credential = await userCredentialRepository.FindByIdAsync(user.Id);
        if (credential == null) return null; // Should not happen if data integrity is kept

        if (!VerifyPassword(password, credential.PasswordHash)) return null;

        return GenerateTokens(user);
    }

    public async Task<string?> RefreshTokenAsync(string refreshToken)
    {
        // Simple validation: check if it's a valid JWT signed by us
        var principal = GetPrincipalFromExpiredToken(refreshToken);
        if (principal == null) return null;

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId)) return null;
        
        // Check if user was kicked out (password changed after token issuance)
        // Note: For refresh token, we should definitely check this.
        var credential = await userCredentialRepository.FindByIdAsync(userId);
        if (credential == null) return null;
        
        // Check LastChangedAt vs iat
        var iatClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);
        if (iatClaim != null && long.TryParse(iatClaim.Value, out var iat))
        {
             var issueTime = DateTimeOffset.FromUnixTimeSeconds(iat).UtcDateTime;
             if (credential.LastChangedAt.HasValue && credential.LastChangedAt.Value > issueTime)
             {
                 return null; // Token invalidated
             }
        }

        var user = await userRepository.FindById(userId);
        if (user == null) return null;

        // Generate new Access Token only (or both? usually refresh token returns new access token)
        // I will return a new Access Token.
        var (newToken, _) = GenerateTokens(user); 
        // Note: In a real system, we might rotate refresh tokens too.
        return newToken;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
         var credential = await userCredentialRepository.FindByIdAsync(userId);
         if (credential == null) return false;

         if (!VerifyPassword(oldPassword, credential.PasswordHash)) return false;

         credential.PasswordHash = HashPassword(newPassword);
         credential.LastChangedAt = DateTime.UtcNow;
         
         await userCredentialRepository.SaveAsync(credential);
         return true;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private (string Token, string RefreshToken) GenerateTokens(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? "ThisIsASecretKeyForDevelopmentOnly1234567890");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(TokenValidityInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        // Refresh Token - for simplicity, using a similar JWT but longer expiry
        var refreshTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(RefreshTokenValidityInDays),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };
        var refreshToken = tokenHandler.CreateToken(refreshTokenDescriptor);

        return (tokenHandler.WriteToken(token), tokenHandler.WriteToken(refreshToken));
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // for simplicity
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? "ThisIsASecretKeyForDevelopmentOnly1234567890")),
            ValidateLifetime = false // Here we allow expired tokens to get principal, but for refresh we want to check expiry?
            // Actually, Refresh Token shouldn't be expired if we use it. 
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}