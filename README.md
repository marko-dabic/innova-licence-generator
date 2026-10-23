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
  "isActivateOnImport": false,
  "validityInMonths": 6,
  "maxConcurrentSessions": 2
}
```

`deviceId`, `enabledOptions`, and `isActivateOnImport` are required. `validityInMonths` defaults to 6 and `maxConcurrentSessions` defaults to 1; both must be positive.

A successful response returns HTTP 200:

```json
{
  "token": "<signed JWT>"
}
```

The token contains the licence ID, issuer, audience, device ID, enabled options, validity, session limit, and UTC dates:

- `issuedAtUtc`: the time the licence was generated.
- `expireAtUtc`: `null` when `isActivateOnImport` is true (expiry is deferred to import); otherwise `issuedAtUtc` plus `validityInMonths`.

Dates use ISO 8601 format. Invalid requests return HTTP 400. Unexpected errors return HTTP 500.

## Structure

- `Controllers/`: HTTP endpoints.
- `Services/Licence/`: validation, licence creation, and signing.
- `Dtos/` and `Models/`: request, response, and token data.
- `Configuration/`: application settings and signing key loading.
- `Serialization/`: JSON serialization settings.
- `Errors/`: error handling.

Licences are signed with RS256 and can be verified using the matching public key. Token contents are readable, not encrypted. The API does not store licences or manage activation. Authentication is not currently implemented.
