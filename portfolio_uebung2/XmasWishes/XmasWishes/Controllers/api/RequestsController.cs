using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using XmasWishes.Models;

namespace XmasWishes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _validatorEndpoint;
    private readonly string _wishesEndpoint;

    public RequestsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _validatorEndpoint = configuration["ServiceEndpoints:Validator"];
        _wishesEndpoint = configuration["ServiceEndpoints:Wishes"];
    }
    
    private ValidationResponse SendValidateRequest(WishRequest wish, string method)
    {
        try
        {
            var validationPayload = new
            {
                Method = method,
                Wish = wish
            };
        
            var content = new StringContent(
                JsonSerializer.Serialize(validationPayload), 
                Encoding.UTF8, 
                "application/json"
            );

            Console.WriteLine("SEND-VALIDATE-REQUEST (1)");
            var responseTask = _httpClient.PostAsync(_validatorEndpoint, content);
            responseTask.Wait();
            var response = responseTask.Result;
        
            Console.WriteLine("SEND-VALIDATE-REQUEST (2)");
            var responseContentTask = response.Content.ReadAsStringAsync();
            responseContentTask.Wait();
            var responseContent = responseContentTask.Result;
            Console.WriteLine(responseContent);

            Console.WriteLine("SEND-VALIDATE-REQUEST (3)");
            var jsonObject = JsonSerializer.Deserialize<ValidationResponse>(responseContent);
            Console.WriteLine(jsonObject);
        
            Console.WriteLine("SEND-VALIDATE-REQUEST (4)");
            return jsonObject;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    

    [HttpGet]
    public IActionResult GetWishes()
    {
        // Console.WriteLine("GET-WISHES (1)");
        // var validationTask = SendValidateRequest(null, "GET");
        // validationTask.Wait();
        // var validationResponse = validationTask.Result;
        //
        // Console.WriteLine("GET-WISHES (2)");
        // if (!validationResponse.IsValid)
        //     return BadRequest(validationResponse.Message);
        
        Console.WriteLine("GET-WISHES (3)");
        var dataServiceTask = _httpClient.GetAsync(_wishesEndpoint);
        dataServiceTask.Wait();
        var dataServiceResponse = dataServiceTask.Result;
        Console.WriteLine(dataServiceResponse);
        
        
        Console.WriteLine("GET-WISHES (4)");
        var resultTask = dataServiceResponse.Content.ReadAsStringAsync();
        resultTask.Wait();
        var result = resultTask.Result;
        Console.WriteLine(result);

        
        Console.WriteLine("GET-WISHES (5)");
        return Ok(JsonSerializer.Deserialize<object>(result));
    }

    [HttpPost]
    public async Task<IActionResult> AddWish([FromBody] WishRequest wish)
    {
        Console.WriteLine("ADD-WISH (1)");
        if (string.IsNullOrWhiteSpace(wish.Description))
            return BadRequest("Description cannot be empty.");

        Console.WriteLine("ADD-WISH (2)");
        var validationResponse = SendValidateRequest(wish, "POST");
        if (!validationResponse.IsValid)
            return BadRequest(validationResponse.Message);

        Console.WriteLine("ADD-WISH (3)");
        if (string.IsNullOrEmpty(wish.FileName))
            wish.FileName = " ";
        
        Console.WriteLine("ADD-WISH (4)");
        var content = new StringContent(JsonSerializer.Serialize(wish), Encoding.UTF8, "application/json");
        var dataServiceResponse = await _httpClient.PostAsync(_wishesEndpoint, content);
        var result = await dataServiceResponse.Content.ReadAsStringAsync();

        Console.WriteLine("ADD-WISH (5)");
        return Ok(JsonSerializer.Deserialize<object>(result));
    }

    [HttpPatch("wishes/{id}")]
    public async Task<IActionResult> UpdateWish(Guid id, [FromBody] WishRequest wish)
    {
        var validationResponse = SendValidateRequest(wish, "PATCH");
        if (!validationResponse.IsValid)
            return BadRequest(validationResponse.Message);

        var content = new StringContent(JsonSerializer.Serialize(wish), Encoding.UTF8, "application/json");
        var dataServiceResponse = await _httpClient.PatchAsync($"{_wishesEndpoint}/{id}", content);
        var result = await dataServiceResponse.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<object>(result));
    }

    [HttpDelete("wishes/{id}")]
    public async Task<IActionResult> DeleteWish(Guid id)
    {
        var validationResponse = SendValidateRequest(null, "DELETE");
        if (!validationResponse.IsValid)
            return BadRequest(validationResponse.Message);

        var dataServiceResponse = await _httpClient.DeleteAsync($"{_wishesEndpoint}/{id}");
        var result = await dataServiceResponse.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<object>(result));
    }
}