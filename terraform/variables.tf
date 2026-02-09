# Input variables for UMS Identity DynamoDB module

# Table names
variable "users_table_name" {
  description = "Name of the DynamoDB table for users"
  type        = string
  default     = "ums-identity-users"
}

variable "tokens_table_name" {
  description = "Name of the DynamoDB table for refresh tokens"
  type        = string
  default     = "ums-identity-tokens"
}

variable "temporary_tokens_table_name" {
  description = "Name of the DynamoDB table for temporary tokens (email confirmation, password reset)"
  type        = string
  default     = "ums-identity-temporary-tokens"
}

variable "user_roles_table_name" {
  description = "Name of the DynamoDB table for user roles"
  type        = string
  default     = "ums-identity-user-roles"
}

# Billing configuration
variable "billing_mode" {
  description = "Controls how you are charged for read and write throughput (PROVISIONED or PAY_PER_REQUEST)"
  type        = string
  default     = "PAY_PER_REQUEST"
  validation {
    condition     = contains(["PROVISIONED", "PAY_PER_REQUEST"], var.billing_mode)
    error_message = "Billing mode must be either PROVISIONED or PAY_PER_REQUEST."
  }
}

# Capacity settings for Users table (only used when billing_mode = "PROVISIONED")
variable "users_read_capacity" {
  description = "Read capacity units for the users table"
  type        = number
  default     = 5
}

variable "users_write_capacity" {
  description = "Write capacity units for the users table"
  type        = number
  default     = 5
}

# Capacity settings for Tokens table (only used when billing_mode = "PROVISIONED")
variable "tokens_read_capacity" {
  description = "Read capacity units for the tokens table"
  type        = number
  default     = 5
}

variable "tokens_write_capacity" {
  description = "Write capacity units for the tokens table"
  type        = number
  default     = 5
}

# Capacity settings for Temporary Tokens table (only used when billing_mode = "PROVISIONED")
variable "temporary_tokens_read_capacity" {
  description = "Read capacity units for the temporary tokens table"
  type        = number
  default     = 3
}

variable "temporary_tokens_write_capacity" {
  description = "Write capacity units for the temporary tokens table"
  type        = number
  default     = 3
}

# Capacity settings for User Roles table (only used when billing_mode = "PROVISIONED")
variable "user_roles_read_capacity" {
  description = "Read capacity units for the user roles table"
  type        = number
  default     = 3
}

variable "user_roles_write_capacity" {
  description = "Write capacity units for the user roles table"
  type        = number
  default     = 3
}

# Security and backup settings
variable "enable_point_in_time_recovery" {
  description = "Enable point-in-time recovery for DynamoDB tables"
  type        = bool
  default     = true
}

variable "enable_encryption" {
  description = "Enable server-side encryption for DynamoDB tables"
  type        = bool
  default     = true
}

# Tagging
variable "common_tags" {
  description = "Common tags to apply to all resources"
  type        = map(string)
  default = {
    Environment = "dev"
    Project     = "UMS"
    Component   = "Identity"
    ManagedBy   = "Terraform"
  }
}

# Environment-specific settings
variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

# AWS region (informational)
variable "aws_region" {
  description = "AWS region where resources will be created"
  type        = string
  default     = null
}
