# InnovaTEST Licence Generator

A standalone .NET 10 console application for issuing signed offline licences. Uses the standard Microsoft.Extensions.Configuration JSON provider and configuration binder.

## Usage

Run from the repository directory:

```powershell
dotnet build InnovaTest.LicenceGenerator.slnx -c Release
# Edit appsettings.json paths and examples/request.json, then run:
dotnet run --project src
```

The local `keys/` directory already contains a generated key pair. The application uses an existing private key; it does not generate keys. `example-licence.json` contains a previously issued sample. Existing output files are never overwritten. Output parent directories must exist. The application exits with code 0 on success and 1 on invalid input or failure.

## Configuration

Edit `appsettings.json` in the repository root:

```json
{
  "LicenceGenerator": {
    "RequestPath": "examples/request.json",
    "PrivateKeyPath": "keys/licence.private.pem",
    "OutputPath": "licence.json"
  }
}
```

.NET reads the JSON and binds the `LicenceGenerator` section to `GeneratorOptions`. All three paths are required and checked before issuance. Only the private key path is configured here; the key remains in its PEM file.

The configuration file is copied beside the executable during build and publish. Relative paths resolve from the working directory, so run the commands above from the repository root. For a published application, edit the appsettings.json beside the executable and use absolute paths or run from the directory containing the configured input files. Rebuild after changing the root configuration when using compiled output directly. Existing environment files are no longer used.

## Request and output

`examples/request.json` specifies `deviceId`, `enabledOptions`, `expiresAtUtc`, and `maxConcurrentSessions`. Device IDs must be non-empty without surrounding whitespace; option names must be non-empty and unique. Session limits must be positive and expiry must be in the future. Always use an explicit timezone, preferably UTC (`Z`), for expiry. Option names are case-sensitive product-defined strings.

The output JSON contains a `token` property holding a compact RS256 JWT. Its signed claims include device ID, options, expiry, maximum sessions, licence ID, issue time, not-before time, issuer, audience and format version. Timestamps use Unix seconds. Signing detects edits; the contents remain readable.

## Keys

Keep `licence.private.pem` exclusively on the issuing computer, restrict file access to the issuing account, and maintain a secure backup. It is an unencrypted PEM file. Keys are excluded from Git by the default directory and private-key filename rules. Never distribute the private key or this generator to customers.

The corresponding `licence.public.pem` is intended for the desktop application. Licence validation and enforcement belong in that application and are not implemented in this repository. The application must verify the RS256 signature using its trusted public key, check issuer/audience/version and dates, compare the licence device ID with the connected hardware identity, and enforce options and concurrent sessions. Offline expiry depends on trustworthy local time.
