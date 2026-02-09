# Production Environment Example
# This example shows a production-ready configuration with high availability and monitoring

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

  default_tags {
    tags = {
      Environment = "prod"
      Project     = "UMS"
      Component   = "Identity"
      ManagedBy   = "Terraform"
      CostCenter  = "Engineering"
    }
  }
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
}

# Use the UMS Identity module with production configuration
module "ums_identity" {
  source = "git::https://ums-projects@dev.azure.com/ums-projects/Unified%20microservices/_git/ums-identity-dynamodb//terraform?ref=v1.0.0"

  # Environment-specific table names
  users_table_name            = "ums-${var.environment}-identity-users"
  tokens_table_name           = "ums-${var.environment}-identity-tokens"
  temporary_tokens_table_name = "ums-${var.environment}-identity-temporary-tokens"
  user_roles_table_name       = "ums-${var.environment}-identity-user-roles"

  # Use provisioned billing for predictable costs in production
  billing_mode = "PROVISIONED"

  # Production capacity settings
  users_read_capacity  = 50
  users_write_capacity = 25

  tokens_read_capacity  = 30
  tokens_write_capacity = 20

  temporary_tokens_read_capacity  = 10
  temporary_tokens_write_capacity = 10

  user_roles_read_capacity  = 20
  user_roles_write_capacity = 10

  # Enable all security features
  enable_point_in_time_recovery = true
  enable_encryption             = true

  # Production tags
  common_tags = {
    Environment = var.environment
    Project     = "UMS"
    Component   = "Identity"
    Owner       = "Platform Team"
    CostCenter  = "Engineering"
    Backup      = "required"
    Monitoring  = "enabled"
  }
}

# CloudWatch Alarms for monitoring
resource "aws_cloudwatch_metric_alarm" "users_table_read_throttle" {
  alarm_name          = "ums-${var.environment}-users-read-throttle"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "ReadThrottledEvents"
  namespace           = "AWS/DynamoDB"
  period              = "300"
  statistic           = "Sum"
  threshold           = "0"
  alarm_description   = "Users table read throttle events"
  treat_missing_data  = "notBreaching"

  dimensions = {
    TableName = module.ums_identity.users_table_name
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
}

resource "aws_cloudwatch_metric_alarm" "users_table_write_throttle" {
  alarm_name          = "ums-${var.environment}-users-write-throttle"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "WriteThrottledEvents"
  namespace           = "AWS/DynamoDB"
  period              = "300"
  statistic           = "Sum"
  threshold           = "0"
  alarm_description   = "Users table write throttle events"
  treat_missing_data  = "notBreaching"

  dimensions = {
    TableName = module.ums_identity.users_table_name
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
}

# SNS topic for alerts
resource "aws_sns_topic" "alerts" {
  name = "ums-${var.environment}-identity-alerts"
}

# IAM role for ECS tasks
resource "aws_iam_role" "ecs_task_role" {
  name = "ums-${var.environment}-identity-ecs-task-role"

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
}

# Attach DynamoDB policy
resource "aws_iam_role_policy" "ecs_dynamodb_access" {
  name   = "dynamodb-access"
  role   = aws_iam_role.ecs_task_role.id
  policy = module.ums_identity.identity_dynamodb_policy
}

# Additional policy for CloudWatch logging
resource "aws_iam_role_policy" "ecs_logging" {
  name = "cloudwatch-logging"
  role = aws_iam_role.ecs_task_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents",
          "logs:DescribeLogStreams"
        ]
        Resource = "*"
      }
    ]
  })
}

# VPC Endpoint for DynamoDB (optional, for private access)
data "aws_vpc" "default" {
  default = true
}

data "aws_route_tables" "default" {
  vpc_id = data.aws_vpc.default.id
}

resource "aws_vpc_endpoint" "dynamodb" {
  vpc_id          = data.aws_vpc.default.id
  service_name    = "com.amazonaws.${var.aws_region}.dynamodb"
  route_table_ids = data.aws_route_tables.default.ids

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = "*"
        Action = [
          "dynamodb:BatchGetItem",
          "dynamodb:BatchWriteItem",
          "dynamodb:DeleteItem",
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:UpdateItem"
        ]
        Resource = module.ums_identity.all_table_arns
      }
    ]
  })

  tags = {
    Name = "ums-${var.environment}-dynamodb-endpoint"
  }
}

# Outputs
output "table_names" {
  description = "All DynamoDB table names"
  value       = module.ums_identity.all_table_names
}

output "table_arns" {
  description = "All DynamoDB table ARNs"
  value       = module.ums_identity.all_table_arns
}

output "ecs_task_role_arn" {
  description = "ECS task role ARN for the application"
  value       = aws_iam_role.ecs_task_role.arn
}

output "configuration_for_dotnet" {
  description = "Configuration object for .NET application"
  value       = module.ums_identity.identity_configuration
}

# Generate application configuration
resource "local_file" "appsettings_production" {
  content = jsonencode({
    Identity = merge(module.ums_identity.identity_configuration, {
      Jwt = {
        # NOTE: Use AWS Systems Manager Parameter Store or AWS Secrets Manager for secrets
        Secret                     = "placeholder-use-parameter-store"
        Issuer                     = "ums-identity-${var.environment}"
        Audience                   = "ums-users"
        AccessTokenLifetimeMinutes = 15
      }
      SendWelcomeEmail = true
    })
    Logging = {
      LogLevel = {
        Default                = "Information"
        "Microsoft.AspNetCore" = "Warning"
        "Identity.DynamoDb"    = "Information"
      }
    }
    AllowedHosts = "*"
  })
  filename = "${path.module}/appsettings.${var.environment}.json"
}
