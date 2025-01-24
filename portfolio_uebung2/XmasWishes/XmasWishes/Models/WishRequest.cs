using XmasWishes.Models.wishes;

namespace XmasWishes.Models;

public class WishRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; }
    public Wish.StatusEnum Status { get; set; } = Wish.StatusEnum.Formulated;

    public WishRequest(string description, Wish.StatusEnum status)
    {
        Description = description;
        Status = status;
    }

    public WishRequest()
    {
        
    }
}