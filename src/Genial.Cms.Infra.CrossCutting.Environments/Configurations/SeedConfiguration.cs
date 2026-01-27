using System.Collections.Generic;

namespace Genial.Cms.Infra.CrossCutting.Environments.Configurations;

public class SeedConfiguration
{
    public List<SeedUserConfiguration> SeedUsers { get; set; } = new();
}

