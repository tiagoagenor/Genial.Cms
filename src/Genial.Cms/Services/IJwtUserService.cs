using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Services;

public interface IJwtUserService
{
    JwtUserData GetUserData();
}

