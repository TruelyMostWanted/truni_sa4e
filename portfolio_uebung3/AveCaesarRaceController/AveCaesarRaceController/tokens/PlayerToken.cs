using System.Text.Json;

namespace AveCaesarRaceController.tokens;

public class PlayerToken
{
    public int PlayerID { get; set; }
    public string SenderID { get; set; }
    public string ReceiverID { get; set; }
    public int CurrentLap { get; set; }
    public int MaxLaps { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public ulong TotalTimeMs { get; set; }

    public PlayerToken()
    {
        
    }

    public PlayerToken(int playerId, string senderId, string receiverId, int currentLap, int maxLaps, DateTime sentAt, DateTime receivedAt, ulong totalTimeMs)
    {
        PlayerID = playerId;
        SenderID = senderId;
        ReceiverID = receiverId;
        CurrentLap = currentLap;
        MaxLaps = maxLaps;
        SentAt = sentAt;
        ReceivedAt = receivedAt;
        TotalTimeMs = totalTimeMs;
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