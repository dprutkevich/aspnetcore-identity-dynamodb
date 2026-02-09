namespace Identity.DynamoDb.Common;

/// <summary>
/// Provides predefined user-related errors for the identity system
/// </summary>
public static class UserErrors
{
    /// <summary>
    /// Gets an error indicating a user was not found by ID
    /// </summary>
    /// <param name="userId">The user ID that was not found</param>
    /// <returns>A not found error with the user ID</returns>
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    /// <summary>
    /// Gets an error indicating a user was not found by email
    /// </summary>
    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    /// <summary>
    /// Gets an error indicating the email is not unique
    /// </summary>
    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    /// <summary>
    /// Gets an error indicating the password is too weak
    /// </summary>
    public static readonly Error PasswordTooWeak = Error.Validation(
        "Users.PasswordTooWeak",
        "The provided password does not meet security requirements.");

    /// <summary>
    /// Gets an error indicating the password is too short
    /// </summary>
    public static readonly Error PasswordTooShort = Error.Validation(
        "Users.PasswordTooShort",
        "The provided password does not meet security requirements.");

    /// <summary>
    /// Gets an error indicating an unexpected error occurred
    /// </summary>
    public static readonly Error UnexpectedError = Error.Failure(
        "Users.UnexpectedError",
        "An unexpected error occurred. Please try again later.");

    /// <summary>
    /// Gets an error indicating email confirmation failed
    /// </summary>
    public static readonly Error EmailConfirmationFailed = Error.Failure(
        "Users.EmailConfirmationFailed",
        "Failed to confirm the email. The token might be invalid or expired.");

    /// <summary>
    /// Gets an error indicating the email is not confirmed
    /// </summary>
    public static readonly Error EmailNotConfirmed = Error.Validation(
        "Users.EmailNotConfirmed",
        "The email address has not been confirmed yet.");

    /// <summary>
    /// Gets an error indicating invalid login credentials
    /// </summary>
    public static readonly Error InvalidLogin = Error.Validation(
        "Users.InvalidLogin",
        "The email or password provided is incorrect.");

    /// <summary>
    /// Gets an error indicating password reset failed
    /// </summary>
    public static readonly Error PasswordResetFailed = Error.Validation(
        "Users.PasswordResetFailed",
        "The password reset token is invalid or expired.");

    /// <summary>
    /// Gets an error indicating the refresh token is invalid
    /// </summary>
    public static readonly Error InvalidRefreshToken = Error.Validation(
        "Users.InvalidRefreshToken",
        "The refresh token is invalid or has expired.");

    /// <summary>
    /// Gets an error indicating user update failed
    /// </summary>
    public static readonly Error UserUpdateFailed = Error.Failure(
        "Users.UserUpdateFailed",
        "Failed to update the user.");

    /// <summary>
    /// Gets an error indicating user deletion failed
    /// </summary>
    public static readonly Error UserDeleteFailed = Error.Failure(
        "Users.UserDeleteFailed",
        "Failed to delete the user.");

    /// <summary>
    /// Gets an error indicating email sending failed
    /// </summary>
    public static readonly Error EmailNotSet = Error.Failure(
        "Users.EmailSendFailed",
        "Failed to send email");

    /// <summary>
    /// Gets an error indicating the user ID is invalid
    /// </summary>
    public static readonly Error InvalidUserId = Error.Failure("Users.UserIdInvalid",
        "Cannot convert userId to Guid.");

    /// <summary>
    /// Gets an error indicating the old password is incorrect
    /// </summary>
    public static readonly Error InvalidOldPassword = Error.Validation(
        "User.InvalidOldPassword",
        "The current password is incorrect");

    /// <summary>
    /// Gets an error indicating password change failed
    /// </summary>
    public static readonly Error PasswordChangeFailed = Error.Failure(
        "User.PasswordChangeFailed",
        "Failed to change password");
}

