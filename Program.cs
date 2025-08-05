using DriftMindWeb.Components;
using DriftMindWeb.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;

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
        .AddAzureSignalR(connectionString);
    
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
