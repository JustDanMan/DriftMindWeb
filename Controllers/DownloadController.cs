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
        /// Generiert einen sicheren Download-Token für ein Dokument
        /// </summary>
        /// <param name="request">Token-Anfrage mit Dokument-ID</param>
        /// <returns>Download-Token Response</returns>
        [HttpPost("token")]
        public async Task<IActionResult> GetDownloadToken([FromBody] GetDownloadTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.DocumentId))
                {
                    return BadRequest(new { success = false, message = "DocumentId ist erforderlich" });
                }

                // Validierung der Expiration-Zeit (max 60 Minuten für Sicherheit)
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
        /// Lädt eine Datei über einen sicheren Token herunter
        /// </summary>
        /// <param name="request">Download-Anfrage mit Token</param>
        /// <returns>Datei zum Download</returns>
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
                    
                    // Setze Content-Type Header
                    Response.ContentType = downloadResponse.ContentType ?? "application/octet-stream";
                    
                    // Setze den Content-Disposition Header mit UTF-8 Encoding für korrekte Umlaute
                    if (!string.IsNullOrEmpty(downloadResponse.FileName))
                    {
                        // RFC 6266 konforme Encoding für Dateinamen mit Umlauten
                        // Erstelle ASCII-Version als Fallback für ältere Browser
                        var asciiFileName = downloadResponse.FileName
                            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
                            .Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue")
                            .Replace("ß", "ss");
                        
                        // Setze beide Header-Varianten für maximale Kompatibilität
                        Response.Headers["Content-Disposition"] = 
                            $"attachment; filename=\"{asciiFileName}\"; filename*=UTF-8''{Uri.EscapeDataString(downloadResponse.FileName)}";
                    }
                    else
                    {
                        Response.Headers["Content-Disposition"] = "attachment";
                    }
                    
                    // Rückgabe der Datei-Bytes direkt
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
        /// Alternative GET-Endpoint für direkten Download via URL mit Token als Query Parameter
        /// </summary>
        /// <param name="token">Download-Token</param>
        /// <returns>Datei zum Download</returns>
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
                    
                    // Setze Content-Type Header
                    Response.ContentType = downloadResponse.ContentType ?? "application/octet-stream";
                    
                    // Setze den Content-Disposition Header mit UTF-8 Encoding für korrekte Umlaute
                    if (!string.IsNullOrEmpty(downloadResponse.FileName))
                    {
                        // RFC 6266 konforme Encoding für Dateinamen mit Umlauten
                        // Erstelle ASCII-Version als Fallback für ältere Browser
                        var asciiFileName = downloadResponse.FileName
                            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
                            .Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue")
                            .Replace("ß", "ss");
                        
                        // Setze beide Header-Varianten für maximale Kompatibilität
                        Response.Headers["Content-Disposition"] = 
                            $"attachment; filename=\"{asciiFileName}\"; filename*=UTF-8''{Uri.EscapeDataString(downloadResponse.FileName)}";
                    }
                    else
                    {
                        Response.Headers["Content-Disposition"] = "attachment";
                    }
                    
                    // Rückgabe der Datei-Bytes direkt
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
