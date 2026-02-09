using Amazon.DynamoDBv2;
using FluentAssertions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Identity.DynamoDb.Extensions.IdentityServiceCollectionExtensions;
using Moq;
using Identity.DynamoDb.Abstractions;


namespace Identity.DynamoDb.Tests;

[TestFixture]
public class AddIdentityDynamoDbTests
{
    
    private ServiceProvider? _provider;

    [SetUp]
    public void Setup()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Identity:SendWelcomeEmail"] = "true",
            ["Identity:Jwt:Secret"] = "test-secret-that-is-at-least-32-characters-long",
            ["Identity:Jwt:Issuer"] = "issuer",
            ["Identity:Jwt:Audience"] = "audience",
            ["Identity:Jwt:AccessTokenLifetimeMinutes"] = "60",
            ["Identity:DynamoDb:UsersTable"] = "users",
            ["Identity:DynamoDb:TokensTable"] = "tokens",
            ["Identity:DynamoDb:TemporaryTokensTable"] = "temp_tokens",
            ["Identity:DynamoDb:UserRolesTable"] = "user_roles"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddUmsIdentity(config);
        
        // Override the DynamoDB registration with a mock
        var dynamoDbMock = new Mock<IAmazonDynamoDB>();
        services.RemoveAll<IAmazonDynamoDB>();
        services.AddSingleton(dynamoDbMock.Object);

        _provider = services.BuildServiceProvider();
    }
    
    [TearDown]
    public void TearDown()
    {
        _provider?.Dispose();
    }

    [Test]
    public void ShouldBind_IdentityOptions_FromConfiguration()
    {
        if (_provider == null)
        {
            throw new InvalidOperationException("Service provider is not initialized.");
        }

        var options = _provider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        options.SendWelcomeEmail.Should().BeTrue();
        options.Jwt.Secret.Should().Be("test-secret-that-is-at-least-32-characters-long");
        options.DynamoDb.UsersTable.Should().Be("users");
    }

    [Test]
    public void ShouldRegister_AuthService_AndRepositories()
    {
        if (_provider == null)
        {
            throw new InvalidOperationException("Service provider is not initialized.");
        }

        _provider.GetRequiredService<IAuthService>().Should().NotBeNull();
        _provider.GetRequiredService<IIdentityUserRepository>().Should().NotBeNull();
        _provider.GetRequiredService<ITokenRepository>().Should().NotBeNull();
        _provider.GetRequiredService<ITemporaryTokenRepository>().Should().NotBeNull();
        _provider.GetRequiredService<IUserRoleRepository>().Should().NotBeNull();
    }

    [Test]
    public void ShouldUse_NullNotificationService_ByDefault()
    {
        if (_provider == null)
        {
            throw new InvalidOperationException("Service provider is not initialized.");
        }

        var notification = _provider.GetRequiredService<IIdentityNotificationService>();
        notification.Should().BeOfType<NullNotificationService>();
    }
}
