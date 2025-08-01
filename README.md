# DriftMindWeb

A modern Blazor Server application for interacting with the DriftMind API - an intelligent document processing and search system based on Azure OpenAI and Azure AI Search.

## 🚀 Features

### 💬 Chat Interface
- **ChatGPT-like user interface** for natural conversations
- **Text Upload**: Direct insertion of text content
- **File Upload**: Support for `.txt`, `.md`, `.pdf`, `.docx` files
- **Configurable Upload Size** (Default: 3MB)
- **Semantic Search** in uploaded documents
- **AI-generated Answers** based on document contents

### 📁 Document Management
- **Overview of all documents** in the Azure AI Search database
- **Detailed document information** (size, type, chunk count)
- **Document content preview**
- **Delete function** for unnecessary documents
- **Pagination** for large document collections
- **Filter functions** by document type and ID

### 🎨 Design
- **Dark Mode** as default design
- **Responsive Layout** for desktop and mobile
- **Bootstrap 5** with custom components
- **Bootstrap Icons** for consistent iconography
- **Modern animations** and transitions

## 🛠️ Technical Details

### Tech Stack
- **Frontend**: Blazor Server (.NET 8.0)
- **UI Framework**: Bootstrap 5
- **HTTP Client**: HttpClient for API communication
- **Styling**: CSS with Dark Mode Theme
- **Deployment**: Docker-ready

### Architecture
```
DriftMindWeb/
├── Components/
│   ├── Layout/           # Layout components (Navigation, Header)
│   └── Pages/            # Page components (Chat, Documents, Home)
├── Services/             # API services and DTOs
├── wwwroot/              # Static files (CSS, Icons)
└── Properties/           # Launch configuration
```

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
    "BaseUrl": "https://your-driftmind-api-url.com",
    "MaxUploadSizeMB": 3,
    "Endpoints": {
      "Upload": "/upload",
      "UploadFile": "/upload/file",
      "Search": "/search",
      "Documents": "/documents"
    }
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

### Adjust Upload Size
The maximum upload size can be configured in `appsettings.json`:

```json
{
  "DriftMindApi": {
    "MaxUploadSizeMB": 5  // Changes the limit to 5MB
  }
}
```

### API Endpoints
All API endpoints are configurable and can be adapted to your DriftMind API implementation.

## 📖 Usage

### 1. Using the Chat Interface
1. Navigate to the **Chat** page
2. Choose between **Text Upload** or **File Upload**
3. Upload your documents
4. Ask questions about the uploaded content
5. Receive AI-generated answers based on your documents

### 2. Managing Documents
1. Visit the **Documents** page
2. View all uploaded documents
3. Use filters and search
4. Delete unnecessary documents

## 🔌 API Integration

The application communicates with the DriftMind API through the following endpoints:

- `POST /upload` - Text upload
- `POST /upload/file` - File upload
- `POST /search` - Document search
- `POST /documents` - Retrieve document list
- `DELETE /documents/{id}` - Delete document

## 🎯 Supported File Formats

- **Text**: `.txt`, `.md`
- **PDF**: `.pdf`
- **Microsoft Word**: `.docx`

## 🚦 Status & Roadmap

### ✅ Implemented
- Chat interface with text/file upload
- Document management
- Dark Mode design
- Configurable upload limits
- Responsive design

### 🔄 In Development
- Batch upload for multiple files
- Extended filter functions
- Export functions

### 🔮 Planned
- User authentication
- Document categorization
- Full-text editor for documents

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## 🔗 Links

- [DriftMind API Documentation](./README.DriftMind.md)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/)

## 💡 Support

For questions or issues, please open an [Issue](https://github.com/JustDanMan/DriftMindWeb/issues) on GitHub.

---

**DriftMindWeb** - Intelligent document processing with cutting-edge web technology 🚀