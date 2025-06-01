using Serilog;
using Serilog.Sinks.OpenTelemetry;
using SerilogTracing;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.OpenTelemetry("http://localhost:4318", OtlpProtocol.HttpProtobuf, null, new Dictionary<string, object>
    {
        { "service.name", typeof(Program).Assembly.GetName().Name ?? "TestASPNetApp" }
    })
    .CreateLogger();

using var _ = new ActivityListenerConfiguration()
    .TraceToSharedLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

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
