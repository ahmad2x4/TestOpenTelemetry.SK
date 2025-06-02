using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestOpenTelemetry.Models;
using TestOpenTelemetry.Services;

namespace TestOpenTelemetry.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserService _userService;
    private static readonly ActivitySource ActivitySource = new("TestOpenTelemetry.HomeController");

    public HomeController(ILogger<HomeController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Hellog world from index");
        // Create a main activity for the Index action
        using var mainActivity = ActivitySource.StartActivity("HomeController.Index");
        mainActivity?.SetTag("controller", "Home");
        mainActivity?.SetTag("action", "Index");
        mainActivity?.SetTag("user.id", "demo-user");
        
        _logger.LogInformation("Starting Index action processing");
        
        // Nested activity for user validation - this will call the service
        using (var validationActivity = ActivitySource.StartActivity("ValidateUser"))
        {
            validationActivity?.SetTag("operation.type", "user-validation");
            validationActivity?.SetTag("user.id", "demo-user");
            
            _logger.LogInformation("Validating user");
            
            // This will create nested activities within the service
            await _userService.ValidateUserAsync("demo-user");
        }
        
        // Nested activity for fetching user preferences - this will also call the service
        using (var preferencesActivity = ActivitySource.StartActivity("FetchUserPreferences"))
        {
            preferencesActivity?.SetTag("operation.type", "data-retrieval");
            preferencesActivity?.SetTag("user.id", "demo-user");
            
            _logger.LogInformation("Fetching user preferences");
            
            // This will create nested activities within the service
            var preferences = await _userService.GetUserPreferencesAsync("demo-user");
            
            preferencesActivity?.SetTag("preferences.retrieved", "true");
            _logger.LogInformation("User preferences retrieved: {Preferences}", preferences);
        }
        
        // Nested activity for additional processing
        using (var processingActivity = ActivitySource.StartActivity("AdditionalProcessing"))
        {
            processingActivity?.SetTag("operation.type", "business-logic");
            processingActivity?.SetTag("processing.stage", "final");
            
            _logger.LogInformation("Performing additional processing");
            
            // Simulate some additional work
            await Task.Delay(100);
            
            processingActivity?.SetTag("processing.completed", "true");
        }
        
        _logger.LogInformation("Index action completed successfully");
        return View();
    }

    public IActionResult Privacy()
    {
        // Create activity for Privacy action
        using var activity = ActivitySource.StartActivity("HomeController.Privacy");
        activity?.SetTag("controller", "Home");
        activity?.SetTag("action", "Privacy");
        activity?.SetTag("page.type", "static");
        
        _logger.LogInformation("Privacy page accessed");
        
        // Nested activity for audit logging
        using (var auditActivity = ActivitySource.StartActivity("AuditLogging"))
        {
            auditActivity?.SetTag("audit.action", "page-access");
            auditActivity?.SetTag("audit.page", "privacy");
            auditActivity?.SetTag("user.ip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            
            _logger.LogInformation("Logging privacy page access for audit");
            LogPageAccess("Privacy");
        }
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Create activity for Error handling
        using var activity = ActivitySource.StartActivity("HomeController.Error");
        activity?.SetTag("controller", "Home");
        activity?.SetTag("action", "Error");
        activity?.SetTag("error.handling", "true");
        
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        _logger.LogError("Error page accessed with RequestId: {RequestId}", requestId);
        
        // Nested activity for error processing
        using (var errorActivity = ActivitySource.StartActivity("ProcessError"))
        {
            errorActivity?.SetTag("error.request_id", requestId);
            errorActivity?.SetTag("error.source", "application");
            
            _logger.LogInformation("Processing error information");
            ProcessErrorInformation(requestId);
        }
        
        return View(new ErrorViewModel { RequestId = requestId });
    }
    
    // Helper methods to simulate work (these would be your actual business logic)
    private void LogPageAccess(string pageName)
    {
        // Simulate audit logging time
        Thread.Sleep(25);
        _logger.LogDebug("Page access logged: {PageName}", pageName);
    }
    
    private void ProcessErrorInformation(string requestId)
    {
        // Simulate error processing time
        Thread.Sleep(40);
        _logger.LogDebug("Error information processed for request: {RequestId}", requestId);
    }
}
