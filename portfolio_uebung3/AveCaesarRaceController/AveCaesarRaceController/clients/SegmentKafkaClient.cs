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
        if(!playerToken.ReceiverID.Equals(Segment.segmentId))
            return;
        
        //(2) Set the ReceivedAt timestamp
        playerToken.ReceivedAt = DateTime.Now;
        
        //(3) Calculate the time between SentAt and ReceivedAt
        //and add it to the TotalTimeMs
        var diff = playerToken.ReceivedAt - playerToken.SentAt;
        playerToken.TotalTimeMs += (ulong)diff.TotalMilliseconds;
        
        //(4) Update Sender and Receiver
        playerToken.SenderID = Segment.segmentId;
        playerToken.ReceivedAt = DateTime.MinValue;
        
        //(5) Are we a "start-goal" segment? => Update the lap counter
        if (Segment.type.Equals(TrackSegment.TYPE_START_GOAL))
            playerToken.CurrentLap++;
        
        playerToken.SentAt = DateTime.Now;
        
        //(6) Who needs to receive it?
        //If below MaxLaps --> Next Segment
        if(playerToken.CurrentLap <= playerToken.MaxLaps)
        {
            var nextSegmentIndex = _GetRandomSegmentIndex();
            if (nextSegmentIndex == -1) 
                return;
            playerToken.ReceiverID = Segment.nextSegments[nextSegmentIndex];
            Task.Run(() => SendMessageAsync(TrackSegment.TOPIC_NAME, playerToken.ToJson()));
        }
        //else --> Race Controller
        else
        {
            Task.Run(() => SendMessageAsync(Race.TOPIC_NAME, playerToken.ToJson()));
        }
    }
    
    private void _OnMessageReceived(string sender, string topic, string msg, Partition partition, Offset offset)
    {
        try
        {
            var playerMessage = JsonSerializer.Deserialize<PlayerToken>(msg);
            if (playerMessage == null)
                return;
            _OnPlayerTokenMessageReceived(sender, topic, playerMessage, partition, offset);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{ClientId}] ERROR: {e.Message}");
        }
    }
}