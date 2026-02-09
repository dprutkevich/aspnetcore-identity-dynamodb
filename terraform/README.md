# UMS Identity DynamoDB Terraform Module

Terraform module that creates all DynamoDB tables required by `UMS.Identity.DynamoDb`.

## Created Tables

1. `users`
- Hash key: `Id`
- GSI: `EmailIndex` (`Email`)

2. `tokens`
- Hash key: `Id`
- GSIs: `UserIdIndex` (`UserId`), `TokenIndex` (`Token`), `ExpiresAtIndex` (`ExpiresAt`)
- TTL: `ExpiresAt`

3. `temporary_tokens`
- Hash key: `Id`
- GSIs: `UserIdTypeIndex` (`UserId` + `Type`), `TokenTypeIndex` (`Token` + `Type`), `ExpiresAtIndex` (`ExpiresAt`)
- TTL: `ExpiresAt`

4. `user_roles`
- Hash key: `Id`
- GSIs: `UserIdIndex` (`UserId`), `RoleIndex` (`RoleName`), `UserIdRoleIndex` (`UserId` + `RoleName`)

## Usage

```hcl
module "ums_identity" {
  source = "./terraform"

  environment = "dev"

  users_table_name             = "my-app-users"
  tokens_table_name            = "my-app-tokens"
  temporary_tokens_table_name  = "my-app-temporary-tokens"
  user_roles_table_name        = "my-app-user-roles"

  common_tags = {
    Environment = "dev"
    Project     = "MyApp"
    Component   = "Identity"
  }
}
```

## Outputs

- `users_table_name`
- `tokens_table_name`
- `temporary_tokens_table_name`
- `user_roles_table_name`
- `identity_configuration`
- `identity_dynamodb_policy`

## Deploy

```bash
cd terraform
cp terraform.tfvars.example terraform.tfvars
./deploy.sh apply
```
