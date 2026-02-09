namespace Identity.DynamoDb.Common;

/// <summary>
/// Represents error types that can occur in the application
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// General failure error
    /// </summary>
    Failure = 0,
    /// <summary>
    /// Validation error
    /// </summary>
    Validation = 1,
    /// <summary>
    /// Problem error
    /// </summary>
    Problem = 2,
    /// <summary>
    /// Not found error
    /// </summary>
    NotFound = 3,
    /// <summary>
    /// Conflict error
    /// </summary>
    Conflict = 4,
    /// <summary>
    /// Authentication error
    /// </summary>
    Authentication = 5,
    /// <summary>
    /// Authorization error
    /// </summary>
    Authorization = 6
}

/// <summary>
/// Represents an error with code, description, and type
/// </summary>
/// <param name="Code">Error code</param>
/// <param name="Description">Error description</param>
/// <param name="Type">Error type</param>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>
    /// Additional metadata for the error
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Represents no error
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    /// <summary>
    /// Represents a null value error
    /// </summary>
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    /// <summary>
    /// Creates a failure error
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="description">Error description</param>
    /// <returns>Error instance</returns>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a not found error
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="description">Error description</param>
    /// <returns>Error instance</returns>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a problem error
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="description">Error description</param>
    /// <returns>Error instance</returns>
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    /// <summary>
    /// Creates a conflict error
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="description">Error description</param>
    /// <returns>Error instance</returns>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Creates a validation error
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="description">Error description</param>
    /// <returns>Error instance</returns>
    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    /// <summary>
    /// Adds metadata to the error
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    /// <returns>Error instance with metadata</returns>
    public Error WithMetadata(string key, string value)
    {
        var newMetadata = Metadata is null
            ? new Dictionary<string, string> { { key, value } }
            : new Dictionary<string, string>(Metadata) { [key] = value };

        return this with { Metadata = newMetadata };
    }
}

