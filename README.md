# DriftMindWeb

A modern Blazor Server application for interacting with the DriftMind API - an intelligent document processing and search system based on Azure OpenAI and Azure AI Search.

## 🚀 Features

### 💬 Chat Interface
- **ChatGPT-like user interface** for natural conversations with AI
- **Real-time communication** via SignalR (local or Azure SignalR Service)
- **Chat History**: Intelligent conversation context with configurable message limits
- **Text Upload**: Direct insertion of text content (up to 15KB)
- **File Upload**: Support for `.txt`, `.md`, `.pdf`, `.docx` files
- **Configurable Upload Size** (Default: 12MB, configurable up to any limit)
- **Semantic Search** in uploaded documents with AI-generated answers
- **Markdown Support**: Rich text formatting in AI responses
- **Progressive Loading**: Smooth chat experience with loading indicators
- **Duplicate Detection**: Smart handling of duplicate file uploads

### 📁 Document Management
- **Overview of all documents** in the Azure AI Search database
- **Detailed document information** (size, type, chunk count, metadata)
- **Document content preview** with chunk visualization
- **Secure Download System** with time-limited tokens
- **Delete function** for unnecessary documents
- **Advanced Pagination** with configurable page sizes and progressive loading
- **Filter functions** by document type, ID, and content
- **Bulk operations** support

### 🔄 Real-time Features (SignalR)
- **Local SignalR**: Single-instance real-time communication (default)
- **Azure SignalR Service**: Scalable, multi-instance deployment option
- **Connection Status**: Visual indicators for connection health
- **Automatic Reconnection**: Robust connection handling
- **Performance Optimized**: Enhanced for chat applications

### 🎨 Design & UX
- **Dark Mode** as default design with modern aesthetics
- **Responsive Layout** optimized for desktop, tablet, and mobile
- **Bootstrap 5** with custom components and themes
- **Bootstrap Icons** for consistent iconography
- **Modern animations** and smooth transitions
- **Accessibility Features**: Screen reader support and keyboard navigation
- **Progressive Enhancement**: Works with and without JavaScript

*📷 Visual examples of the application interface can be found in [docs/screenshots/](./docs/screenshots/)*

## 🛠️ Technical Details

### Tech Stack
- **Frontend**: Blazor Server (.NET 8.0) with Interactive Server Rendering
- **Real-time Communication**: SignalR (local or Azure SignalR Service)
- **UI Framework**: Bootstrap 5 with custom dark theme
- **HTTP Client**: Configured HttpClient for API communication
- **Styling**: Modern CSS with Dark Mode Theme and responsive design
- **Document Processing**: Multi-format support (PDF, DOCX, TXT, MD)
- **Security**: Secure download system with time-limited tokens
- **Data Protection**: Optional Azure Blob Storage for shared keys
- **Deployment**: Docker-ready with Azure integration

### Architecture
```
DriftMindWeb/
├── Components/
│   ├── Layout/           # Layout components (Navigation, MainLayout)
│   ├── Pages/            # Page components (Chat, Documents, Home, SignalR Info)
│   └── Shared/           # Shared components (Download, SignalR Status)
├── Controllers/          # Download controller for secure file access
├── Services/             # API services and business logic
│   ├── DriftMindApiService.cs    # Main API communication
│   ├── MarkdownService.cs        # Markdown processing for chat
│   └── SignalRService.cs         # SignalR connection management
├── Hubs/                 # SignalR hub configuration (if needed)
├── wwwroot/              # Static files (CSS, Images, Bootstrap)
└── Properties/           # Launch configuration and settings
```

### Key Features Implementation
- **Chat System**: Real-time messaging with history and context preservation
- **File Handling**: Secure upload/download with validation and preview
- **State Management**: Optimized Blazor state handling for chat applications
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Performance**: Optimized for large document collections and frequent chat usage

## 🔧 Installation & Setup

### Prerequisites
- .NET 8.0 SDK
- Access to a DriftMind API instance

### 1. Clone Repository
```bash
git clone https://github.com/JustDanMan/DriftMindWeb.git
cd DriftMindWeb
```

