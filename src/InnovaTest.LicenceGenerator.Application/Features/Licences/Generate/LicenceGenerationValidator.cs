using InnovaTest.LicenceGenerator.Application.Errors;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Generate;

internal static class LicenceGenerationValidator
{
    public static void Validate(BaseLicenceRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request,
                new ValidationContext(request), results, validateAllProperties: true))
            throw new InvalidLicenceRequestException(results[0].MemberNames.FirstOrDefault() ?? "request",
                results[0].ErrorMessage ?? "Invalid licence request.");

        if (!string.IsNullOrWhiteSpace(request.DeviceId) && request.DeviceId != request.DeviceId.Trim())
            throw new InvalidLicenceRequestException(nameof(request.DeviceId),
                "Device ID must not have surrounding whitespace.");

        ValidateOptions(request.EnabledOptions, "enabledOptions");
    }

    private static void ValidateOptions(LicenceOptionDto[] options, string path)
    {
        foreach (var (index, option) in options.Index())
        {
            var optionPath = $"{path}[{index}]";
            ValidateOption(option, optionPath);
        }
    }

    private static void ValidateOption(LicenceOptionDto? option, string path)
    {
        if (option is null)
            throw new InvalidLicenceRequestException(path, "Option elements must not be null.");

        if (string.IsNullOrWhiteSpace(option.Name))
            throw new InvalidLicenceRequestException($"{path}.name", "An option name is required.");

        if (option.Options is not null)
            ValidateOptions(option.Options, $"{path}.options");
    }
}
