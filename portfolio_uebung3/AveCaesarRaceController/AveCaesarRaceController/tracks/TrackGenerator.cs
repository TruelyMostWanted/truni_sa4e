using System.Text.Json;

namespace AveCaesarRaceController.tracks
{
    public class TrackGenerator
    {
        public static TrackList GenerateTracks(int numTracks, int lengthOfTrack)
        {
            var allTracks = new TrackList();

            for (int t = 1; t <= numTracks; t++)
            {
                var segments = new List<TrackSegment>();

                // First segment: start-and-goal-t
                string startSegmentId = $"start-and-goal-{t}";
                List<string> nextSegments;
                if (lengthOfTrack == 1)
                {
                    // Edge case: track length is 1 => no "normal" segments, loops onto itself
                    nextSegments = new List<string> { startSegmentId };
                }
                else
                {
                    nextSegments = new List<string> { $"segment-{t}-1" };
                }

                var startSegment = new TrackSegment
                {
                    segmentId = startSegmentId,
                    type = TrackSegment.TYPE_START_GOAL,
                    nextSegments = nextSegments
                };
                segments.Add(startSegment);

                // Create normal segments: segment-t-c for c in [1..(L-1)]
                for (int c = 1; c < lengthOfTrack; c++)
                {
                    string segId = $"segment-{t}-{c}";
                    List<string> nextSegs;

                    // If this is the last normal segment, it loops back to 'start-and-goal-t'
                    if (c == lengthOfTrack - 1)
                    {
                        nextSegs = new List<string> { startSegmentId };
                    }
                    else
                    {
                        nextSegs = new List<string> { $"segment-{t}-{c + 1}" };
                    }

                    var segment = new TrackSegment
                    {
                        segmentId = segId,
                        type = TrackSegment.TYPE_NORMAL,
                        nextSegments = nextSegs
                    };
                    segments.Add(segment);
                }

                var trackDefinition = new Track
                {
                    trackId = t,
                    segments = segments
                };
                allTracks.Add(trackDefinition);
            }

            return allTracks;
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
                TrackList tracksData = GenerateTracks(numTracks, lengthOfTrack);

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