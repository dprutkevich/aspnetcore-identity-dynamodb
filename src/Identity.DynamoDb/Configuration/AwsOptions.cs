namespace Identity.DynamoDb.Configuration;

/// <summary>
/// Configuration options for AWS DynamoDB connection
/// </summary>
public class AwsOptions
{
    /// <summary>
    /// AWS Access Key ID. If not provided, will use default AWS credential chain
    /// </summary>
    public string? AccessKey { get; init; }

    /// <summary>
    /// AWS Secret Access Key. If not provided, will use default AWS credential chain
    /// </summary>
    public string? SecretKey { get; init; }

    /// <summary>
    /// AWS Region (e.g., "us-east-1", "eu-west-1")
    /// </summary>
    public string Region { get; init; } = "us-east-1";

    /// <summary>
    /// DynamoDB service URL. Use for local DynamoDB development
    /// </summary>
    public string? ServiceUrl { get; init; }

    /// <summary>
    /// Whether to use LocalStack or local DynamoDB for development
    /// </summary>
    public bool UseLocalDynamoDb { get; init; } = false;
}
