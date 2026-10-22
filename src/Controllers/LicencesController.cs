using InnovaTest.LicenceGenerator.Services;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InnovaTest.LicenceGenerator.Controllers;

[ApiController]
[Route("licences")]
[Consumes("application/json")]
public sealed class LicencesController : ControllerBase
{
    private readonly ILicenceService _licenceService;

    public LicencesController(ILicenceService licenceService)
    {
        _licenceService = licenceService;
    }

    [HttpPost("issue-licence")]
    [RequestSizeLimit(65536)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<LicenceDocument>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public ActionResult<LicenceDocument> IssueLicence([FromBody] LicenceRequestDto licenceRequest)
        => Ok(_licenceService.IssueLicence(licenceRequest));
}
