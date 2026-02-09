# Example Usage

## Program.cs

```csharp
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddUmsIdentity(builder.Configuration);
builder.Services.AddUmsJwtAuthentication();

builder.Services.AddScoped<IIdentityNotificationService, EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapUmsIdentityEndpoints();

app.MapGet("/api/protected", () => "This is a protected endpoint")
   .RequireAuthorization();

app.Run();

public sealed class EmailService : IIdentityNotificationService
{
    public Task SendEmailConfirmationAsync(Guid userId, string email, string token) => Task.CompletedTask;
    public Task SendPasswordResetAsync(Guid userId, string email, string token) => Task.CompletedTask;
    public Task SendPasswordChangedAsync(Guid userId, string email) => Task.CompletedTask;
    public Task SendWelcomeEmailAsync(Guid userId, string email) => Task.CompletedTask;
}
```

## appsettings.json

```json
{
  "Identity": {
    "SendWelcomeEmail": true,
    "RequireEmailConfirmation": true,
    "Jwt": {
      "Secret": "your-super-secret-jwt-key-that-is-at-least-32-characters-long",
      "Issuer": "YourAppName",
      "Audience": "YourAppUsers",
      "AccessTokenLifetimeMinutes": 15,
      "RefreshTokenLifetimeDays": 30
    },
    "DynamoDb": {
      "UsersTable": "your-app-users",
      "TokensTable": "your-app-refresh-tokens",
      "TemporaryTokensTable": "your-app-temporary-tokens",
      "UserRolesTable": "your-app-user-roles"
    },
    "Aws": {
      "Region": "us-east-1",
      "UseLocalDynamoDb": false
    }
  }
}
```

## Local DynamoDB

```yaml
version: '3.8'
services:
  dynamodb-local:
    image: amazon/dynamodb-local:latest
    ports:
      - "8000:8000"
    command: ["-jar", "DynamoDBLocal.jar", "-sharedDb", "-dbPath", "./data"]
```
