using Microsoft.AspNetCore.Mvc;
using XmasWishes.Models;

namespace XmasWishes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidatorController : ControllerBase
{
    private const int MaxWishLength = 250;

    [HttpPost]
    public IActionResult Validate([FromBody] ValidationRequest request)
    {
        Console.WriteLine($"AccessToken: {request.AccessToken}");
        Console.WriteLine($"Wish: {request.Wish.Description}");
        
        // Prüfen, ob AccessToken vorhanden ist
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            return BadRequest(new ValidationResponse
            {
                IsValid = false,
                Message = "AccessToken is missing",
                ValidatedWish = request.Wish,
            });
        }

        // Validieren der Länge des Wunsches
        if (string.IsNullOrEmpty(request.Wish.Description) || 
            request.Wish.Description.Length > MaxWishLength)
        {
            return BadRequest(new ValidationResponse
            {
                IsValid = false,
                Message = $"Wish is invalid. It must be between 1 and {MaxWishLength} characters long.",
                ValidatedWish = request.Wish
            });
        }

        // Wenn alles erfolgreich ist
        return Ok(new ValidationResponse
        {
            IsValid = true,
            Message = "Request is valid",
            ValidatedWish = request.Wish
        });
    }
}