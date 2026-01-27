using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Genial.Cms.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfiguration _jwtConfiguration;

    public JwtTokenService(JwtConfiguration jwtConfiguration)
    {
        _jwtConfiguration = jwtConfiguration;
    }

    public string GenerateToken(string userId, string email, Stage stage)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey);

        var claims = new List<Claim>
        {
            new Claim("userId", userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("stageId", stage.Id),
            new Claim("stageKey", stage.Key),
            new Claim("stageLabel", stage.Label),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfiguration.ExpirationInMinutes),
            Issuer = _jwtConfiguration.Issuer,
            Audience = _jwtConfiguration.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfiguration.Audience,
                ValidateLifetime = false, // Não validar expiração aqui, pois queremos renovar tokens expirados
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string RefreshToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
        {
            throw new SecurityTokenException("Token inválido");
        }

        // Extrair claims do token antigo
        var userId = principal.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var stageId = principal.Claims.FirstOrDefault(c => c.Type == "stageId")?.Value;
        var stageKey = principal.Claims.FirstOrDefault(c => c.Type == "stageKey")?.Value;
        var stageLabel = principal.Claims.FirstOrDefault(c => c.Type == "stageLabel")?.Value;

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(stageId) ||
            string.IsNullOrWhiteSpace(stageKey) ||
            string.IsNullOrWhiteSpace(stageLabel))
        {
            throw new SecurityTokenException("Token não contém as claims necessárias");
        }

        // Criar novo token com as mesmas informações mas com nova data de expiração
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey);

        var claims = new List<Claim>
        {
            new Claim("userId", userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("stageId", stageId),
            new Claim("stageKey", stageKey),
            new Claim("stageLabel", stageLabel),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfiguration.ExpirationInMinutes),
            Issuer = _jwtConfiguration.Issuer,
            Audience = _jwtConfiguration.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var newToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(newToken);
    }
}

