# Azure SignalR Service Integration

This application supports both local SignalR (default) and Azure SignalR Service for scalability.

## Configuration

### Local SignalR (Default)
```json
{
  "AzureSignalR": {
    "Enabled": false,
    "ConnectionString": "",
    "ApplicationName": "DriftMindWeb"
  }
}
```

### Azure SignalR Service
```json
{
  "AzureSignalR": {
    "Enabled": true,
    "ConnectionString": "Endpoint=https://your-signalr-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;",
    "ApplicationName": "DriftMindWeb-Production"
  }
}
```

## Setting up Azure SignalR Service

### 1. Create Azure SignalR Service
```bash
# Using Azure CLI
az signalr create \
  --name your-signalr-service \
  --resource-group your-resource-group \
  --location West Europe \
  --sku Standard_S1
```

### 2. Get Connection String
```bash
az signalr key list \
  --name your-signalr-service \
  --resource-group your-resource-group \
  --query primaryConnectionString \
  --output tsv
```

### 3. Set Environment Variables (recommended for production)
```bash
export AzureSignalR__ConnectionString="Endpoint=https://your-signalr-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;"
export AzureSignalR__Enabled=true
```

## Benefits of Azure SignalR Service

### Scalability
- **Local SignalR**: Limited to a single server instance
- **Azure SignalR**: Supports horizontal scaling across multiple server instances

### Availability
- **Local SignalR**: Single point of failure
- **Azure SignalR**: High availability with automatic failover

### Performance
- **Local SignalR**: Limited by server resources
- **Azure SignalR**: Optimized for high throughput and low latency

## Deployment

### Local Development
```bash
dotnet run
# Automatically uses local SignalR
```

### Azure App Service
```bash
# Configure Connection String as App Setting:
# AzureSignalR__ConnectionString = "Endpoint=https://..."
# AzureSignalR__Enabled = true
```

### Docker
```dockerfile
# Set environment variables in container
ENV AzureSignalR__Enabled=true
ENV AzureSignalR__ConnectionString="Endpoint=https://..."
```

## Monitoring

The application displays the current SignalR mode on the `/signalr-info` page:
- 🟨 **Local**: Uses local SignalR
- 🟢 **Azure**: Uses Azure SignalR Service

## Troubleshooting

### Connection Issues
1. Verify Connection String
2. Check Azure SignalR Service status
3. Test network connectivity

### Performance Issues
1. Check Azure SignalR Service tier (Free/Standard/Premium)
2. Monitor concurrent connections limit
3. Use Azure Monitor for metrics

## Costs

| Service Tier | Concurrent Connections | Messages per Day | Cost (approx.) |
|--------------|----------------------|------------------|----------------|
| Free         | 20                   | 20,000           | €0.00          |
| Standard     | 1,000               | 1,000,000        | €40.00/month   |
| Premium      | 100,000             | 1,000,000        | €320.00/month  |

*Prices may vary - check current pricing in Azure Portal*
