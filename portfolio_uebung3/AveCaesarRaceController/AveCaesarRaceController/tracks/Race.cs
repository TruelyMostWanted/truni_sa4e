namespace AveCaesarRaceController.tracks;

public class Race
{
    public static readonly string TOPIC_NAME = "race_api";

    public static readonly string START_PREFIX = "start_race";
    public static readonly string STOP_PREFIX = "stop_race";
    
    public int ID { get; set; }
    public int Players { get; set; }
    public int Segments { get; set; }
    public int Laps { get; set; }
    public Track Track { get; set; }

    public Race(int id, int laps, int segments, int players, Track track)
    {
        ID = id;
        Players = players;
        Segments = segments;
        Laps = laps;
        Track = track;
    }
}