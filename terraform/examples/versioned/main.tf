# Versioned Remote Module Example
# This example shows how to use the UMS Identity module with version pinning for production

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

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "prod"
  
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod."
  }
}

variable "module_version" {
  description = "Version of the UMS Identity module to use"
  type        = string
  default     = "v1.0.0"
}

# Use the UMS Identity module with version pinning
module "ums_identity" {
  source = "git::https://ums-projects@dev.azure.com/ums-projects/Unified%20microservices/_git/ums-identity-dynamodb//terraform?ref=${var.module_version}"
  
  # Environment configuration
  environment = var.environment
  
  # Environment-specific table names
  users_table_name             = "ums-${var.environment}-identity-users"
  tokens_table_name            = "ums-${var.environment}-identity-tokens"
  temporary_tokens_table_name  = "ums-${var.environment}-identity-temporary-tokens"
  user_roles_table_name        = "ums-${var.environment}-identity-user-roles"
  
  # Production settings
  billing_mode = var.environment == "prod" ? "PROVISIONED" : "PAY_PER_REQUEST"
  
  # Capacity settings (only for production)
  users_read_capacity  = var.environment == "prod" ? 100 : 10
  users_write_capacity = var.environment == "prod" ? 50 : 5
  
  tokens_read_capacity  = var.environment == "prod" ? 60 : 8
  tokens_write_capacity = var.environment == "prod" ? 30 : 4
  
  temporary_tokens_read_capacity  = var.environment == "prod" ? 20 : 3
  temporary_tokens_write_capacity = var.environment == "prod" ? 15 : 3
  
  user_roles_read_capacity  = var.environment == "prod" ? 30 : 5
  user_roles_write_capacity = var.environment == "prod" ? 20 : 3
  
  # Security settings (always enabled)
  enable_point_in_time_recovery = true
  enable_encryption             = true
  
  # Environment-specific tags
  common_tags = {
    Environment = var.environment
    Project     = "UMS"
    Component   = "Identity"
    ManagedBy   = "Terraform"
    ModuleVersion = var.module_version
    Repository  = "ums-identity-dynamodb"
  }
}

# Create application configuration file
resource "local_file" "app_config" {
  content = jsonencode({
    Identity = merge(module.ums_identity.identity_configuration, {
      Jwt = {
        # Use AWS Systems Manager Parameter Store for production secrets
        Secret = var.environment == "prod" ? "{{resolve:ssm-secure:/ums/${var.environment}/identity/jwt-secret}}" : "dev-secret-key-at-least-32-characters-long"
        Issuer                     = "ums-identity-${var.environment}"
        Audience                   = "ums-${var.environment}-users"
        AccessTokenLifetimeMinutes = var.environment == "prod" ? 15 : 60
      }
      SendWelcomeEmail = true
    })
    Logging = {
      LogLevel = {
        Default               = var.environment == "prod" ? "Warning" : "Information"
        "Microsoft.AspNetCore" = "Warning"
        "Identity.DynamoDb"   = var.environment == "prod" ? "Warning" : "Debug"
      }
    }
    ConnectionStrings = {
      # If using RDS for other data
      # DefaultConnection = "Server=..."
    }
  })
  
  filename = "${path.module}/appsettings.${var.environment}.json"
}

# Outputs
output "module_version_used" {
  description = "Version of the UMS Identity module used"
  value       = var.module_version
}

output "table_configuration" {
  description = "DynamoDB table configuration"
  value       = module.ums_identity.identity_configuration
}

output "table_arns" {
  description = "ARNs of all created tables"
  value       = module.ums_identity.all_table_arns
}

output "dynamodb_policy" {
  description = "IAM policy for DynamoDB access"
  value       = module.ums_identity.identity_dynamodb_policy
  sensitive   = false
}

# Example usage commands:
# terraform plan -var="environment=staging" -var="module_version=v1.1.0"
# terraform apply -var="environment=prod" -var="module_version=v1.0.0"
