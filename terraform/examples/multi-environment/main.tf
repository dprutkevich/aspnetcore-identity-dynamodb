# Multi-Environment Setup Example
# This example shows how to deploy UMS Identity to multiple environments

terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Configure the AWS Provider
provider "aws" {
  region = var.aws_region
}

# Variables
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-west-2"
}

variable "environments" {
  description = "List of environments to deploy"
  type = map(object({
    billing_mode          = string
    users_read_capacity   = optional(number, 5)
    users_write_capacity  = optional(number, 5)
    tokens_read_capacity  = optional(number, 5)
    tokens_write_capacity = optional(number, 5)
  }))
  default = {
    dev = {
      billing_mode = "PAY_PER_REQUEST"
    }
    staging = {
      billing_mode          = "PROVISIONED"
      users_read_capacity   = 10
      users_write_capacity  = 5
      tokens_read_capacity  = 10
      tokens_write_capacity = 5
    }
    prod = {
      billing_mode          = "PROVISIONED"
      users_read_capacity   = 50
      users_write_capacity  = 25
      tokens_read_capacity  = 30
      tokens_write_capacity = 20
    }
  }
}

# Deploy to each environment
module "ums_identity" {
  source   = "git::https://ums-projects@dev.azure.com/ums-projects/Unified%20microservices/_git/ums-identity-dynamodb//terraform"
  for_each = var.environments

  # Environment-specific configuration
  environment = each.key

  # Table names with environment prefix
  users_table_name            = "ums-${each.key}-identity-users"
  tokens_table_name           = "ums-${each.key}-identity-tokens"
  temporary_tokens_table_name = "ums-${each.key}-identity-temporary-tokens"
  user_roles_table_name       = "ums-${each.key}-identity-user-roles"

  # Billing configuration
  billing_mode = each.value.billing_mode

  # Capacity settings (only used when billing_mode = "PROVISIONED")
  users_read_capacity  = each.value.users_read_capacity
  users_write_capacity = each.value.users_write_capacity

  tokens_read_capacity  = each.value.tokens_read_capacity
  tokens_write_capacity = each.value.tokens_write_capacity

  # Set capacity for other tables based on users table
  temporary_tokens_read_capacity  = max(3, floor(each.value.users_read_capacity * 0.3))
  temporary_tokens_write_capacity = max(3, floor(each.value.users_write_capacity * 0.3))

  user_roles_read_capacity  = max(3, floor(each.value.users_read_capacity * 0.5))
  user_roles_write_capacity = max(3, floor(each.value.users_write_capacity * 0.5))

  # Security settings (enable for all environments)
  enable_point_in_time_recovery = true
  enable_encryption             = true

  # Environment-specific tags
  common_tags = {
    Environment = each.key
    Project     = "UMS"
    Component   = "Identity"
    Owner       = each.key == "prod" ? "Platform Team" : "Development Team"
    CostCenter  = "Engineering"
    Backup      = each.key == "prod" ? "required" : "optional"
  }
}

# Create IAM roles for each environment
resource "aws_iam_role" "app_role" {
  for_each = var.environments
  name     = "ums-${each.key}-identity-app-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Environment = each.key
    Project     = "UMS"
    Component   = "Identity"
  }
}

# Attach DynamoDB policies
resource "aws_iam_role_policy" "app_dynamodb_access" {
  for_each = var.environments
  name     = "dynamodb-access"
  role     = aws_iam_role.app_role[each.key].id
  policy   = module.ums_identity[each.key].identity_dynamodb_policy
}

# Generate configuration files for each environment
resource "local_file" "appsettings" {
  for_each = var.environments

  content = jsonencode({
    Identity = merge(module.ums_identity[each.key].identity_configuration, {
      Jwt = {
        Secret                     = "placeholder-use-parameter-store-for-${each.key}"
        Issuer                     = "ums-identity-${each.key}"
        Audience                   = "ums-users"
        AccessTokenLifetimeMinutes = each.key == "prod" ? 15 : 60
      }
      SendWelcomeEmail = true
    })
    Logging = {
      LogLevel = {
        Default                = each.key == "prod" ? "Warning" : "Information"
        "Microsoft.AspNetCore" = "Warning"
        "Identity.DynamoDb"    = each.key == "prod" ? "Warning" : "Information"
      }
    }
  })

  filename = "${path.module}/appsettings.${each.key}.json"
}

# Outputs
output "environments" {
  description = "Configuration for all environments"
  value = {
    for env, config in var.environments : env => {
      table_names   = module.ums_identity[env].all_table_names
      table_arns    = module.ums_identity[env].all_table_arns
      iam_role_arn  = aws_iam_role.app_role[env].arn
      configuration = module.ums_identity[env].identity_configuration
    }
  }
}

output "environment_summary" {
  description = "Summary of deployed environments"
  value = {
    for env in keys(var.environments) : env => {
      users_table  = module.ums_identity[env].users_table_name
      billing_mode = var.environments[env].billing_mode
      iam_role     = aws_iam_role.app_role[env].name
    }
  }
}
