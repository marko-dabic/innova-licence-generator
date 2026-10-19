# InnovaTEST offline licensing

.NET 10 console generator and a separate public-key validation library. No external packages or network services are required at runtime. Options use case-sensitive string identifiers so the product can define its actual feature catalogue without changing the licence format.

## Quick start

A local key pair has already been generated in `keys/` (excluded from Git), and `example-licence.json` was issued for the example serial. Use those keys for further local trials; run the `keys` command below only when creating a new key pair in a new directory. Set the real serial and feature names before issuing customer licences.

Run from this repository directory in PowerShell:

```powershell
dotnet build InnovaTest.Licensing.slnx -c Release
dotnet run --project src/InnovaTest.LicenceGenerator -- keys keys
# Edit examples/request.json with the real device serial, features and expiry.
dotnet run --project src/InnovaTest.LicenceGenerator -- issue examples/request.json keys/licence.private.pem licence.json
dotnet run --project src/InnovaTest.LicenceGenerator -- validate licence.json keys/licence.public.pem INNOVATEST-SERIAL-12345
```

The output is JSON containing one `token` property: a standard compact RS256 JWT. All claims (including enabled options, device ID, maximum concurrent sessions and expiry) are inside the signed JWT. Changing any signed data invalidates the signature. JWT signing does not encrypt the contents; customers can read them. Expiry is an exact UTC instant, exclusive, with no grace period. `iat`, `nbf` and `exp` use Unix seconds. Always provide an explicit timezone in requests (use `Z` for UTC as in the example); a date without an offset is interpreted using the issuing computer?s local timezone.

Commands return 0 on success and 1 on invalid input or failed validation. Keys and output files are created without overwriting existing files. Create the output parent directory beforehand. No production key is supplied or embedded in the source.

## Private and public keys

Run `keys` once on the issuing computer. Keep `keys/licence.private.pem` only there, protect it with OS file permissions, and keep an access-controlled offline backup. The PEM is unencrypted, so restrict access to the issuing account. Never ship the generator, its private PEM, or a whole copy of this repository to a customer. Private keys are generated explicitly, never regenerated silently. Losing the key prevents issuing licences for its deployed public key; exposing it allows forged licences. The ignore rules exclude the default keys directory and private-key filename convention; they do not replace access controls.

Ship only `InnovaTest.Licensing.dll` and `licence.public.pem` with the desktop application. Embed the public PEM as a build resource or otherwise protect its deployment from replacement; do not accept a public key from the licence file or customer input. No signing code is present in the validation library. The generator uses the built-in .NET RSA PEM/signature APIs: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa

## Desktop backend integration

Reference `src/InnovaTest.Licensing/InnovaTest.Licensing.csproj` from the backend (or package that library). This repository does not modify the existing InnovaTEST Fork application.

```csharp
using System.Text.Json;
using InnovaTest.Licensing;

var document = JsonSerializer.Deserialize<LicenceDocument>(
    File.ReadAllText(licencePath), LicenceJson.Options)
    ?? throw new InvalidOperationException("Missing licence.");
ILicenceValidator validator = new RsaLicenceValidator();
var claims = validator.Validate(document, trustedPublicPem, connectedDeviceSerial);
if (!claims.Enables("report-export"))
    throw new InvalidOperationException("Report export is not licensed.");
```

Read `connectedDeviceSerial` from the actual testing hardware or another trusted, stable hardware identity. A textbox, config value or value copied from the licence does not bind the licence to physical hardware. Device IDs are compared exactly and case-sensitively. Spoofable hardware identifiers need stronger device authentication to resist impersonation.

Validate before opening a session, before protected device commands and feature operations, and periodically during long-running sessions so expiry takes effect while running. Fail closed on missing/invalid licences and catch `LicenceValidationException` (plus file/JSON errors when loading). Bound input file size before loading untrusted files; the validator itself bounds the token to 64 KiB. Check permissions in the backend, not only by hiding UI controls. Cache verified claims only until their expiry and revalidate on device changes.

`MaxConcurrentSessions` is signed and validated as positive. It is a policy value: the host application must atomically reserve/release sessions and reject reservations at the limit. Define a session as an authenticated operator/device-control connection; avoid using an untrusted client-supplied session count. Use one authority in the local backend for all UI clients. If multiple backend processes can run, coordinate reservations across them using an OS-wide lock/shared store or prevent multiple instances. Independent offline computers cannot enforce a shared global session limit without coordination. Runtime session accounting is intentionally not implemented in this generator.

A signed offline licence prevents editing expiry, but relies on the local clock and application integrity. Administrators can roll back the clock or patch the application. A protected last-seen UTC value can detect some rollbacks; strong guarantees require trusted hardware time/state or periodic online validation. Offline revocation and global concurrent-use tracking likewise require additional infrastructure. Plan key rotation as an application update with trusted public keys; this initial format uses one public key per validator call.

## Checks

```powershell
dotnet run --project tests/InnovaTest.Licensing.Tests -c Release
```

This dependency-free executable test harness checks valid JSON/JWT round trips, feature lookup, expiry boundaries, future issuance, wrong device/key, edited claims, invalid session counts, duplicate options, malformed tokens and algorithm substitution. It exits nonzero on failure; use this command rather than `dotnet test`.
