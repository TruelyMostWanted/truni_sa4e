using XmasWishes.Models.wishes;

namespace XmasWishes.Models;

public class ValidationRequest
{
    public string AccessToken { get; set; }
    public Wish Wish { get; set; }

    public ValidationRequest(string accessToken, Wish wish)
    {
        AccessToken = accessToken;
        Wish = wish;
    }
    public ValidationRequest()
    {
        
    }
}