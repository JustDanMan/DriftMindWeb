using DriftMindWeb.Components;
using DriftMindWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

// Add Controllers for API endpoints
builder.Services.AddControllers();

var app = builder.Build();

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
