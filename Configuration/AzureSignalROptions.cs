namespace DriftMindWeb.Configuration;

/// <summary>
/// Configuration options for Azure SignalR Service
/// </summary>
public class AzureSignalROptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "AzureSignalR";

    /// <summary>
    /// Indicates whether Azure SignalR Service is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Connection string for Azure SignalR Service
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Application name for Azure SignalR
    /// </summary>
    public string ApplicationName { get; set; } = "DriftMindWeb";

    /// <summary>
    /// Validates if the Azure SignalR configuration is valid
    /// </summary>
    public bool IsValid => Enabled && !string.IsNullOrEmpty(ConnectionString);
}
