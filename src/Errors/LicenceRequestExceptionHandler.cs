using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InnovaTest.LicenceGenerator.Errors;

internal sealed class LicenceRequestExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge })
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status413PayloadTooLarge,
                    Title = "The request body exceeds the 64 KiB limit."
                }
            });
        }
        if (exception is not InvalidLicenceRequestException validationException) return false;

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [validationException.Field] = [validationException.Message]
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The licence request is invalid."
            }
        });
    }
}
