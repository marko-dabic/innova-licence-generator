# InnovaTEST Licence Generator

A standalone .NET 10 console application for creating RSA keys and issuing signed offline licences. No external packages are required.

## Usage

Run from the repository directory:

```powershell
dotnet build InnovaTest.LicenceGenerator.slnx -c Release
# Create keys once in a new directory:
dotnet run --project src -- keys keys
# Edit the request with the actual device serial, options and expiry:
dotnet run --project src -- issue examples/request.json keys/licence.private.pem licence.json
```

The local `keys/` directory already contains a generated key pair, so skip key creation when using that pair. `example-licence.json` contains a previously issued sample. Existing keys and output files are never overwritten. Output parent directories must exist. Commands exit with code 0 on success and 1 on invalid input or failure.

## Request and output

`examples/request.json` specifies `deviceId`, `enabledOptions`, `expiresAtUtc`, and `maxConcurrentSessions`. Device IDs must be non-empty without surrounding whitespace; option names must be non-empty and unique. Session limits must be positive and expiry must be in the future. Always use an explicit timezone, preferably UTC (`Z`), for expiry. Option names are case-sensitive product-defined strings.

The output JSON contains a `token` property holding a compact RS256 JWT. Its signed claims include device ID, options, expiry, maximum sessions, licence ID, issue time, not-before time, issuer, audience and format version. Timestamps use Unix seconds. Signing detects edits; the contents remain readable.

## Keys

Keep `licence.private.pem` exclusively on the issuing computer, restrict file access to the issuing account, and maintain a secure backup. It is an unencrypted PEM file. Keys are excluded from Git by the default directory and private-key filename rules. Never distribute the private key or this generator to customers.

The corresponding `licence.public.pem` is intended for the desktop application. Licence validation and enforcement belong in that application and are not implemented in this repository. The application must verify the RS256 signature using its trusted public key, check issuer/audience/version and dates, compare the licence device ID with the connected hardware identity, and enforce options and concurrent sessions. Offline expiry depends on trustworthy local time.
