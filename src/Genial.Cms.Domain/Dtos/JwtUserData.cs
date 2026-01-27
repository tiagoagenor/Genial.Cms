namespace Genial.Cms.Domain.Dtos;

public class JwtUserData
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string StageId { get; set; }
    public string StageKey { get; set; }
    public string StageLabel { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Email);
    }
}

