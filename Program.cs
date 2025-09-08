using DriftMindWeb.Components;
using DriftMindWeb.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure SignalR if enabled
var azureEnabled = builder.Configuration.GetValue<bool>("AzureSignalR:Enabled");
var connectionString = builder.Configuration["AzureSignalR:ConnectionString"];
var isAzureSignalRValid = azureEnabled && !string.IsNullOrEmpty(connectionString);

// Add services to the container.
if (isAzureSignalRValid)
{
    // Use Azure SignalR Service for scalability
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddSignalR()
        .AddAzureSignalR(options =>
        {
            options.ConnectionString = connectionString;
            // Enable Sticky Sessions for Blazor Server Circuits
            // This ensures that clients are always routed to the same server instance
            options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;
        });
    
    // Configure Circuit options for Azure SignalR with Sticky Sessions
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        // Extended JSInterop timeout for better reliability with Azure SignalR
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(3);
        // Extended circuit retention period - important for sticky sessions
        // Allows clients to reconnect to the same circuit on the same server
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(20);
        // Higher buffer for better performance with sticky sessions
        options.MaxBufferedUnacknowledgedRenderBatches = 50;
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
        // JSInterop timeout optimized for Azure SignalR (default: 1 min)
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(3);
        // Circuit retention for better reconnection (default: 3 min)
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(20);
        // More UI update buffer for chat apps (default: 10)
        options.MaxBufferedUnacknowledgedRenderBatches = 50;
    });
}

// Configure Shared Data Protection for Azure Blob Storage if enabled
var sharedDataProtectionEnabled = builder.Configuration.GetValue<bool>("SharedDataProtection:Enabled");
var sharedDataProtectionConnectionString = builder.Configuration["SharedDataProtection:AzureStorage:ConnectionString"];
var sharedDataProtectionContainerName = builder.Configuration["SharedDataProtection:AzureStorage:ContainerName"];
var sharedDataProtectionBlobName = builder.Configuration["SharedDataProtection:AzureStorage:BlobName"];
var sharedDataProtectionApplicationName = builder.Configuration["SharedDataProtection:ApplicationName"];

if (sharedDataProtectionEnabled && !string.IsNullOrEmpty(sharedDataProtectionConnectionString))
{
    try
    {
        // Configure Data Protection with Azure Blob Storage
        builder.Services.AddDataProtection()
            .SetApplicationName(sharedDataProtectionApplicationName ?? "DriftMindWeb")
            .PersistKeysToAzureBlobStorage(sharedDataProtectionConnectionString, sharedDataProtectionContainerName, sharedDataProtectionBlobName ?? "keys.xml");
            
        Console.WriteLine($"Shared Data Protection configured with Azure Blob Storage. Container: {sharedDataProtectionContainerName}, Blob: {sharedDataProtectionBlobName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to configure Shared Data Protection with Azure Blob Storage: {ex.Message}. Falling back to default configuration.");
        
        // Fall back to default data protection
        builder.Services.AddDataProtection()
            .SetApplicationName(sharedDataProtectionApplicationName ?? "DriftMindWeb");
    }
}
else
{
    // Use default data protection (local file system)
    builder.Services.AddDataProtection()
        .SetApplicationName(sharedDataProtectionApplicationName ?? "DriftMindWeb");
        
    if (sharedDataProtectionEnabled)
    {
        Console.WriteLine("Shared Data Protection is enabled but Azure Storage connection string is missing. Using default configuration.");
    }
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

// Protected Browser Storage is available via package; no explicit registration needed in .NET 8 Razor Components

// Add DriftMind API Service
builder.Services.AddScoped<IDriftMindApiService, DriftMindApiService>();

// Add Markdown Service
builder.Services.AddScoped<IMarkdownService, MarkdownService>();

// Add Timezone Service
builder.Services.AddScoped<ITimezoneService, TimezoneService>();

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

var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
// Ensure correct MIME type for web manifests
contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path.Value ?? string.Empty;
        if (string.Equals(path, "/service-worker.js", StringComparison.OrdinalIgnoreCase))
        {
            // Ensure the service worker itself is never cached aggressively
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    }
});
app.UseAntiforgery();

// Map Controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
