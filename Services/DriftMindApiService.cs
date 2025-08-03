using System.Text.Json;

namespace DriftMindWeb.Services
{
    public interface IDriftMindApiService
    {
        Task<FileUploadResponse?> UploadTextAsFileAsync(string text, string? documentId = null, string? metadata = null, int chunkSize = 300, int chunkOverlap = 20);
        Task<FileUploadResponse?> UploadFileAsync(Stream fileStream, string fileName, string? documentId = null, string? metadata = null, int chunkSize = 300, int chunkOverlap = 20);
        Task<SearchResponse?> SearchAsync(SearchRequest request);
        Task<DocumentListResponse?> GetDocumentsAsync(int maxResults = 50, int skip = 0, string? documentIdFilter = null);
        Task<bool> DeleteDocumentAsync(string documentId);
        Task<DownloadTokenResponse?> GetDownloadTokenAsync(string documentId, int expirationMinutes = 15);
        Task<DownloadFileResponse?> DownloadFileAsync(string token);
    }

    public class DriftMindApiService : IDriftMindApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DriftMindApiService> _logger;
        private readonly string _baseUrl;

        public DriftMindApiService(HttpClient httpClient, IConfiguration configuration, ILogger<DriftMindApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["DriftMindApi:BaseUrl"] ?? "http://localhost:5175";
        }

