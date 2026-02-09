# Output values for UMS Identity DynamoDB module

# Table names
output "users_table_name" {
  description = "Name of the users DynamoDB table"
  value       = aws_dynamodb_table.users.name
}

output "tokens_table_name" {
  description = "Name of the tokens DynamoDB table"
  value       = aws_dynamodb_table.tokens.name
}

output "temporary_tokens_table_name" {
  description = "Name of the temporary tokens DynamoDB table"
  value       = aws_dynamodb_table.temporary_tokens.name
}

output "user_roles_table_name" {
  description = "Name of the user roles DynamoDB table"
  value       = aws_dynamodb_table.user_roles.name
}

# Table ARNs
output "users_table_arn" {
  description = "ARN of the users DynamoDB table"
  value       = aws_dynamodb_table.users.arn
}

output "tokens_table_arn" {
  description = "ARN of the tokens DynamoDB table"
  value       = aws_dynamodb_table.tokens.arn
}

output "temporary_tokens_table_arn" {
  description = "ARN of the temporary tokens DynamoDB table"
  value       = aws_dynamodb_table.temporary_tokens.arn
}

output "user_roles_table_arn" {
  description = "ARN of the user roles DynamoDB table"
  value       = aws_dynamodb_table.user_roles.arn
}

# Combined outputs for easier consumption
output "all_table_names" {
  description = "Map of all table names for easy reference"
  value = {
    users            = aws_dynamodb_table.users.name
    tokens           = aws_dynamodb_table.tokens.name
    temporary_tokens = aws_dynamodb_table.temporary_tokens.name
    user_roles       = aws_dynamodb_table.user_roles.name
  }
}

output "all_table_arns" {
  description = "List of all table ARNs for IAM policy creation"
  value = [
    aws_dynamodb_table.users.arn,
    aws_dynamodb_table.tokens.arn,
    aws_dynamodb_table.temporary_tokens.arn,
    aws_dynamodb_table.user_roles.arn
  ]
}

# Configuration object for .NET application
output "identity_configuration" {
  description = "Configuration object for UMS Identity application"
  value = {
    DynamoDb = {
      UsersTable           = aws_dynamodb_table.users.name
      TokensTable          = aws_dynamodb_table.tokens.name
      TemporaryTokensTable = aws_dynamodb_table.temporary_tokens.name
      UserRolesTable       = aws_dynamodb_table.user_roles.name
    }
  }
}

# IAM policy for application access
output "identity_dynamodb_policy" {
  description = "IAM policy JSON for UMS Identity application to access DynamoDB tables"
  value = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
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
        Resource = [
          aws_dynamodb_table.users.arn,
          aws_dynamodb_table.tokens.arn,
          aws_dynamodb_table.temporary_tokens.arn,
          aws_dynamodb_table.user_roles.arn,
          "${aws_dynamodb_table.users.arn}/index/*",
          "${aws_dynamodb_table.tokens.arn}/index/*",
          "${aws_dynamodb_table.temporary_tokens.arn}/index/*",
          "${aws_dynamodb_table.user_roles.arn}/index/*"
        ]
      }
    ]
  })
}
