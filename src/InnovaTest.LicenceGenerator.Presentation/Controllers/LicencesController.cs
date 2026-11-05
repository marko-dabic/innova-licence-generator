using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Application.Features.Licences;
using InnovaTest.LicenceGenerator.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;

namespace InnovaTest.LicenceGenerator.Presentation.Controllers;

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
    public async Task<ActionResult<LicenceDocument>> GenerateLicence([FromBody] BaseLicenceRequestDto licenceRequest,
        CancellationToken cancellationToken)
        => Ok(await licenceService.GenerateLicenceAsync(licenceRequest, cancellationToken));

    [HttpPost("activate-licence")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LicenceDocument>> ActivateLicence([FromBody] ActivateLicenceDto request,
        CancellationToken cancellationToken)
        => Ok(await licenceService.ActivateLicenceAsync(request, cancellationToken));
}
