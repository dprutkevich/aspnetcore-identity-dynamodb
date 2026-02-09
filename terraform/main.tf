# UMS Identity DynamoDB Tables Terraform Module
# This module creates all necessary DynamoDB tables for the UMS Identity system

# Users table - stores identity user information
resource "aws_dynamodb_table" "users" {
  name           = var.users_table_name
  billing_mode   = var.billing_mode
  read_capacity  = var.billing_mode == "PROVISIONED" ? var.users_read_capacity : null
  write_capacity = var.billing_mode == "PROVISIONED" ? var.users_write_capacity : null
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "Email"
    type = "S"
  }

  global_secondary_index {
    name            = "EmailIndex"
    hash_key        = "Email"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.users_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.users_write_capacity : null
  }

  dynamic "point_in_time_recovery" {
    for_each = var.enable_point_in_time_recovery ? [1] : []
    content {
      enabled = true
    }
  }

  dynamic "server_side_encryption" {
    for_each = var.enable_encryption ? [1] : []
    content {
      enabled = true
    }
  }

  tags = merge(var.common_tags, {
    Name      = var.users_table_name
    Purpose   = "UMS Identity Users"
    TableType = "Users"
  })
}

# Refresh tokens table - stores JWT refresh tokens
resource "aws_dynamodb_table" "tokens" {
  name           = var.tokens_table_name
  billing_mode   = var.billing_mode
  read_capacity  = var.billing_mode == "PROVISIONED" ? var.tokens_read_capacity : null
  write_capacity = var.billing_mode == "PROVISIONED" ? var.tokens_write_capacity : null
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "UserId"
    type = "S"
  }

  attribute {
    name = "Token"
    type = "S"
  }

  attribute {
    name = "ExpiresAt"
    type = "S"
  }

  global_secondary_index {
    name            = "UserIdIndex"
    hash_key        = "UserId"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.tokens_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.tokens_write_capacity : null
  }

  global_secondary_index {
    name            = "TokenIndex"
    hash_key        = "Token"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.tokens_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.tokens_write_capacity : null
  }

  global_secondary_index {
    name            = "ExpiresAtIndex"
    hash_key        = "ExpiresAt"
    projection_type = "KEYS_ONLY"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.tokens_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.tokens_write_capacity : null
  }

  ttl {
    attribute_name = "ExpiresAt"
    enabled        = true
  }

  dynamic "point_in_time_recovery" {
    for_each = var.enable_point_in_time_recovery ? [1] : []
    content {
      enabled = true
    }
  }

  dynamic "server_side_encryption" {
    for_each = var.enable_encryption ? [1] : []
    content {
      enabled = true
    }
  }

  tags = merge(var.common_tags, {
    Name      = var.tokens_table_name
    Purpose   = "UMS Identity Refresh Tokens"
    TableType = "Tokens"
  })
}

# Temporary tokens table - stores email confirmation and password reset tokens
resource "aws_dynamodb_table" "temporary_tokens" {
  name           = var.temporary_tokens_table_name
  billing_mode   = var.billing_mode
  read_capacity  = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_read_capacity : null
  write_capacity = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_write_capacity : null
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "UserId"
    type = "S"
  }

  attribute {
    name = "Token"
    type = "S"
  }

  attribute {
    name = "Type"
    type = "S"
  }

  attribute {
    name = "ExpiresAt"
    type = "S"
  }

  global_secondary_index {
    name            = "UserIdTypeIndex"
    hash_key        = "UserId"
    range_key       = "Type"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_write_capacity : null
  }

  global_secondary_index {
    name            = "TokenTypeIndex"
    hash_key        = "Token"
    range_key       = "Type"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_write_capacity : null
  }

  global_secondary_index {
    name            = "ExpiresAtIndex"
    hash_key        = "ExpiresAt"
    projection_type = "KEYS_ONLY"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.temporary_tokens_write_capacity : null
  }

  ttl {
    attribute_name = "ExpiresAt"
    enabled        = true
  }

  dynamic "point_in_time_recovery" {
    for_each = var.enable_point_in_time_recovery ? [1] : []
    content {
      enabled = true
    }
  }

  dynamic "server_side_encryption" {
    for_each = var.enable_encryption ? [1] : []
    content {
      enabled = true
    }
  }

  tags = merge(var.common_tags, {
    Name      = var.temporary_tokens_table_name
    Purpose   = "UMS Identity Temporary Tokens"
    TableType = "TemporaryTokens"
  })
}

# User roles table - stores user role assignments
resource "aws_dynamodb_table" "user_roles" {
  name           = var.user_roles_table_name
  billing_mode   = var.billing_mode
  read_capacity  = var.billing_mode == "PROVISIONED" ? var.user_roles_read_capacity : null
  write_capacity = var.billing_mode == "PROVISIONED" ? var.user_roles_write_capacity : null
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "UserId"
    type = "S"
  }

  attribute {
    name = "RoleName"
    type = "S"
  }

  global_secondary_index {
    name            = "UserIdIndex"
    hash_key        = "UserId"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.user_roles_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.user_roles_write_capacity : null
  }

  global_secondary_index {
    name            = "RoleIndex"
    hash_key        = "RoleName"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.user_roles_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.user_roles_write_capacity : null
  }

  global_secondary_index {
    name            = "UserIdRoleIndex"
    hash_key        = "UserId"
    range_key       = "RoleName"
    projection_type = "ALL"
    read_capacity   = var.billing_mode == "PROVISIONED" ? var.user_roles_read_capacity : null
    write_capacity  = var.billing_mode == "PROVISIONED" ? var.user_roles_write_capacity : null
  }

  dynamic "point_in_time_recovery" {
    for_each = var.enable_point_in_time_recovery ? [1] : []
    content {
      enabled = true
    }
  }

  dynamic "server_side_encryption" {
    for_each = var.enable_encryption ? [1] : []
    content {
      enabled = true
    }
  }

  tags = merge(var.common_tags, {
    Name      = var.user_roles_table_name
    Purpose   = "UMS Identity User Roles"
    TableType = "UserRoles"
  })
}
