namespace Genial.Cms.Infra.CrossCutting.Environments.Configurations;

public class JwtConfiguration
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationInMinutes { get; set; } = 60;
}

