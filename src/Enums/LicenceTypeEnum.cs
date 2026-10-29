using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Models;

[JsonConverter(typeof(JsonStringEnumConverter<LicenceTypeEnum>))]
public enum LicenceTypeEnum
{
    Standard,
    ActivateOnImport,
    Perpetual
}
