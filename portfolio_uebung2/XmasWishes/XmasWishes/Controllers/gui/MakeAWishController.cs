using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using XmasWishes.Models;

namespace XmasWishes.Controllers;

public class GuiController : Controller
{
        
    [Route("gui/make-a-wish")]
    public IActionResult MakeAWish()
    {
        return View();
    }

        
    [HttpPost]
    [Route("gui/make-a-wish")]
    public async Task<IActionResult> SubmitWish(WishRequest wishRequest)
    {
        if (string.IsNullOrWhiteSpace(wishRequest.Description))
        {
            ModelState.AddModelError("Description", "Description cannot be empty.");
            return View("MakeAWish");
        }
        
        var target = new Uri("http://172.19.0.10:8080/api/requests");
        var method = HttpMethod.Post;
        var json = JsonSerializer.Serialize(wishRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        using var httpClient = new HttpClient();
        
        var response = await httpClient.PostAsync(target, content);
        if (response.IsSuccessStatusCode)
        {
            ViewBag.Message = "Wish submitted successfully!";
        }
        else
        {
            ViewBag.Message = "Error submitting wish.";
        }

        return View("MakeAWish");
    }
}