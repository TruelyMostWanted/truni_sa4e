using XmasWishes.Models.wishes;

namespace XmasWishes.Models;

public class ValidationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public Wish ValidatedWish { get; set; }

    public ValidationResponse(bool isValid, string message, Wish validatedWish)
    {
        IsValid = isValid;
        Message = message;
        ValidatedWish = validatedWish;
    }
    public ValidationResponse()
    {
        
    }
}