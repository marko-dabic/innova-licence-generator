using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using Xunit;

namespace InnovaTest.LicenceGenerator.Tests;

public sealed class LicenceRequestContractTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowOutOfOrderMetadataProperties = true
    };

    [Theory]
    [InlineData("Standard", typeof(StandardLicenceDto), ",\"validityInMonths\":6")]
    [InlineData("ActivateOnImport", typeof(ActivateOnImportLicenceDto), ",\"validityInMonths\":6")]
    [InlineData("Perpetual", typeof(PerpetualLicenceDto), "")]
    public void ExistingRequestsSelectCorrectSubtype(string type, Type expected, string duration)
    {
        var json = "{\"deviceId\":\"device\",\"enabledOptions\":[{\"name\":\"package\",\"options\":[{\"name\":\"feature\"}]}],\"maxConcurrentSessions\":1,\"licenceType\":\"" + type + "\"" + duration + "}";
        var request = JsonSerializer.Deserialize<BaseLicenceRequestDto>(json, Options);
        Assert.IsType(expected, request);
        Assert.Equal("feature", request!.EnabledOptions[0].Options![0].Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void OptionNamesStillRequireNonWhitespaceText(string name)
    {
        var option = new LicenceOptionDto { Name = name };
        Assert.False(Validator.TryValidateObject(option, new ValidationContext(option), [], true));
    }

    [Fact]
    public void PerpetualRequestStillRejectsDuration()
    {
        const string json = "{\"deviceId\":\"device\",\"enabledOptions\":[],\"maxConcurrentSessions\":1,\"licenceType\":\"Perpetual\",\"validityInMonths\":6}";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseLicenceRequestDto>(json, Options));
    }
}
