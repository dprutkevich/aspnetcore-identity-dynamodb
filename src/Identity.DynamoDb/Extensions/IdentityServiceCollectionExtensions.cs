using Amazon.DynamoDBv2;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Repositories;
using Identity.DynamoDb.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Identity.DynamoDb.Extensions;

/// <summary>
/// Extension methods for configuring UMS Identity services
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Adds UMS Identity services with DynamoDB storage to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="configureOptions">Optional action to configure identity options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUmsIdentity(
        this IServiceCollection services, 
        IConfiguration configuration,
        Action<IdentityOptions>? configureOptions = null)
    {
        // Configure options
        services.Configure<IdentityOptions>(configuration.GetSection("Identity"));
        
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Validate configuration
        services.AddSingleton<IValidateOptions<IdentityOptions>, IdentityOptionsValidator>();

        // Configure DynamoDB
        services.AddSingleton<IAmazonDynamoDB>(provider =>
        {
            var identityOptions = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
            var awsOptions = identityOptions.Aws;

            var config = new AmazonDynamoDBConfig();
            
            if (!string.IsNullOrEmpty(awsOptions?.Region))
            {
                config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region);
            }

            if (awsOptions?.UseLocalDynamoDb == true && !string.IsNullOrEmpty(awsOptions.ServiceUrl))
            {
                config.ServiceURL = awsOptions.ServiceUrl;
                config.UseHttp = true;
            }

            if (!string.IsNullOrEmpty(awsOptions?.AccessKey) && !string.IsNullOrEmpty(awsOptions?.SecretKey))
            {
                return new AmazonDynamoDBClient(awsOptions.AccessKey, awsOptions.SecretKey, config);
            }

            return new AmazonDynamoDBClient(config);
        });

        // Register core services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<ITemporaryTokenRepository, TemporaryTokenRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.TryAddScoped<IPasswordHasher, PasswordHasher>();
        services.TryAddScoped<IPasswordValidator, PasswordValidator>();

        // Register default notification service (can be overridden)
        services.TryAddScoped<IIdentityNotificationService, NullNotificationService>();

        // Register all endpoints from the assembly
        var assembly = Assembly.GetExecutingAssembly();
        services.AddEndpoints(assembly);

        return services;
    }

    /// <summary>
    /// Adds JWT authentication to the service collection using UMS Identity configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUmsJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<IdentityOptions>>((jwtBearerOptions, identityOptionsAccessor) =>
            {
                var jwtOptions = identityOptionsAccessor.Value.Jwt;

                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwtOptions.Issuer),
                    ValidateAudience = !string.IsNullOrEmpty(jwtOptions.Audience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Default notification service that does nothing (for development/testing)
    /// Override by registering your own IIdentityNotificationService implementation
    /// </summary>
    public class NullNotificationService : IIdentityNotificationService
    {
        /// <summary>
        /// Sends email confirmation (no-op implementation)
        /// </summary>
        public Task SendEmailConfirmationAsync(Guid userId, string email, string token) => Task.CompletedTask;
        
        /// <summary>
        /// Sends password reset email (no-op implementation)
        /// </summary>
        public Task SendPasswordResetAsync(Guid userId, string email, string token) => Task.CompletedTask;
        
        /// <summary>
        /// Sends password changed notification (no-op implementation)
        /// </summary>
        public Task SendPasswordChangedAsync(Guid userId, string email) => Task.CompletedTask;
        
        /// <summary>
        /// Sends welcome email (no-op implementation)
        /// </summary>
        public Task SendWelcomeEmailAsync(Guid userId, string email) => Task.CompletedTask;
    }
}
