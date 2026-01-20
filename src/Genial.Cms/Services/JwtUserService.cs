using System.Security.Claims;
using Genial.Cms.Domain.Dtos;
using Microsoft.AspNetCore.Http;

namespace Genial.Cms.Services;

public class JwtUserService : IJwtUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public JwtUserData GetUserData()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return new JwtUserData();
        }

        return new JwtUserData
        {
            UserId = user.FindFirst("userId")?.Value,
            Email = user.FindFirst(ClaimTypes.Email)?.Value,
            StageId = user.FindFirst("stageId")?.Value,
            StageKey = user.FindFirst("stageKey")?.Value,
            StageLabel = user.FindFirst("stageLabel")?.Value
        };
    }
}

