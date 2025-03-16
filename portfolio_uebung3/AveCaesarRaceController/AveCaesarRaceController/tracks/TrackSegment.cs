namespace AveCaesarRaceController.tracks;

public class TrackSegment
{
    public static readonly string TOPIC_NAME = "segments_api";
    
    public static readonly string TYPE_NORMAL = "normal";
    public static readonly string TYPE_START_GOAL = "start-goal";

    
    public string segmentId { get; set; }
    public string type { get; set; }
    public List<string> nextSegments { get; set; }
}