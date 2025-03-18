using System.Text.Json;

namespace AveCaesarRaceController.tracks
{
    public class TrackGenerator
    {
        /// <summary>
        /// Wenn numTracks <= 0 || lengthOfTrack <= 0, dann wird eine leere TrackList zurückgegeben.
        /// Wenn numTracks == 1
        ///     WENN lengthOfTrack == 1:
        ///     DANN Gebe eine TrackList mit 1 Track und 1 Segment zurück
        ///     ANSONSTEN
        ///     - Das 1. Segment ist immer "start-goal" und die weiteren sind gemischt aus "Normal", "Caesar"
        ///     - 
        /// </summary>
        public static List<Track> GenerateTracks(int numTracks, int segmentsCount)
        {
            //(0) Check for invalid parameters
            if (numTracks <= 0 || segmentsCount <= 0)
                return [];
            
            //(1) Create the resulting tracks list
            var tracksList = new List<Track>(capacity: numTracks);
            
            //(2) Generate the tracks
            for (int trackId = 1; trackId <= numTracks; trackId++)
            {
                var segments = new List<TrackSegment>();
                
                //(2.1) First segment: start-and-goal-t
                var startGoalSegment = new TrackSegment()
                {
                    segmentId = "start-and-goal-" + trackId,
                    type = TrackSegment.TYPE_START_GOAL,
                    nextSegments = [],
                    MaxPlayers = 1_000_000
                };
                segments.Add(startGoalSegment);

                //(2.2) If there are no more segments, then the start-and-goal segment loops onto itself
                if (segmentsCount == 1)
                {
                    startGoalSegment.nextSegments.Add(startGoalSegment.segmentId);
                    tracksList.Add(new Track(){ trackId = trackId, segments = segments });
                    continue;
                }
                else
                {
                    startGoalSegment.nextSegments.Add($"segment-{trackId}-1");
                }
                
                //(2.3) Create normal segments: segment-t-c for c in [1..(L-1)]
                for (int segmentId = 1; segmentId < segmentsCount; segmentId++)
                {
                    //(2.3.1) Define the new segment
                    var segment = new TrackSegment()
                    {
                        segmentId = $"segment-{trackId}-{segmentId}",
                        type = TrackSegment.TYPE_NORMAL,
                        nextSegments = new(capacity: 4),
                        MaxPlayers = 1
                    };
                    
                    //(2.3.2) Add the segment to the track
                    segments.Add(segment);
                    
                    //(2.3.3) Are we at the last segment?
                    if (segmentId == segmentsCount - 1)
                        segment.nextSegments.Add(startGoalSegment.segmentId);
                    else
                        segment.nextSegments.Add($"segment-{trackId}-{segmentId + 1}");
                    
                    var random = new Random();
                    
                    //(2.3.4) There is a 10% chance that the field is of type "Caesar" instead of "Normal"
                    if (random.Next(0, 100) < 15)
                        segment.type = TrackSegment.TYPE_CAESAR;
                    //(2.3.5) There is a 15% chance that the field is of type "Bottleneck" instead of "Normal"
                    else if (random.Next(0, 100) < 20)
                        segment.type = TrackSegment.TYPE_BOTTLENECK;
                    //(2.3.6) There is a 5% chance that the field is of type "Portal" instead of "Normal"
                    else if (random.Next(0, 100) < 5)
                        segment.type = TrackSegment.TYPE_PORTAL;
                    //(2.3.7) There is a 3% chance that the field is of type "Jump" instead of "Normal"
                    else if (random.Next(0, 100) < 2)
                        segment.type = TrackSegment.TYPE_JUMP;
                }

                //(2.4) Create a new Track storing the segments
                var trackDefinition = new Track
                {
                    trackId = trackId,
                    segments = segments
                };
                tracksList.Add(trackDefinition);
            }

            return tracksList;
        }

        public static void TestMain(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <num_tracks> <length_of_track> <output_file>");
                Environment.Exit(1);
            }

            if (!int.TryParse(args[0], out int numTracks))
            {
                Console.WriteLine("Please provide valid integer value for num_tracks.");
                Environment.Exit(1);
            } 
            if(!int.TryParse(args[1], out int lengthOfTrack))
            {
                Console.WriteLine("Please provide valid integer value for length_of_track.");
                Environment.Exit(1);
            }

            string outputFile = args[2];

            try
            {
                List<Track> tracksData = GenerateTracks(numTracks, lengthOfTrack);

                // Serialize to JSON
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(tracksData, options);

                // Write to file
                File.WriteAllText(outputFile, json);
                Console.WriteLine($"Successfully generated {numTracks} track(s) of length {lengthOfTrack} into '{outputFile}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}