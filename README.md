# InnovaTEST Licence Generator

ASP.NET Core API that generates RSA-signed JWT licences.

## Run

Requires the .NET 10 SDK, Docker Desktop running Linux containers, and an RSA private key in PEM format (at least 3072 bits).

Set `LicenceGenerator:PrivateKeyPath` in `appsettings.json`. The default is `keys/licence.private.pem`, relative to the project root. Keep the private key on the licence generation server. Restart the API after changing the key.

Open Docker Desktop in Linux containers mode. From the `innova-licence-generator` directory,
create your local database configuration once:

```powershell
Copy-Item .env.example .env
```

Edit `.env` and replace the example `MSSQL_SA_PASSWORD` with your own strong password.
This file is excluded from Git. Use unquoted `KEY=value` entries and keep the password on one line
without `$` or `;` characters. SQL Server requires at least 8 characters from at least three categories:
uppercase letters, lowercase letters, digits, and symbols.
Do not overwrite an existing `.env`; it contains the credentials for your existing database.

Start the database:

```powershell
docker compose up -d --wait
```

The first run downloads SQL Server 2022 Express. The command waits until the database is healthy.
You can view its status with `docker compose ps` and its logs with `docker compose logs sqlserver`.

Start the API from PowerShell or Git Bash:

```powershell
dotnet run --project src/InnovaTest.LicenceGenerator.Presentation
```

API: `http://localhost:5050`  
Swagger: `http://localhost:5050/swagger`

The API creates the `InnovaLicences` database and applies EF Core migrations before accepting requests.
SQL Server listens only on `localhost:1433`. The API runs on Windows, outside Docker.

Database data is stored in the Docker named volume `innova-licence-generator_sqlserver-data`.
`docker compose stop` stops the database; `docker compose up -d --wait` starts it again.
`docker compose down` removes the container while preserving the volume.
Do not use `docker compose down -v` unless you intend to delete the database and its activation records.
Keep database backups; a Docker volume is persistent storage, not a backup.

To inspect the database in SQL Server Management Studio, connect to `tcp:127.0.0.1,1433`
using SQL Server Authentication, user `sa`, and the password in `.env`.
Enable **Trust server certificate** for this local container.

Docker Compose and the API both read the project root `.env` file automatically, including when
launching the API from Visual Studio. The API reads the file at startup; restart it after changes.
Environment variables and command-line arguments override file values. If you previously exported
`MSSQL_SA_PASSWORD`, remove that override with `unset MSSQL_SA_PASSWORD` in Git Bash or
`Remove-Item Env:MSSQL_SA_PASSWORD` in PowerShell to use the file instead.
The file is optional, so deployments can supply environment variables without a `.env` file.
You can alternatively supply the entire connection string through `ConnectionStrings__LicenceDatabase`.
Changing `.env` after the database has been initialized does not change SQL Server's existing password.
The local setup uses `sa` and a trusted self-signed certificate for convenience. For deployment,
use a dedicated database account, managed secrets, and a validated server certificate.

Schema migrations are committed under `src/InnovaTest.LicenceGenerator.Infrastructure/Persistence/Migrations`. To add future migrations:

```powershell
dotnet tool install dotnet-ef --tool-path .tools --version 10.0.14
.\.tools\dotnet-ef migrations add YourMigrationName --project src/InnovaTest.LicenceGenerator.Infrastructure --startup-project src/InnovaTest.LicenceGenerator.Presentation --output-dir Persistence/Migrations
```

## Generate a licence

