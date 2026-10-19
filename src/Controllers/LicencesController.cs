using InnovaTest.LicenceGenerator.Configuration;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InnovaTest.LicenceGenerator.Controllers;

[ApiController]
[Route("licences")]
[Consumes("application/json")]
public sealed class LicencesController : ControllerBase
{
    private readonly LicenceIssuer _issuer;
    private readonly SigningKey _signingKey;

    public LicencesController(LicenceIssuer issuer, SigningKey signingKey)
    {
        _issuer = issuer;
        _signingKey = signingKey;
    }

    [HttpPost]
    [RequestSizeLimit(65536)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<LicenceDocument>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public ActionResult<LicenceDocument> Issue([FromBody] LicenceRequestDto request)
        => Ok(_issuer.Issue(request, _signingKey.Pem));
}
