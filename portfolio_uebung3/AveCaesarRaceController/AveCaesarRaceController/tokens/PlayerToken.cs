using System.Text.Json;

namespace AveCaesarRaceController.tokens;

public class PlayerToken
{
    public int PlayerID { get; set; }
    public string SenderID { get; set; }
    public string ReceiverID { get; set; }
    public int CurrentLap { get; set; }
    public int MaxLaps { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime SentAt { get; set; }
    
    public ulong TotalTimeMs { get; set; }
    
    //TASK 3: Number of Caesar Greets
    public int CaesarGreets { get; set; }

    public PlayerToken()
    {
        
    }
    
    public static PlayerToken FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<PlayerToken>(json);   
        }
        catch (Exception e)
        {
            return null;
        }
    }
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

}