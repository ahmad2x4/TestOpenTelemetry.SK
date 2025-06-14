using Microsoft.SemanticKernel;
using OpenAI;
using Serilog;
using SerilogTracing;
using TestOpenTelemetry.SK.Plugins;
using TestOpenTelemetry.SK.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", typeof(Program).Assembly.GetName().Name)
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnostics", true);

using var _ = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .TraceToSharedLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserService, UserService>();

var openAIKey = builder.Configuration["Settings:OpenAI:ApiKey"];


// Create a kernel with Azure OpenAI chat completion
var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o-mini", openAIKey);
kernelBuilder.Services.AddSerilog();

// Register dependencies in kernel's service collection
kernelBuilder.Services.AddSingleton<List<LightModel>>(provider => new List<LightModel>
{
    new() { Id = 1, Name = "Living Room Light", IsOn = true, Brightness = Brightness.Medium, Color = "#FFF8DC" },
    new() { Id = 2, Name = "Kitchen Light", IsOn = false, Brightness = Brightness.High, Color = "#F0F8FF" },
    new() { Id = 3, Name = "Bedroom Light", IsOn = true, Brightness = Brightness.Low, Color = "#4169E1" }
});
kernelBuilder.Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
kernelBuilder.Services.AddScoped<LightsPlugin>();

// Build the kernel
Kernel kernel = kernelBuilder.Build();

builder.Services.AddSingleton(kernel);



var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
