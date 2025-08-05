# Data Protection Setup für horizontale Skalierung

Diese Konfiguration ermöglicht es, die ASP.NET Core Data Protection Keys in Azure Blob Storage zu speichern, um AntiforgeryValidationException-Fehler bei horizontaler Skalierung zu vermeiden.

## Konfiguration

### 1. appsettings.json / appsettings.{Environment}.json

```json
{
  "DataProtection": {
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

### 2. Konfigurationsparameter

- **`Enabled`**: Aktiviert/deaktiviert die Azure Blob Storage Data Protection
- **`ConnectionString`**: Azure Storage Account Connection String
- **`ContainerName`**: Name des Blob Containers (wird automatisch erstellt wenn nicht vorhanden)
- **`BlobName`**: Name der Blob-Datei für die Keys (Standard: "keys.xml")
- **`ApplicationName`**: Eindeutiger Name der Anwendung für Key-Isolation

## Funktionsweise

### Einzelne Instanz (Enabled: false)
- Data Protection Keys werden lokal im Dateisystem gespeichert
- Funktioniert nur bei einer einzigen App-Instanz
- Standard ASP.NET Core Verhalten

### Horizontale Skalierung (Enabled: true)
- Data Protection Keys werden in Azure Blob Storage gespeichert
- Alle App-Instanzen teilen sich dieselben Keys
- Verhindert AntiforgeryValidationException bei Load Balancing

## Deployment-Szenarien

### Entwicklung
```json
{
  "DataProtection": {
    "Enabled": false
  }
}
```

### Staging/Production (Single Instance)
```json
{
  "DataProtection": {
    "Enabled": false
  }
}
```

### Production (Multiple Instances)
```json
{
  "DataProtection": {
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

Die Implementierung enthält robuste Fehlerbehandlung:

1. **Missing Connection String**: Fallback auf lokale Data Protection
2. **Azure Storage Fehler**: Fallback auf lokale Data Protection mit Logging
3. **Container Creation**: Automatische Erstellung wenn nicht vorhanden

## Logs

Die Konfiguration gibt detaillierte Logs aus:
- Erfolgreiche Azure Blob Storage Konfiguration
- Fehlgeschlagene Konfiguration mit Fallback-Information
- Warnung bei fehlender Connection String

## Sicherheit

- Connection Strings sollten niemals in Source Control gespeichert werden
- Verwenden Sie Azure Key Vault oder Umgebungsvariablen für Production
- Der `ApplicationName` isoliert Keys zwischen verschiedenen Anwendungen

## Migration

Bestehende Anwendungen können diese Konfiguration ohne Downtime aktivieren:
1. `Enabled: false` → Keys bleiben lokal
2. `Enabled: true` → Neue Keys werden in Azure erstellt, alte Keys bleiben gültig bis sie ablaufen
