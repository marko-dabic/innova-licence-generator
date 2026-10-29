using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Models;

[JsonConverter(typeof(JsonStringEnumConverter<LicenceType>))]
public enum LicenceType
{
    Standard,
    ActivateOnImport,
    Perpetual
}
