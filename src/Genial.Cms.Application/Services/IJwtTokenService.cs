using System.Security.Claims;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, Stage stage);
    ClaimsPrincipal? ValidateToken(string token);
    string RefreshToken(string token);
}

