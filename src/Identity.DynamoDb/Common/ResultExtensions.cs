namespace Identity.DynamoDb.Common;

/// <summary>
/// Extension methods for Result types
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Matches a result to either success or failure function
    /// </summary>
    /// <typeparam name="TOut">The output type</typeparam>
    /// <param name="result">The result to match</param>
    /// <param name="onSuccess">Function to call on success</param>
    /// <param name="onFailure">Function to call on failure</param>
    /// <returns>The result of the matched function</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    /// <summary>
    /// Matches a result with value to either success or failure function
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output type</typeparam>
    /// <param name="result">The result to match</param>
    /// <param name="onSuccess">Function to call on success with the value</param>
    /// <param name="onFailure">Function to call on failure</param>
    /// <returns>The result of the matched function</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }

    /// <summary>
    /// Maps the value of a successful result to a new type
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="result">The result to map</param>
    /// <param name="map">The mapping function</param>
    /// <returns>A result with the mapped value or the original error</returns>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> map)
    {
        return result.IsSuccess
            ? Result.Success(map(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Maps the value of a successful result from a task to a new type
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="resultTask">The task containing the result to map</param>
    /// <param name="map">The mapping function</param>
    /// <returns>A task with a result containing the mapped value or the original error</returns>
    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> map)
    {
        var result = await resultTask;
        return result.Map(map);
    }

    /// <summary>
    /// Maps each item in a list result to a new type
    /// </summary>
    /// <typeparam name="TIn">The input item type</typeparam>
    /// <typeparam name="TOut">The output item type</typeparam>
    /// <param name="result">The result containing a list to map</param>
    /// <param name="map">The mapping function for each item</param>
    /// <returns>A result with the mapped list or the original error</returns>
    public static Result<List<TOut>> MapList<TIn, TOut>(
        this Result<List<TIn>> result,
        Func<TIn, TOut> map)
    {
        return result.IsSuccess
            ? Result.Success(result.Value.Select(map).ToList())
            : Result.Failure<List<TOut>>(result.Error);
    }

    /// <summary>
    /// Binds a result from a task to another async operation
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="task">The task containing the result to bind</param>
    /// <param name="next">The next async operation to bind to</param>
    /// <returns>A task with the result of the bind operation</returns>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> task,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await task;
        return result.IsSuccess
            ? await next(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

}

