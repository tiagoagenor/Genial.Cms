using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Genial.Cms.Controllers.V1;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/st")]
public class ServerTimeController : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Retorna o timestamp atual do servidor")]
    [ProducesResponseType(typeof(ServerTimeResponse), StatusCodes.Status200OK)]
    public IActionResult GetServerTime()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        return Ok(new ServerTimeResponse
        {
            Timestamp = timestamp,
            DateTime = DateTimeOffset.UtcNow.ToString("o")
        });
    }
}

public class ServerTimeResponse
{
    public long Timestamp { get; set; }
    public string DateTime { get; set; }
}
