using Microsoft.Extensions.Configuration;

namespace DriftMindWeb.Services;

/// <summary>
/// Interface for SignalR Service Management
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Indicates whether Azure SignalR is enabled
    /// </summary>
    bool IsAzureSignalREnabled { get; }
    
    /// <summary>
    /// Returns information about the current SignalR configuration
    /// </summary>
    SignalRInfo GetSignalRInfo();
}

/// <summary>
/// Information about the SignalR configuration
/// </summary>
public record SignalRInfo(
    bool IsAzureEnabled,
    string Mode,
    string ApplicationName
);

/// <summary>
/// Service for SignalR management and configuration
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly IConfiguration _configuration;
    private readonly SignalRInfo _signalRInfo;

    public SignalRService(IConfiguration configuration, ILogger<SignalRService> logger)
    {
        _configuration = configuration;
        
        // Initialize SignalR info once
        var mode = IsAzureSignalREnabled ? "Azure SignalR Service" : "Local SignalR (In-Memory)";
        var appName = _configuration["AzureSignalR:ApplicationName"] ?? "DriftMindWeb";
        
        _signalRInfo = new SignalRInfo(
            IsAzureSignalREnabled,
            mode,
            appName
        );
        
        // Log once during service initialization
        logger.LogInformation("SignalR Mode: {Mode}, Azure Enabled: {IsEnabled}", 
            _signalRInfo.Mode, _signalRInfo.IsAzureEnabled);
    }

    /// <summary>
    /// Indicates whether Azure SignalR is enabled
    /// </summary>
    public bool IsAzureSignalREnabled
    {
        get
        {
            var enabled = _configuration.GetValue<bool>("AzureSignalR:Enabled");
            var connectionString = _configuration["AzureSignalR:ConnectionString"];
            return enabled && !string.IsNullOrEmpty(connectionString);
        }
    }

    /// <summary>
    /// Returns information about the current SignalR configuration
    /// </summary>
    public SignalRInfo GetSignalRInfo()
    {
        return _signalRInfo;
    }
}