        public async Task<FileUploadResponse?> UploadTextAsFileAsync(string text, string? documentId = null, string? metadata = null, int chunkSize = 300, int chunkOverlap = 20)
        {
            try
            {
                // Erstelle einen eindeutigen Dateinamen
                var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Erste 8 Zeichen der GUID
                var fileName = $"QuickNotes-{uniqueId}.txt";
                
                // Konvertiere Text zu Stream
                var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
                using var textStream = new MemoryStream(textBytes);
                
                // Verwende die bestehende UploadFileAsync Methode
                return await UploadFileAsync(textStream, fileName, documentId, metadata, chunkSize, chunkOverlap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading text as file");
                return null;
            }
        }

        public async Task<FileUploadResponse?> UploadFileAsync(Stream fileStream, string fileName, string? documentId = null, string? metadata = null, int chunkSize = 300, int chunkOverlap = 20)
        {
            try
            {
                var endpoint = _configuration["DriftMindApi:Endpoints:Upload"] ?? "/uploads";
                var url = $"{_baseUrl}{endpoint}";

                using var content = new MultipartFormDataContent();
                
                // File content
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "file", fileName);
                
                // Optional parameters
                if (!string.IsNullOrEmpty(documentId))
                    content.Add(new StringContent(documentId), "documentId");
                if (!string.IsNullOrEmpty(metadata))
                    content.Add(new StringContent(metadata), "metadata");
                content.Add(new StringContent(chunkSize.ToString()), "chunkSize");
                content.Add(new StringContent(chunkOverlap.ToString()), "chunkOverlap");

                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<FileUploadResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    _logger.LogError("File upload failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return null;
            }
        }

        public async Task<SearchResponse?> SearchAsync(SearchRequest request)
        {
            try
            {
                var endpoint = _configuration["DriftMindApi:Endpoints:Search"] ?? "/search";
                var url = $"{_baseUrl}{endpoint}";
                
                // Debug-Logging für ChatHistory
                if (request.ChatHistory?.Count > 0)
                {
                    _logger.LogInformation("Sending chat history with {Count} messages to API", request.ChatHistory.Count);
                    foreach (var item in request.ChatHistory)
                    {
                        _logger.LogInformation("ChatHistory: {Role} - {Content}", item.Role, 
                            item.Content.Length > 100 ? item.Content.Substring(0, 100) + "..." : item.Content);
                    }
                }
                else
                {
                    _logger.LogInformation("No chat history being sent to API");
                }
                
                var response = await _httpClient.PostAsJsonAsync(url, request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    _logger.LogError("Search failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search");
                return null;
            }
        }

        public async Task<DocumentListResponse?> GetDocumentsAsync(int maxResults = 50, int skip = 0, string? documentIdFilter = null)
        {
            try
            {
                var endpoint = _configuration["DriftMindApi:Endpoints:Documents"] ?? "/documents";
                var url = $"{_baseUrl}{endpoint}";

                var request = new DocumentListRequest
                {
                    MaxResults = maxResults,
                    Skip = skip,
                    DocumentIdFilter = documentIdFilter
                };

                var response = await _httpClient.PostAsJsonAsync(url, request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<DocumentListResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    _logger.LogError("Get documents failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return null;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string documentId)
        {
            try
            {
                var endpoint = _configuration["DriftMindApi:Endpoints:Documents"] ?? "/documents";
                var url = $"{_baseUrl}{endpoint}/{documentId}";
                
                var response = await _httpClient.DeleteAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogError("Delete document failed with status code: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID: {DocumentId}", documentId);
                return false;
            }
        }

        public async Task<DownloadTokenResponse?> GetDownloadTokenAsync(string documentId, int expirationMinutes = 15)
        {
            try
            {
                var endpoint = _configuration["DriftMindApi:Endpoints:DownloadToken"] ?? "/download/token";
                var url = $"{_baseUrl}{endpoint}";

                var request = new DownloadTokenRequest
                {
                    DocumentId = documentId,
                    ExpirationMinutes = expirationMinutes
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<DownloadTokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    _logger.LogError("Download token generation failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating download token for document {DocumentId}", documentId);
                return null;
            }
        }

        public async Task<DownloadFileResponse?> DownloadFileAsync(string token)
        {
            try
            {
                var endpoint = _configuration["DriftMindApi:Endpoints:DownloadFile"] ?? "/download/file";
                var url = $"{_baseUrl}{endpoint}";

                var request = new DownloadFileRequest { Token = token };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = GetFileNameFromResponse(response) ?? "download";
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                    return new DownloadFileResponse
                    {
                        FileBytes = fileBytes,
                        FileName = fileName,
                        ContentType = contentType,
                        Success = true
                    };
                }
                else
                {
                    _logger.LogError("File download failed with status code: {StatusCode}", response.StatusCode);
                    return new DownloadFileResponse { Success = false };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file with token");
                return new DownloadFileResponse { Success = false };
            }
        }

        private string? GetFileNameFromResponse(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                return response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }
            return null;
        }
    }

    // DTOs für die API-Kommunikation
    public class FileUploadResponse
    {
        public string DocumentId { get; set; } = "";
        public int ChunksCreated { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileType { get; set; } = "";
        public long FileSizeBytes { get; set; }
    }

    public class SearchRequest
    {
        public string Query { get; set; } = "";
        public int MaxResults { get; set; } = 10;
        public bool UseSemanticSearch { get; set; } = true;
        public string? DocumentId { get; set; }
        public bool IncludeAnswer { get; set; } = true;
        public List<ChatMessage>? ChatHistory { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = ""; // "user" oder "assistant"
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class SearchResponse
    {
        public string Query { get; set; } = "";
        public List<SearchResult> Results { get; set; } = new();
        public string? GeneratedAnswer { get; set; }
        public bool Success { get; set; }
        public int TotalResults { get; set; }
    }

    public class SearchResult
    {
        public string Id { get; set; } = "";
        public string Content { get; set; } = "";
        public string DocumentId { get; set; } = "";
        public int ChunkIndex { get; set; }
        public double Score { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSizeBytes { get; set; }
        public bool IsFileAvailable { get; set; }
    }

    // Download DTOs
    public class DownloadTokenRequest
    {
        public string DocumentId { get; set; } = "";
        public int ExpirationMinutes { get; set; } = 15;
    }

    public class DownloadTokenResponse
    {
        public string Token { get; set; } = "";
        public string DocumentId { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public string DownloadUrl { get; set; } = "";
        public bool Success { get; set; }
    }

    public class DownloadFileRequest
    {
        public string Token { get; set; } = "";
    }

    public class DownloadFileResponse
    {
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public bool Success { get; set; }
    }

    // Document Management DTOs
    public class DocumentListRequest
    {
        public int MaxResults { get; set; } = 50;
        public int Skip { get; set; } = 0;
        public string? DocumentIdFilter { get; set; }
    }

    public class DocumentListResponse
    {
        public List<DocumentInfo> Documents { get; set; } = new();
        public int TotalDocuments { get; set; }
        public int ReturnedDocuments { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class DocumentInfo
    {
        public string DocumentId { get; set; } = "";
        public int ChunkCount { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> SampleContent { get; set; } = new();
    }
}
