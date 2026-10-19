# InnovaTEST Licence Generator API

ASP.NET Core 10 API for issuing offline RS256 JWT licences. The existing `LicenceIssuer` still validates the request, creates claims and signs the JWT. Each call returns a new licence; no output file or licence database is written on the server.

## Run

From the repository root:

```powershell
dotnet build InnovaTest.LicenceGenerator.slnx -c Release
dotnet run --project src
```

The API listens on `http://localhost:5050` by default. Configure `LicenceGenerator:PrivateKeyPath` in the root `appsettings.json`. Relative key paths resolve from the working directory where you start the API. Configuration is copied beside the executable during build/publish; edit that copy for a deployed executable. ASP.NET Core environment overrides are supported, e.g. `LicenceGenerator__PrivateKeyPath` and `ASPNETCORE_URLS`. Missing, invalid, undersized or public-only keys prevent startup. Restart to load a changed key.

The local `keys/` directory already contains a generated key pair. Keep the private key only on the issuing server, protected by OS file permissions; it is an unencrypted PEM. Ship the public key with the offline application. Never send the private key in API requests or include it in source control.

## Issue a licence

`POST /api/licences` accepts the JSON contents of `examples/request.json` in the HTTP body with `Content-Type: application/json` (no multipart upload).

```powershell
curl.exe --request POST http://localhost:5050/api/licences --header "Content-Type: application/json" --data-binary "@examples/request.json" --output licence.json
```

```json
{
  "deviceId": "INNOVATEST-SERIAL-12345",
  "enabledOptions": ["hardness-testing", "report-export"],
  "expiresAtUtc": "2027-12-31T23:59:59Z",
  "maxConcurrentSessions": 2
}
```

Success returns HTTP 200 with `{ "token": "<signed JWT>" }`. Save the response as the licence JSON on the client. Malformed JSON, missing fields, unknown fields, invalid options/session limits/device IDs, and past expiry return HTTP 400 problem details. Other failures return generic HTTP 500 problem details and are logged server-side. Requests are limited to 64 KiB. Responses use `Cache-Control: no-store`. All four fields are required; use explicit UTC dates with `Z`.

## Deployment and future payments

This initial endpoint is for trusted local use and has no authentication or payment integration. Before exposing it online, add authentication and authorization, HTTPS at the host or reverse proxy, and rate limits. Configure `AllowedHosts` and listening URLs for your deployment. A customer-facing payment flow must derive allowed options, expiry, device and session limits from a verified paid order on the server, rather than allowing customers to choose their own entitlements. Add order persistence and idempotency before automated payment fulfillment. The present endpoint issues a fresh licence on every successful call.

Offline licence validation belongs in the desktop application: verify the signature with its trusted public key, issuer/audience/version, dates and actual device identity, and enforce feature and session limits. Signed JWTs are readable and resist claim editing; offline expiry still depends on trustworthy local time.

## Verification

After a Release build, run `python tests/api_smoke.py`. The standard-library-only script starts a temporary local API process and checks successful and concurrent issuance, signed claim contents, malformed/invalid input, problem details, request size, content type and HTTP method handling. It stops the process afterward. It uses the configured local signing key.
