namespace Identity.DynamoDb.Common;

/// <summary>
/// Represents a validation error that contains multiple individual errors
/// </summary>
/// <param name="Errors">The array of individual validation errors</param>
public sealed record ValidationError(Error[] Errors) : Error("Validation.General",
    "One or more validation errors occurred", ErrorType.Validation)
{
    /// <summary>
    /// Creates a validation error from a collection of failed results
    /// </summary>
    /// <param name="results">The collection of results to extract errors from</param>
    /// <returns>A validation error containing all the failed result errors</returns>
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
