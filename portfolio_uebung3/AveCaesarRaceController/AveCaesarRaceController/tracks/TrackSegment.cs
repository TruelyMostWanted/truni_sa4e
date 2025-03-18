using AveCaesarRaceController.tokens;

namespace AveCaesarRaceController.tracks;

public class TrackSegment
{
    public static readonly string TOPIC_NAME = "segments_api";
    
    public static readonly string TYPE_NORMAL = "normal";
    public static readonly string TYPE_START_GOAL = "start-goal";
    //TASK 3: Add new segment types
    public static readonly string TYPE_CAESAR = "caesar";
    public static readonly string TYPE_BOTTLENECK = "bottleneck";
    
    //CUSTOM 1: Teleport to a specified segment
    public static readonly string TYPE_PORTAL = "portal";
    //CUSTOM 2: Jump N segments forward
    public static readonly string TYPE_JUMP = "jump";
    //CUSTOM 3: Go back to the start
    public static readonly string TYPE_JAIL = "jail";
    

    public string segmentId { get; set; }
    public string type { get; set; }
    public int MaxPlayers { get; set; }
    public List<PlayerToken> CurrentPlayers { get; set; } = new();

    public List<string> nextSegments { get; set; }

    public TrackSegment()
    {
        
    }
    public TrackSegment(string segmentId, string type, int maxPlayers, List<string> nextSegments)
    {
        this.segmentId = segmentId;
        this.type = type;
        MaxPlayers = maxPlayers;
        this.nextSegments = nextSegments;
    }
    
    public bool CanAddPlayer()
    {
        return CurrentPlayers.Count < MaxPlayers;
    }
    public bool TryRegisterPlayer(PlayerToken player)
    {
        if (!CanAddPlayer()) 
            return false;
        
        CurrentPlayers.Add(player);
        return true;
    }
    public void UnregisterPlayer(PlayerToken player)
    {
        CurrentPlayers.Remove(player);
    }
}