### 2. Adjust Configuration
Edit `appsettings.Development.json` or `appsettings.json`:

```json
{
  "DriftMindApi": {
    "BaseUrl": "http://localhost:5175",
    "Endpoints": {
      "Upload": "/upload",
      "Search": "/search", 
      "Documents": "/documents",
      "DownloadToken": "/download/token",
      "DownloadFile": "/download/file"
    },
    "MaxUploadSizeMB": 12
  },
  "ChatService": {
    "MaxSourcesForAnswer": 10,
    "MaxContextLength": 16000,
    "ChatHistoryEnabled": true,
    "MaxChatHistoryMessages": 10,
    "ChatHistoryContextPercentage": 30
  },
  "AzureSignalR": {
    "Enabled": false,
    "ConnectionString": "",
    "ApplicationName": "DriftMindWeb"
  },
  "SharedDataProtection": {
    "Enabled": false,
    "AzureStorage": {
      "ConnectionString": "",
      "ContainerName": "dataprotection-keys",
      "BlobName": "keys.xml"
    },
    "ApplicationName": "DriftMindWeb"
  },
  "DocumentsPage": {
    "InitialPageSize": 25,
    "EnableProgressiveLoading": true,
    "SkeletonCardCount": 3,
    "PreRenderingEnabled": true
  }
}
```

### Optional: Azure SignalR Service Setup
For production deployments with multiple instances, configure Azure SignalR:

```json
{
  "AzureSignalR": {
    "Enabled": true,
    "ConnectionString": "Endpoint=https://your-signalr-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;",
    "ApplicationName": "DriftMindWeb-Production"
  }
}
```

### Optional: Shared Data Protection (Multi-Instance)
For load-balanced deployments, configure shared data protection keys:

```json
{
  "SharedDataProtection": {
    "Enabled": true,
    "AzureStorage": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
      "ContainerName": "dataprotection-keys",
      "BlobName": "keys.xml"
    },
    "ApplicationName": "DriftMindWeb"
  }
}
```

### 3. Start Application
```bash
dotnet restore
dotnet build
dotnet run
```

The application will be available at `https://localhost:5001`.

## ⚙️ Configuration

### Core API Settings
Configure the connection to your DriftMind API instance:

```json
{
  "DriftMindApi": {
    "BaseUrl": "http://localhost:5175",      // Your DriftMind API URL
    "MaxUploadSizeMB": 12,                   // Maximum file upload size
    "Endpoints": {
      "Upload": "/upload",                   // File upload endpoint
      "Search": "/search",                   // Document search endpoint
      "Documents": "/documents",             // Document management endpoint
      "DownloadToken": "/download/token",    // Secure download token generation
      "DownloadFile": "/download/file"       // Secure file download endpoint
    }
  }
}
```

### Chat & AI Configuration
Control chat behavior and AI response generation:

```json
{
  "ChatService": {
    "MaxSourcesForAnswer": 10,               // Max document sources for AI answers
    "MaxContextLength": 16000,               // Maximum context length for AI
    "ChatHistoryEnabled": true,              // Enable conversation history
    "MaxChatHistoryMessages": 10,            // Max messages to keep in history
    "ChatHistoryContextPercentage": 30       // % of context used for chat history
  }
}
```

### SignalR Configuration
Choose between local SignalR (single instance) or Azure SignalR Service (scalable):

```json
{
  "AzureSignalR": {
    "Enabled": false,                        // Use local SignalR by default
    "ConnectionString": "",                  // Azure SignalR connection string
    "ApplicationName": "DriftMindWeb"        // Application identifier
  }
}
```

### Document Management
Configure the documents page behavior:

```json
{
  "DocumentsPage": {
    "InitialPageSize": 25,                   // Initial number of documents to load
    "EnableProgressiveLoading": true,        // Enable progressive loading
    "SkeletonCardCount": 3,                 // Number of skeleton cards while loading
    "PreRenderingEnabled": true             // Enable server-side pre-rendering
  }
}
```

