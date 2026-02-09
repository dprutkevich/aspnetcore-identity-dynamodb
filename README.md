# UMS.Identity.DynamoDb

Identity management library for ASP.NET Core with DynamoDB storage.

## Installation

```bash
dotnet add package UMS.Identity.DynamoDb
```

## Quick Start

```csharp
using Identity.DynamoDb.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUmsIdentity(builder.Configuration);
builder.Services.AddUmsJwtAuthentication();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapUmsIdentityEndpoints();

app.Run();
```

## Configuration

Add this section to `appsettings.json`:

```json
{
  "Identity": {
    "SendWelcomeEmail": true,
    "RequireEmailConfirmation": true,
    "Jwt": {
      "Secret": "your-super-secret-key-must-be-at-least-32-characters-long",
      "Issuer": "your-app-name",
      "Audience": "your-users",
      "AccessTokenLifetimeMinutes": 15,
      "RefreshTokenLifetimeDays": 30
    },
    "DynamoDb": {
      "UsersTable": "your-users-table",
      "TokensTable": "your-tokens-table",
      "TemporaryTokensTable": "your-temporary-tokens-table",
      "UserRolesTable": "your-user-roles-table"
    },
    "Aws": {
      "Region": "us-east-1",
      "UseLocalDynamoDb": false
    },
    "Password": {
      "MinLength": 8,
      "MaxLength": 100,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true,
      "Iterations": 16384
    }
  }
}
```

## Required DynamoDB Schema

The package needs 4 tables.

1. `UsersTable`
- Partition key: `Id` (S)
- GSI: `EmailIndex` with partition key `Email` (S)

2. `TokensTable`
- Partition key: `Id` (S)
- GSIs: `TokenIndex` (`Token`), `UserIdIndex` (`UserId`), `ExpiresAtIndex` (`ExpiresAt`)
- TTL attribute: `ExpiresAt`

3. `TemporaryTokensTable`
- Partition key: `Id` (S)
- GSIs: `UserIdTypeIndex` (`UserId` + `Type`), `TokenTypeIndex` (`Token` + `Type`), `ExpiresAtIndex` (`ExpiresAt`)
- TTL attribute: `ExpiresAt`

4. `UserRolesTable`
- Partition key: `Id` (S)
- GSIs: `UserIdIndex` (`UserId`), `RoleIndex` (`RoleName`), `UserIdRoleIndex` (`UserId` + `RoleName`)

For Terraform module usage see [`terraform/README.md`](terraform/README.md).

## Endpoints

- `POST /api/identity/register`
- `POST /api/identity/login`
- `POST /api/identity/refresh-token`
- `POST /api/identity/logout`
- `GET /api/identity/me`
- `POST /api/identity/change-password`
- `POST /api/identity/forgot-password`
- `POST /api/identity/reset-password`
- `POST /api/identity/send-confirmation-email`
- `POST /api/identity/confirm-email`

## Notes

- Passwords are hashed with BCrypt (`BCrypt.Net-Next`).
- If `RequireEmailConfirmation=true`, login is blocked until email confirmation.
- Register your own `IIdentityNotificationService` to send real emails.

## Links

- [Example Usage](EXAMPLE.md)
- [License](LICENSE)

## Private NuGet (GitHub Packages)

### Publish from this repository

The workflow file is already added:
`/.github/workflows/publish-private-nuget.yml`

It publishes package `UMS.Identity.DynamoDb` to:
`https://nuget.pkg.github.com/dprutkevich/index.json`

How to publish:

1. Bump package version in `/src/Identity.DynamoDb/Identity.DynamoDb.csproj` (or publish by tag with version override).
2. Push tag:
   `git tag v1.3.1 && git push origin v1.3.1`
3. Workflow will pack and push package to GitHub Packages.

### Automatic prerelease from `main`

Added workflow:
`/.github/workflows/publish-private-nuget-prerelease.yml`

On every push to `main` it publishes prerelease package with version:
`<PackageVersion>-ci.<run_number>`

Example:
`1.3.0-ci.42`

If the same commit already has a release tag `v*`, prerelease publish is skipped.

### Use package in another private project

1. Create GitHub PAT (classic) with scopes:
- `read:packages`
- `repo` (required for private repositories)

2. Add source on your machine:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/dprutkevich/index.json" \
  --name "github-dprutkevich" \
  --username "dprutkevich" \
  --password "<YOUR_GITHUB_PAT>" \
  --store-password-in-clear-text
```

3. Install package in any project:

```bash
dotnet add package UMS.Identity.DynamoDb --version 1.3.1 --source "github-dprutkevich"
```

Install prerelease package:

```bash
dotnet add package UMS.Identity.DynamoDb --version 1.3.0-ci.42 --source "github-dprutkevich"
```
