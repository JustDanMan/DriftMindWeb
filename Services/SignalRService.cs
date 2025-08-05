using Microsoft.Extensions.Options;
using DriftMindWeb.Configuration;

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
    private readonly AzureSignalROptions _azureSignalROptions;
    private readonly SignalRInfo _signalRInfo;

    public SignalRService(
        IOptions<AzureSignalROptions> azureSignalROptions,
        ILogger<SignalRService> logger)
    {
        _azureSignalROptions = azureSignalROptions.Value ?? new AzureSignalROptions();
        
        // Initialize SignalR info once
        var mode = IsAzureSignalREnabled ? "Azure SignalR Service" : "Local SignalR (In-Memory)";
        _signalRInfo = new SignalRInfo(
            IsAzureSignalREnabled,
            mode,
            _azureSignalROptions.ApplicationName
        );
        
        // Log once during service initialization
        logger.LogInformation("SignalR Mode: {Mode}, Azure Enabled: {IsEnabled}", 
            _signalRInfo.Mode, _signalRInfo.IsAzureEnabled);
    }

    /// <summary>
    /// Indicates whether Azure SignalR is enabled
    /// </summary>
    public bool IsAzureSignalREnabled => _azureSignalROptions.IsValid;

    /// <summary>
    /// Returns information about the current SignalR configuration
    /// </summary>
    public SignalRInfo GetSignalRInfo()
    {
        return _signalRInfo;
    }
}
