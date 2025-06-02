using System.Diagnostics;

namespace TestOpenTelemetry.SK.Services;

public interface IUserService
{
    Task<string> GetUserPreferencesAsync(string userId);
    Task ValidateUserAsync(string userId);
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private static readonly ActivitySource ActivitySource = new("TestOpenTelemetry.SK.UserService");

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetUserPreferencesAsync(string userId)
    {
        // Create an activity for the service method
        using var activity = ActivitySource.StartActivity("UserService.GetUserPreferences");
        activity?.SetTag("user.id", userId);
        activity?.SetTag("service.method", "GetUserPreferences");
        
        _logger.LogInformation("Fetching user preferences for user: {UserId}", userId);
        
        // Nested activity for database query
        using (var dbActivity = ActivitySource.StartActivity("Database.QueryUserPreferences"))
        {
            dbActivity?.SetTag("db.operation", "SELECT");
            dbActivity?.SetTag("db.table", "user_preferences");
            dbActivity?.SetTag("user.id", userId);
            
            _logger.LogDebug("Executing database query for user preferences");
            
            // Simulate database query
            await Task.Delay(150);
            
            _logger.LogDebug("Database query completed");
        }
        
        // Nested activity for caching
        using (var cacheActivity = ActivitySource.StartActivity("Cache.SetUserPreferences"))
        {
            cacheActivity?.SetTag("cache.operation", "SET");
            cacheActivity?.SetTag("cache.key", $"user_prefs_{userId}");
            cacheActivity?.SetTag("cache.ttl", "3600");
            
            _logger.LogDebug("Caching user preferences");
            
            // Simulate cache operation
            await Task.Delay(50);
            
            _logger.LogDebug("User preferences cached successfully");
        }
        
        _logger.LogInformation("User preferences retrieved successfully for user: {UserId}", userId);
        return $"Preferences for user {userId}";
    }

    public async Task ValidateUserAsync(string userId)
    {
        // Create an activity for user validation
        using var activity = ActivitySource.StartActivity("UserService.ValidateUser");
        activity?.SetTag("user.id", userId);
        activity?.SetTag("service.method", "ValidateUser");
        
        _logger.LogInformation("Validating user: {UserId}", userId);
        
        // Nested activity for authentication check
        using (var authActivity = ActivitySource.StartActivity("Authentication.CheckUser"))
        {
            authActivity?.SetTag("auth.method", "session");
            authActivity?.SetTag("user.id", userId);
            
            _logger.LogDebug("Checking user authentication");
            
            // Simulate authentication check
            await Task.Delay(75);
            
            authActivity?.SetTag("auth.result", "valid");
            _logger.LogDebug("User authentication validated");
        }
        
        // Nested activity for authorization check
        using (var authzActivity = ActivitySource.StartActivity("Authorization.CheckPermissions"))
        {
            authzActivity?.SetTag("authz.resource", "user_data");
            authzActivity?.SetTag("user.id", userId);
            
            _logger.LogDebug("Checking user permissions");
            
            // Simulate authorization check
            await Task.Delay(25);
            
            authzActivity?.SetTag("authz.result", "authorized");
            _logger.LogDebug("User permissions validated");
        }
        
        _logger.LogInformation("User validation completed for user: {UserId}", userId);
    }
}