Send JSON to `POST /licences/generate-licence`:

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
  "validityInMonths": 6,
  "maxConcurrentSessions": 2
}
```

`deviceId`, `enabledOptions`, `maxConcurrentSessions`, and the string discriminator `licenceType` are required. `maxConcurrentSessions` must be positive. This replaces the previous request format with independent licence flags.

| `licenceType` | Request DTO | Expiry |
| --- | --- | --- |
| `Standard` | `StandardLicenceDto` | Generation time plus `validityInMonths` (required positive integer). |
| `ActivateOnImport` | `ActivateOnImportLicenceDto` | Calculated by the importing application using `validityInMonths` (required positive integer). |
| `Perpetual` | `PerpetualLicenceDto` | No expiry. |

All three sealed DTOs inherit `BaseLicenceRequestDto`. Its `LicenceType` enum property is determined by the subtype; JSON uses `licenceType` to select that subtype. The import and perpetual DTOs expose read-only `IsActivateOnImport` and `IsPerpetual` flags respectively, both always `true`; callers can omit them.

Standard and activate-on-import requests require `"validityInMonths"` greater than zero, for example `"validityInMonths": 12`. For an activate-on-import request, use `"licenceType": "ActivateOnImport"`. For a perpetual request, use `"licenceType": "Perpetual"` and omit validity. Fields belonging to another subtype are rejected.

A successful response returns HTTP 200:

```json
{
  "licence": "<signed JWT>"
}
```

Every successful generation is saved to SQL Server's `GeneratedLicences` table before the API returns
HTTP 200. It stores `LicenceId` (primary key), `DeviceId`, `LicenceType` (as a string), `Licence`
(the exact signed JWT returned to the caller), and `GeneratedAtUtc` (matching the JWT's `issuedAtUtc`).
All three licence types are recorded. Database failures fail the request. Existing licences generated
before this feature are not backfilled. Each generation request creates a new licence and record;
retrying a request can generate another licence. Activation does not overwrite the generated token.

The `licence` property contains the signed JWT with the licence ID, issuer, audience, device ID, enabled options, validity, session limit, and UTC dates:

- `issuedAtUtc`: the time the licence was generated.
- `expireAtUtc`: `issuedAtUtc` plus the requested `validityInMonths` for standard licences; `null` for activate-on-import and perpetual licences.
- `licenceType`: identifies the licence behavior (`Standard`, `ActivateOnImport`, or `Perpetual`). The signed claims do not include the redundant `isActivateOnImport` and `isPerpetual` flags.
- `validityInMonths`: the import duration for activate-on-import licences; `null` otherwise.

Dates use ISO 8601 format. Invalid requests return HTTP 400. Unexpected errors return HTTP 500.

## Activate a licence

Send a generated ActivateOnImport licence to `POST /licences/activate-licence`:

```json
{
  "licence": "<original signed JWT>"
}
```

The server verifies the RS256 signature with its configured signing key, validates the issuer and audience,
and accepts only ActivateOnImport licences with a positive `validityInMonths`.
It returns HTTP 200 with `{ "licence": "<activated signed JWT>" }`.
Only `expireAtUtc` changes: it becomes the server's current UTC time plus `validityInMonths` calendar months.
For example, activation on 29 October 2026 for one month expires on 29 November 2026 at the same UTC time.
The licence ID, generation time, type, validity, device, options, and session limit stay the same.
Malformed, modified, incorrectly signed, Standard, and Perpetual licences return HTTP 400.

The first activated token is stored in SQL Server's `LicenceActivations` table, together with the original
licence ID, activation time, and expiry. A SHA-256 hash of the licence ID is the primary key, preserving
the previous store's exact, case-sensitive licence ID lookup without limiting ID length.
Only the first activation succeeds. Concurrent attempts for the same licence ID return HTTP 409
except for the first successful database insert. Resubmitting the original token or submitting an
already activated token returns HTTP 409, regardless of whether its expiry is past, present, or future.
The ProblemDetails response includes `code: "licence_already_activated"`. Expiry is never extended.
If the first response is lost, retrying also returns 409; clients should retain a successful activation response.
Database failures fail the request;
there is no filesystem fallback. Multiple API instances must use the same database.
Deleting activation records allows the original unactivated tokens to be activated again.

Activation requires access to the server (directly or through another connected computer that transfers
the token to the offline machine). The importing application must verify the new token's signature and
enforce its `expireAtUtc`. Server-side activation fixes the initial expiry calculation; protecting offline
expiry checks against subsequent local clock rollback remains the importing application's responsibility.

## Structure

```text
InnovaTest.LicenceGenerator.slnx
src/
  InnovaTest.LicenceGenerator.Presentation/    # HTTP endpoints, Swagger, error mapping, startup
  InnovaTest.LicenceGenerator.Application/     # Generation/activation use cases, DTOs, interfaces
  InnovaTest.LicenceGenerator.Domain/          # Licence models and licence types
  InnovaTest.LicenceGenerator.Infrastructure/  # SQL Server, EF migrations, JWT/RSA, key loading
```

Project references enforce the dependency direction:

- Domain has no project or package dependencies.
- Application references Domain only.
- Infrastructure references Application (and uses its Domain types).
- Presentation references Application and Infrastructure, and wires implementations at startup.

Keep business workflows in `Application/Features`, storage and token contracts in Application,
and technical implementations in Infrastructure. `LicenceActivation` and `GeneratedLicence` live in `Domain/Models`. The activation lookup hash
is an EF shadow property configured and populated by Infrastructure, preserving the existing table.
Domain types have no ASP.NET Core, EF Core,
or JSON annotations. Token serialization conventions belong to Infrastructure. Application DTOs
retain their JSON and input-validation annotations to preserve the existing public API contract.
The Domain project is deliberately small until additional domain behavior warrants richer models.

`ILicenceTokenCodec` hides RSA and JSON payloads from the use cases. Its verified-token contract
exposes claims, while Infrastructure preserves the original payload when changing activation expiry.
`ILicenceActivationStore.CreateAsync` atomically creates an activation and rejects duplicates; SQL Server
unique-key handling stays in Infrastructure. Existing migration IDs and table mappings are unchanged.

Select Presentation as the startup project in Visual Studio. The solution-root `appsettings.json`,
`.env`, and `keys/` locations stay the same; published output includes `appsettings.json`.

Build from the project root:

```powershell
dotnet build InnovaTest.LicenceGenerator.slnx
```

Licences are signed with RS256 and can be verified using the matching public key. Token contents are readable, not encrypted. The API stores the first activation result to prevent repeat activation from extending expiry.

## Perpetual licences

Set `licenceType` to `"Perpetual"` to generate a licence without expiry. The signed claims contain `licenceType: "Perpetual"` and `expireAtUtc: null`.
