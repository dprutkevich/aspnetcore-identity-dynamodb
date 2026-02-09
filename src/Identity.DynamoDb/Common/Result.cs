using System.Diagnostics.CodeAnalysis;

namespace Identity.DynamoDb.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Initializes a new instance of the Result class
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="error">The error if operation failed</param>
        public Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None ||
                !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error associated with the result
        /// </summary>
        public Error Error { get; }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <returns>A successful result</returns>
        public static Result Success() => new(true, Error.None);

        /// <summary>
        /// Creates a successful result with a value
        /// </summary>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="value">The value</param>
        /// <returns>A successful result with value</returns>
        public static Result<TValue> Success<TValue>(TValue value) =>
            new(value, true, Error.None);

        /// <summary>
        /// Creates a failed result
        /// </summary>
        /// <param name="error">The error</param>
        /// <returns>A failed result</returns>
        public static Result Failure(Error error) => new(false, error);

        /// <summary>
        /// Creates a failed result with a specific type
        /// </summary>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="error">The error</param>
        /// <returns>A failed result</returns>
        public static Result<TValue> Failure<TValue>(Error error) =>
            new(default, false, error);
    }

    /// <summary>
    /// Represents the result of an operation that can either succeed with a value or fail
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        /// <summary>
        /// Initializes a new instance of the Result class with a value
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="error">The error if operation failed</param>
        public Result(TValue? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value if the result is successful
        /// </summary>
        [NotNull]
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result can't be accessed.");

        /// <summary>
        /// Implicitly converts a value to a Result
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>A successful result if value is not null, otherwise a failure</returns>
        public static implicit operator Result<TValue>(TValue? value) =>
            value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

        /// <summary>
        /// Creates a validation failure result
        /// </summary>
        /// <param name="error">The validation error</param>
        /// <returns>A failed result</returns>
        public static Result<TValue> ValidationFailure(Error error) =>
            new(default, false, error);
    }
}

