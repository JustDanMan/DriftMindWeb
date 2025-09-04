using Microsoft.AspNetCore.Mvc;
using DriftMindWeb.Services;
using System.ComponentModel.DataAnnotations;

namespace DriftMindWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        private readonly IDriftMindApiService _apiService;
        private readonly ILogger<DownloadController> _logger;

        public DownloadController(IDriftMindApiService apiService, ILogger<DownloadController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

    /// <summary>
    /// Generates a secure download token for a document
    /// </summary>
    /// <param name="request">Token request with document ID</param>
    /// <returns>Download token response</returns>
        [HttpPost("token")]
        public async Task<IActionResult> GetDownloadToken([FromBody] GetDownloadTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.DocumentId))
                {
                    return BadRequest(new { success = false, message = "DocumentId ist erforderlich" });
                }

                // Validate expiration time (max 60 minutes for safety)
                var expirationMinutes = Math.Min(request.ExpirationMinutes, 60);
                if (expirationMinutes < 1) expirationMinutes = 15; // Default

                var tokenResponse = await _apiService.GetDownloadTokenAsync(request.DocumentId, expirationMinutes);

                if (tokenResponse?.Success == true)
                {
                    return Ok(new
                    {
                        success = true,
                        token = tokenResponse.Token,
                        documentId = tokenResponse.DocumentId,
                        expiresAt = tokenResponse.ExpiresAt,
                        downloadUrl = Url.Action("DownloadFile", "Download"), // Frontend Download URL
                        expirationMinutes = expirationMinutes
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to generate download token for document {DocumentId}", request.DocumentId);
                    return BadRequest(new { success = false, message = "Token konnte nicht generiert werden" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating download token for document {DocumentId}", request.DocumentId);
                return StatusCode(500, new { success = false, message = "Interner Server-Fehler" });
            }
        }

    /// <summary>
    /// Downloads a file using a secure token
    /// </summary>
    /// <param name="request">Download request with token</param>
    /// <returns>File for download</returns>
        [HttpPost("file")]
        public async Task<IActionResult> DownloadFile([FromBody] DownloadFileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return BadRequest(new { success = false, message = "Token ist erforderlich" });
                }

                var downloadResponse = await _apiService.DownloadFileAsync(request.Token);

                if (downloadResponse?.Success == true)
                {
                    _logger.LogInformation("File downloaded successfully: {FileName}", downloadResponse.FileName);
                    
                    // Set Content-Type header
                    Response.ContentType = downloadResponse.ContentType ?? "application/octet-stream";
                    
                    // Set the Content-Disposition header with UTF-8 encoding for correct umlauts
                    if (!string.IsNullOrEmpty(downloadResponse.FileName))
                    {
                        // RFC 6266 compliant encoding for filenames with umlauts
                        // Create ASCII version as fallback for older browsers
                        var asciiFileName = downloadResponse.FileName
                            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
                            .Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue")
                            .Replace("ß", "ss");
                        
                        // Set both header variants for maximum compatibility
                        Response.Headers["Content-Disposition"] = 
                            $"attachment; filename=\"{asciiFileName}\"; filename*=UTF-8''{Uri.EscapeDataString(downloadResponse.FileName)}";
                    }
                    else
                    {
                        Response.Headers["Content-Disposition"] = "attachment";
                    }
                    
                    // Return the file bytes directly
                    return File(downloadResponse.FileBytes, Response.ContentType);
                }
                else
                {
                    _logger.LogWarning("Failed to download file with provided token");
                    return BadRequest(new { success = false, message = "Download fehlgeschlagen - Token ungültig oder abgelaufen" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, new { success = false, message = "Interner Server-Fehler beim Download" });
            }
        }

    /// <summary>
    /// Alternative GET endpoint for direct download via URL with token as query parameter
    /// </summary>
    /// <param name="token">Download token</param>
    /// <returns>File for download</returns>
        [HttpGet("file")]
        public async Task<IActionResult> DownloadFileViaGet([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token ist erforderlich");
            }

            try
            {
                var downloadResponse = await _apiService.DownloadFileAsync(token);

                if (downloadResponse?.Success == true)
                {
                    _logger.LogInformation("File downloaded successfully via GET: {FileName}", downloadResponse.FileName);
                    
                    // Set Content-Type header
                    Response.ContentType = downloadResponse.ContentType ?? "application/octet-stream";
                    
                    // Set the Content-Disposition header with UTF-8 encoding for correct umlauts
                    if (!string.IsNullOrEmpty(downloadResponse.FileName))
                    {
                        // RFC 6266 compliant encoding for filenames with umlauts
                        // Create ASCII version as fallback for older browsers
                        var asciiFileName = downloadResponse.FileName
                            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
                            .Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue")
                            .Replace("ß", "ss");
                        
                        // Set both header variants for maximum compatibility
                        Response.Headers["Content-Disposition"] = 
                            $"attachment; filename=\"{asciiFileName}\"; filename*=UTF-8''{Uri.EscapeDataString(downloadResponse.FileName)}";
                    }
                    else
                    {
                        Response.Headers["Content-Disposition"] = "attachment";
                    }
                    
                    // Return the file bytes directly
                    return File(downloadResponse.FileBytes, Response.ContentType);
                }
                else
                {
                    _logger.LogWarning("Failed to download file with provided token via GET");
                    return BadRequest(new { success = false, message = "Download fehlgeschlagen - Token ungültig oder abgelaufen" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file via GET");
                return StatusCode(500, new { success = false, message = "Interner Server-Fehler beim Download" });
            }
        }
    }

    public class GetDownloadTokenRequest
    {
        [Required]
        public string DocumentId { get; set; } = "";
        
        [Range(1, 60)]
        public int ExpirationMinutes { get; set; } = 15;
    }

    public class DownloadFileRequest
    {
        [Required]
        public string Token { get; set; } = "";
    }
}
