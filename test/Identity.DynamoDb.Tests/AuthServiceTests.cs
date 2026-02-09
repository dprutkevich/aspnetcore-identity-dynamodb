using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Models;
using Identity.DynamoDb.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IIdentityUserRepository> _userRepo = null!;
    private Mock<ITokenRepository> _tokenRepo = null!;
    private Mock<ITemporaryTokenRepository> _tempTokenRepo = null!;
    private Mock<IIdentityNotificationService> _notifier = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IPasswordValidator> _passwordValidator = null!;
    private IOptions<IdentityOptions> _options = null!;
    private AuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepo = new Mock<IIdentityUserRepository>();
        _tokenRepo = new Mock<ITokenRepository>();
        _tempTokenRepo = new Mock<ITemporaryTokenRepository>();
        _notifier = new Mock<IIdentityNotificationService>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _passwordValidator = new Mock<IPasswordValidator>();

        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>()))
            .Returns<string>(password => $"hash:{password}");
        _passwordHasher.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((hash, password) => hash == $"hash:{password}");
        _passwordValidator.Setup(x => x.ValidatePassword(It.IsAny<string>()))
            .Returns((true, new List<string>()));

        _options = Options.Create(new IdentityOptions
        {
            SendWelcomeEmail = true,
            Jwt = new JwtOptions
            {
                Secret = "testsecretQWEqweRFVTGByhhnUJ8447esjfngbidgfnIUS",
                Issuer = "test",
                Audience = "test",
                AccessTokenLifetimeMinutes = 15
            },
            DynamoDb = new DynamoDbTablesOptions
            {
                UsersTable = "users",
                TokensTable = "tokens",
                TemporaryTokensTable = "temp_tokens",
                UserRolesTable = "user_roles"
            }
        });

        _authService = new AuthService(
            _userRepo.Object,
            _tokenRepo.Object,
            _tempTokenRepo.Object,
            _notifier.Object,
            _passwordHasher.Object,
            _passwordValidator.Object,
            _options
        );
    }

    [Test]
    public async Task RegisterAsync_ShouldCreateUserAndReturnTokens()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("new@example.com")).ReturnsAsync((IdentityUser?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<IdentityUser>())).Returns(Task.CompletedTask);
        _tokenRepo.Setup(r => r.StoreRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync("new@example.com", "Password1!");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().NotBeNull();
        _notifier.Verify(n => n.SendWelcomeEmailAsync(It.IsAny<Guid>(), "new@example.com"), Times.Once);
    }

    [Test]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUser
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash:testpw",
            IsActive = true,
            IsEmailConfirmed = true
        };

        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _tokenRepo.Setup(r => r.StoreRefreshTokenAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>()))
                  .Returns(Task.CompletedTask);

        var result = await _authService.LoginAsync(user.Email, "testpw");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsWrong()
    {
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash:correct",
            IsActive = true
        };

        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await _authService.LoginAsync(user.Email, "wrong");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidPassword");
    }

    [Test]
    public async Task RefreshAccessTokenAsync_ShouldReturnAccessToken_WhenValid()
    {
        var userId = Guid.NewGuid();
        _tokenRepo.Setup(r => r.IsValidAsync("valid-token")).ReturnsAsync(true);
        _tokenRepo.Setup(r => r.GetUserIdByTokenAsync("valid-token")).ReturnsAsync(userId);
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new IdentityUser
        {
            Id = userId,
            Email = "test@example.com"
        });

        var result = await _authService.RefreshAccessTokenAsync("valid-token");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task RefreshAccessTokenAsync_ShouldFail_WhenInvalid()
    {
        _tokenRepo.Setup(r => r.IsValidAsync("invalid")).ReturnsAsync(false);

        var result = await _authService.RefreshAccessTokenAsync("invalid");

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task SendConfirmationEmailAsync_ShouldTriggerEmailAndNotify()
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUser { Id = userId, Email = "x@y.com" };
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _authService.SendConfirmationEmailAsync(userId);

        result.IsSuccess.Should().BeTrue();
        _tempTokenRepo.Verify(r => r.AddAsync(It.Is<TemporaryToken>(t => 
            t.UserId == userId && 
            t.Type == TokenType.EmailConfirmation)), Times.Once);
        _notifier.Verify(n => n.SendEmailConfirmationAsync(userId, user.Email, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ResetPasswordAsync_ShouldFail_WhenTokenInvalid()
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUser 
        { 
            Id = userId, 
            Email = "test@example.com"
        };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _tempTokenRepo.Setup(r => r.GetByTokenAsync("bad-token", TokenType.PasswordReset))
            .ReturnsAsync((TemporaryToken?)null);

        var result = await _authService.ResetPasswordAsync(user.Email, "bad-token", "new");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reset.InvalidToken");
    }

    [Test]
    public async Task ChangePasswordAsync_ShouldFail_WhenOldPasswordWrong()
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUser
        {
            Id = userId,
            Email = "x@y.com",
            PasswordHash = "hash:right"
        };

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _authService.ChangePasswordAsync(userId, "wrong", "new");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidPassword");
    }
}