### Production Settings

#### Azure SignalR Service (Recommended for Production)
For multi-instance deployments:

```json
{
  "AzureSignalR": {
    "Enabled": true,
    "ConnectionString": "Endpoint=https://your-signalr.service.signalr.net;AccessKey=your-key;Version=1.0;",
    "ApplicationName": "DriftMindWeb-Production"
  }
}
```

#### Shared Data Protection (Multi-Instance)
For load balancers and multiple instances:

```json
{
  "SharedDataProtection": {
    "Enabled": true,
    "AzureStorage": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=your-account;AccountKey=your-key;EndpointSuffix=core.windows.net",
      "ContainerName": "dataprotection-keys",
      "BlobName": "keys.xml"
    },
    "ApplicationName": "DriftMindWeb"
  }
}
```

## 🔌 API Integration

The application communicates with the DriftMind API through the following endpoints:

### Core Endpoints
- `POST /upload` - File and text upload with chunking support
- `POST /search` - Semantic document search with AI-generated answers
- `POST /documents` - Retrieve paginated document list with filtering
- `DELETE /documents/{id}` - Delete document and all associated chunks

### Secure Download System
- `POST /download/token` - Generate time-limited download tokens (15-60 minutes)
- `POST /download/file` - Download files using secure tokens

### Advanced Features
- **Chat History Integration**: Contextual conversations with previous message context
- **Bulk Operations**: Efficient handling of multiple documents
- **Metadata Management**: Rich document metadata with file size, type, and creation info
- **Error Handling**: Comprehensive error responses with user-friendly messages
- **Security**: Token-based authentication for file downloads with HMAC-SHA256 signatures

### Example API Communication
```csharp
// File Upload with advanced options
var uploadRequest = new
{
    file = selectedFile,
    documentId = "optional-custom-id",
    metadata = "Additional document information",
    chunkSize = 300,
    chunkOverlap = 20
};

// Search with Chat History
var searchRequest = new
{
    query = "User question",
    maxResults = 10,
    useSemanticSearch = true,
    includeAnswer = true,
    chatHistory = conversationHistory
};

// Secure Download
var tokenRequest = new { documentId = "doc-123", expirationMinutes = 15 };
var downloadRequest = new { token = "secure-token" };
```

## 🎯 Supported File Formats

### Document Types
- **Plain Text**: `.txt` files (up to 12MB)
- **Markdown**: `.md` files with full markdown support
- **PDF Documents**: `.pdf` files with text extraction
- **Microsoft Word**: `.docx` files with full document processing

### Text Input
- **Direct Text Entry**: Up to 15KB of text directly in the chat interface
- **Smart Truncation**: Automatic handling of oversized text with user warnings
- **Real-time Validation**: Live feedback on text length and formatting

### Upload Limits
- **File Size**: Configurable up to 12MB per file (default)
- **Text Length**: 15KB maximum for direct text input
- **File Types**: Validated against supported extensions
- **Content Validation**: Automatic file type detection and validation

## 📝 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## 🔗 Links

- [DriftMind API Documentation](./docs/README.DriftMind.md) - Complete API reference and setup guide
- [Azure SignalR Setup Guide](./docs/AZURE_SIGNALR_SETUP.md) - Detailed SignalR configuration
- [Shared Data Protection Setup](./docs/SHARED_DATA_PROTECTION_SETUP.md) - Multi-instance deployment guide

## 💡 Support

### Getting Help
For questions, issues, or feature requests:
- **GitHub Issues**: [Open an Issue](https://github.com/JustDanMan/DriftMindWeb/issues)

### Troubleshooting
Common issues and solutions:
- **Connection Issues**: Check SignalR status on the SignalR Info page
- **Upload Problems**: Verify file size limits and supported formats
- **API Connectivity**: Ensure DriftMind API is running and accessible
- **Performance**: Monitor document count and consider pagination settings

---

**DriftMindWeb** - Intelligent document processing with cutting-edge web technology 🚀

*Powered by Azure OpenAI, Blazor Server, and modern web standards*