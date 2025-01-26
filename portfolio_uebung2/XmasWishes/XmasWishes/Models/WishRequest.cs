using XmasWishes.Models.wishes;

namespace XmasWishes.Models;

public class WishRequest
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
    public string Status { get; set; } = Wish.StatusEnum.Formulated.ToString();

    public WishRequest(string description, string fileName)
    {
        Description = description;
        FileName = fileName;
    }

    public WishRequest()
    {
        
    }
}