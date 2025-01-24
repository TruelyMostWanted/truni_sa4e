using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace XmasWishes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MirrorController : ControllerBase
{
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpPatch]
    [HttpDelete]
    public async Task<IActionResult> Mirror()
    {
        // Extrahiere HTTP-Method
        var method = HttpContext.Request.Method;

        // Extrahiere URL-Pfad
        var path = HttpContext.Request.Path;

        // Extrahiere Query-Parameter
        var queryParams = HttpContext.Request.Query;

        // Extrahiere Header
        var headers = HttpContext.Request.Headers;

        // Lese den Body (falls vorhanden)
        string body;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        // Erstelle ein JSON-Objekt für die Antwort
        var response = new
        {
            Method = method,
            Path = path,
            QueryParameters = queryParams,
            Headers = headers,
            Body = string.IsNullOrWhiteSpace(body) ? null : JsonSerializer.Deserialize<object>(body)
        };

        // Gebe die Antwort als JSON zurück
        return Ok(response);
    }
}