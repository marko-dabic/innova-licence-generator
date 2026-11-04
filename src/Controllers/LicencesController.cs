using InnovaTest.LicenceGenerator.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Features.Licences;
using InnovaTest.LicenceGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using InnovaTest.LicenceGenerator.Features.Licences.Activate.Dtos;

namespace InnovaTest.LicenceGenerator.Controllers;

[ApiController]
[Route("licences")]
[Consumes("application/json")]
[RequestSizeLimit(65536)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[ProducesResponseType<LicenceDocument>(StatusCodes.Status200OK)]
[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class LicencesController(ILicenceService licenceService) : ControllerBase
{
    [HttpPost("generate-licence")]
    public ActionResult<LicenceDocument> GenerateLicence([FromBody] BaseLicenceRequestDto licenceRequest)
        => Ok(licenceService.GenerateLicence(licenceRequest));

    [HttpPost("activate-licence")]
    public async Task<ActionResult<LicenceDocument>> ActivateLicence([FromBody] ActivateLicenceDto request,
        CancellationToken cancellationToken)
        => Ok(await licenceService.ActivateLicenceAsync(request, cancellationToken));
}
