using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<LicenceTypeEnum>))]
public enum LicenceTypeEnum
{
    Standard,
    ActivateOnImport,
    Perpetual
}
