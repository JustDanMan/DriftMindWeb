using DriftMindWeb.Components;
using DriftMindWeb.Services;
using DriftMindWeb.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure SignalR if enabled
var azureSignalROptions = builder.Configuration.GetSection(AzureSignalROptions.SectionName).Get<AzureSignalROptions>() ?? new AzureSignalROptions();

// Configure options
builder.Services.Configure<AzureSignalROptions>(builder.Configuration.GetSection(AzureSignalROptions.SectionName));

// Add services to the container.
if (azureSignalROptions.IsValid)
{
    // Use Azure SignalR Service for scalability
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddSignalR()
        .AddAzureSignalR(azureSignalROptions.ConnectionString);
    
    // Configure Circuit options for Azure SignalR
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });
}
else
{
    // Use default SignalR (in-memory) for single instance
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
        
    // Configure Circuit options for local SignalR
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });
}

// Configure form options for file uploads
var maxUploadSizeMB = builder.Configuration.GetValue<int>("DriftMindApi:MaxUploadSizeMB", 3);
var maxUploadSizeBytes = maxUploadSizeMB * 1024 * 1024;

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadSizeBytes;
    options.ValueLengthLimit = maxUploadSizeBytes;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add DriftMind API Service
builder.Services.AddScoped<IDriftMindApiService, DriftMindApiService>();

// Add Markdown Service
builder.Services.AddScoped<IMarkdownService, MarkdownService>();

// Add SignalR Service as Singleton (configuration doesn't change at runtime)
builder.Services.AddSingleton<ISignalRService, SignalRService>();

// Add Controllers for API endpoints
builder.Services.AddControllers();

var app = builder.Build();

// Initialize SignalR Service to trigger logging at startup
var signalRService = app.Services.GetRequiredService<ISignalRService>();
var signalRInfo = signalRService.GetSignalRInfo(); // This will trigger the logging

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Map Controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
