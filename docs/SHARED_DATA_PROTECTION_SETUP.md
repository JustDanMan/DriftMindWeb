# Shared Data Protection Setup for Horizontal Scaling

This configuration enables storing ASP.NET Core Data Protection Keys in Azure Blob Storage to prevent AntiforgeryValidationException errors during horizontal scaling.

## Configuration

### 1. appsettings.json / appsettings.{Environment}.json

```json
{
  "SharedDataProtection": {
    "Enabled": true,
    "AzureStorage": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorageaccount;AccountKey=yourkey;EndpointSuffix=core.windows.net",
      "ContainerName": "dataprotection-keys",
      "BlobName": "keys.xml"
    },
    "ApplicationName": "DriftMindWeb"
  }
}
```

### 2. Configuration Parameters

- **`Enabled`**: Enables/disables Azure Blob Storage Data Protection
- **`ConnectionString`**: Azure Storage Account Connection String
- **`ContainerName`**: Name of the blob container (must exist before application startup)
- **`BlobName`**: Name of the blob file for the keys (default: "keys.xml")
- **`ApplicationName`**: Unique application name for key isolation

## How It Works

### Single Instance (Enabled: false)
- Data Protection keys are stored locally in the file system
- Works only with a single app instance
- Standard ASP.NET Core behavior

### Horizontal Scaling (Enabled: true)
- Data Protection keys are stored in Azure Blob Storage
- All app instances share the same keys
- Prevents AntiforgeryValidationException during load balancing
- **Important**: The blob container must exist before starting the application

## Prerequisites

Before enabling Shared Data Protection, ensure:
1. Azure Storage Account is created
2. Blob container `dataprotection-keys` exists (or your custom container name)
3. Connection string has appropriate permissions (read/write access to the container)

## Deployment Scenarios

### Development
```json
{
  "SharedDataProtection": {
    "Enabled": false
  }
}
```

### Staging/Production (Single Instance)
```json
{
  "SharedDataProtection": {
    "Enabled": false
  }
}
```

### Production (Multiple Instances)
```json
{
  "SharedDataProtection": {
    "Enabled": true,
    "AzureStorage": {
      "ConnectionString": "{{AZURE_STORAGE_CONNECTION_STRING}}",
      "ContainerName": "dataprotection-keys",
      "BlobName": "keys.xml"
    },
    "ApplicationName": "DriftMindWeb"
  }
}
```

## Error Handling

The implementation includes robust error handling:

1. **Missing Connection String**: Fallback to local Data Protection
2. **Azure Storage Errors**: Fallback to local Data Protection with logging
3. **Missing Container**: Application will fail to start - container must be created manually

## Setup Steps

1. **Create Azure Storage Account** (if not already exists)
2. **Create blob container** named `dataprotection-keys` (or your custom name)
3. **Configure connection string** in appsettings
4. **Enable the feature** by setting `Enabled: true`

## Logs

The configuration provides detailed logs:
- Successful Azure Blob Storage configuration
- Failed configuration with fallback information
- Warning for missing connection string

## Security

- Connection strings should never be stored in source control
- Use Azure Key Vault or environment variables for production
- The `ApplicationName` isolates keys between different applications

## Migration

Existing applications can enable this configuration without downtime:
1. `Enabled: false` → Keys remain local
2. `Enabled: true` → New keys are created in Azure, old keys remain valid until they expire
