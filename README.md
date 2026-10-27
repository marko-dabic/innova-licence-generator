# InnovaTEST Licence Generator

ASP.NET Core API that generates RSA-signed JWT licences.

## Run

Requires the .NET 10 SDK and an RSA private key in PEM format (at least 3072 bits).

Set `LicenceGenerator:PrivateKeyPath` in `appsettings.json`. The default is `keys/licence.private.pem`, relative to the project root. Keep the private key on the issuing server. Restart the API after changing the key.

From the `innova-licence-generator` directory:

```powershell
dotnet build InnovaTest.LicenceGenerator.slnx
dotnet run --project src
```

API: `http://localhost:5050`  
Swagger: `http://localhost:5050/swagger`

## Generate a licence

Send JSON to `POST /licences/issue-licence`:

```json
{
  "deviceId": "INNOVATEST-SERIAL-12345",
  "enabledOptions": [
    {
      "name": "package",
      "options": [
        { "name": "option-1" },
        { "name": "option-2" },
        { "name": "option-3" }
      ]
    },
    { "name": "option-4" }
  ],
  "licenceType": "Standard",
  "maxConcurrentSessions": 2
}
```

`deviceId`, `enabledOptions`, `maxConcurrentSessions`, and the string discriminator `licenceType` are required. `maxConcurrentSessions` must be positive. This replaces the previous request format with independent licence flags.

| `licenceType` | Request DTO | Expiry |
| --- | --- | --- |
| `Standard` | `StandardLicenceDto` | Fixed at issuance time plus six months. |
| `ActivateOnImport` | `ActivateOnImportLicenceDto` | Calculated by the importing application using `validityInMonths` (required positive integer). |
| `Perpetual` | `PerpetualLicenceDto` | No expiry. |

All three sealed DTOs inherit `BaseLicenceRequestDto`. Its `LicenceType` enum property is determined by the subtype; JSON uses `licenceType` to select that subtype. The import and perpetual DTOs expose read-only `IsActivateOnImport` and `IsPerpetual` flags respectively, both always `true`; callers can omit them.

For an activate-on-import request, use the same common fields with `"licenceType": "ActivateOnImport"` and a required `"validityInMonths"` greater than zero, for example `"validityInMonths": 12`. For a perpetual request, use `"licenceType": "Perpetual"` and omit validity. Fields belonging to another subtype are rejected.

A successful response returns HTTP 200:

```json
{
  "licence": "<signed JWT>"
}
```

The `licence` property contains the signed JWT with the licence ID, issuer, audience, device ID, enabled options, validity, session limit, and UTC dates:

- `issuedAtUtc`: the time the licence was generated.
- `expireAtUtc`: `issuedAtUtc` plus six months for standard licences; `null` for activate-on-import and perpetual licences.
- `licenceType`: identifies the licence behavior (`Standard`, `ActivateOnImport`, or `Perpetual`). The signed claims do not include the redundant `isActivateOnImport` and `isPerpetual` flags.
- `validityInMonths`: the import duration for activate-on-import licences; `null` otherwise.

Dates use ISO 8601 format. Invalid requests return HTTP 400. Unexpected errors return HTTP 500.

## Structure

- `Controllers/`: HTTP endpoints.
- `Services/Licence/`: validation, licence creation, and signing.
- `Dtos/` and `Models/`: request, response, and token data.
- `Configuration/`: application settings and signing key loading.
- `Serialization/`: JSON serialization settings.
- `Errors/`: error handling.

Licences are signed with RS256 and can be verified using the matching public key. Token contents are readable, not encrypted. The API does not store licences or manage activation.

## Perpetual licences

Set `licenceType` to `"Perpetual"` to issue a licence without expiry. The signed claims contain `licenceType: "Perpetual"` and `expireAtUtc: null`.
