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
