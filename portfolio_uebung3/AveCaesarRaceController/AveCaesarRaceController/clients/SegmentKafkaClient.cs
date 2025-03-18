using System.Text.Json;
using AveCaesarRaceController.tokens;
using AveCaesarRaceController.tracks;
using Confluent.Kafka;

namespace AveCaesarRaceController.races;

public class SegmentKafkaClient : KafkaClient
{
    public TrackSegment Segment { get; set; }
    
    public SegmentKafkaClient(TrackSegment segment) : base(DefaultBootstrapServers, segment.segmentId)
    {
        Segment = segment;
        MessageReceived += _OnMessageReceived;
    }

    private int _GetRandomSegmentIndex()
    {
        if (Segment.nextSegments.Count == 0)
            return -1;
        
        if (Segment.nextSegments.Count == 1)
            return 0;
        
        return new Random().Next(0, Segment.nextSegments.Count);
    }
    
    private void _OnPlayerTokenMessageReceived(string sender, string topic, PlayerToken playerToken, Partition partition, Offset offset)
    {
        //(1) Messages are only valid if the target matches our segmentId
        if (!playerToken.ReceiverID.Equals(ClientId))
        {
            //Console.WriteLine($"[{ClientId}] ERROR: Received message for wrong segment: {playerToken.ReceiverID}");
            return;
        }
        //else
        //{
        //    Console.WriteLine($"[{ClientId}] Received message (from:{playerToken.SenderID}/to:{playerToken.ReceiverID})");
        //}
        
        //(2) Set the ReceivedAt timestamp
        var receivedAt = DateTime.Now;
        
        //(2.1) TASK 3: Register the player in the segment
        var canAdd = Segment.TryRegisterPlayer(playerToken);
        
        //(3) Calculate the time between SentAt and ReceivedAt
        //and add it to the TotalTimeMs
        var diff = receivedAt - playerToken.SentAt;
        playerToken.TotalTimeMs += (ulong)diff.TotalMilliseconds;
        
        //(4) Update Sender and Receiver
        playerToken.SenderID = Segment.segmentId;
        
        //(5) Are we a "start-goal" segment? => Update the lap counter
        if (Segment.type.Equals(TrackSegment.TYPE_START_GOAL))
            playerToken.CurrentLap++;
        
        //(6.1) TASK 3: Is this a "Caesar" segment type? => Increment CaesarGreets
        if (Segment.type.Equals(TrackSegment.TYPE_CAESAR))
        {
            playerToken.CaesarGreets++;
        }
        //(6.2) TASK 3: Is this a "Bottleneck" segment type? => Wait for a random time between 1 and 5 seconds
        else if (Segment.type.Equals(TrackSegment.TYPE_BOTTLENECK))
        {
            var randomWait = new Random().Next(1000, 5000);
            playerToken.TotalTimeMs += (ulong)randomWait;
        }
        
        playerToken.SentAt = DateTime.Now;
        
        //(7) TASK 3: Unregister the player from the segment
        Segment.UnregisterPlayer(playerToken);
        
        //(8) Who needs to receive it?
        //If below MaxLaps --> Next Segment
        if(playerToken.CurrentLap <= playerToken.MaxLaps)
        {
            var nextSegmentIndex = _GetRandomSegmentIndex();
            if (nextSegmentIndex == -1) 
                return;
            playerToken.ReceiverID = Segment.nextSegments[nextSegmentIndex];
            
            var sendTask = SendMessageAsync(TrackSegment.TOPIC_NAME, playerToken.ToJson());
            sendTask.Wait();
        }
        //else --> Race Controller
        else
        {
            var sendTask = SendMessageAsync(Race.TOPIC_NAME, playerToken.ToJson());
            sendTask.Wait();
        }
    }
    
    private void _OnMessageReceived(string sender, string topic, string msg, Partition partition, Offset offset)
    {
        try
        {
            var playerMessage = JsonSerializer.Deserialize<PlayerToken>(msg);
            if (playerMessage == null)
            {
                Console.WriteLine($"[{ClientId}] ERROR: Could not deserialize message: {msg}");
                return;
            }
            _OnPlayerTokenMessageReceived(sender, topic, playerMessage, partition, offset);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{ClientId}] ERROR: {e.Message}");
        }
    }
}