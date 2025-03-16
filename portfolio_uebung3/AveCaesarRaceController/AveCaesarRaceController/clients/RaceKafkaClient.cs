using System.Text.Json;
using AveCaesarRaceController.tokens;
using AveCaesarRaceController.tracks;
using Confluent.Kafka;

namespace AveCaesarRaceController.races;

public class RaceKafkaClient : KafkaClient
{
    public static readonly string CLIENT_ID = "RACE_CTRL";
    
    public delegate void RequestRaceDelegate(int laps, int segments, int players);
    public delegate void PlayerFinishedDelegate(PlayerToken playerToken);
    
    private event RequestRaceDelegate _RaceRequested;
    public event RequestRaceDelegate RaceRequested
    {
        add => _RaceRequested += value;
        remove => _RaceRequested -= value;
    }
    
    private event PlayerFinishedDelegate _PlayerFinished;
    public event PlayerFinishedDelegate PlayerFinished
    {
        add => _PlayerFinished += value;
        remove => _PlayerFinished -= value;
    }
    
    public Race CurrentRace { get; set; }
    public List<SegmentKafkaClient> SegmentClients { get; set; } = new();
    public List<PlayerToken> FinishedPlayers { get; set; } = new();
    
    public RaceKafkaClient() : base(KafkaClient.DefaultBootstrapServers, CLIENT_ID)
    {
        MessageReceived += _OnMessageReceived;
        RaceRequested += _OnRaceRequested;
    }
    
    private void _OnRaceRequested(int laps, int segments, int players)
    {
        if (CurrentRace != null)
        {
            Console.WriteLine($"[{ClientId}] ERROR_CREATING_RACE: There is already a race in progress.");
            return;
        }
        
        //(1) Generate a new track
        var tracksList = TrackGenerator.GenerateTracks(1, segments);
        var track1 = tracksList[0];
        var trackSegments = track1.segments;
        Console.WriteLine(JsonSerializer.Serialize(track1, JsonSerializerOptions.Web));

        //(2) Create a new race
        CurrentRace = new(0, laps, segments, players, track1);
        
        //(3) Create a new segment client for each segment
        for (var i = 0; i < segments; i++)
        {
            var senderId = (i == 0) ? this.ClientId : trackSegments[i-1].segmentId;
            var segmentClient = new SegmentKafkaClient(trackSegments[i]);
            SegmentClients.Add(segmentClient);
            
            segmentClient.GetAllTopics();
            segmentClient.SubscribeToTopic(TrackSegment.TOPIC_NAME);
            segmentClient.BeginReceivingMessagesAsync();
        }
        
        //(4) Create the players 
        var playerTokens = new List<PlayerToken>();
        for (var i = 0; i < players; i++)
        {
            var playerToken = new PlayerToken(
                playerId: i, 
                senderId: ClientId, 
                receiverId: track1.segments[0].segmentId, 
                currentLap: 0, 
                maxLaps: laps, 
                sentAt: DateTime.Now, 
                receivedAt: DateTime.MinValue, 
                totalTimeMs: 0
            );
            playerTokens.Add(playerToken);
            
            Task.Run(() => SegmentClients[0].SendMessageAsync(TrackSegment.TOPIC_NAME, playerToken.ToJson()));
        }
    }
    
    private void _OnStartRaceMessageReceived(string sender, string topic, string message, Partition partition, Offset offset)
    {
        Console.WriteLine($"[{ClientId}] RECOGNIZED: StartRaceMessage");

        var createRaceTopicTask = CreateTopicAsync(Race.TOPIC_NAME);
        createRaceTopicTask.Wait();
        var createSegmentsTopicTask = CreateTopicAsync(TrackSegment.TOPIC_NAME);
        createSegmentsTopicTask.Wait();
        Resubscribe();
        
        // Nachricht in Parameter parsen
        var startIndex = Race.START_PREFIX.Length + 1;
        var parametersString = message.Substring(startIndex);
        //Console.WriteLine($"[{ClientId}] PARAMETERS: {parametersString}");

        var parameters = parametersString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parameterPairs = parameters.Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToArray();
        var parameterDict = new Dictionary<string, string>();
        foreach (var entry in parameterPairs)
        {
            //Console.WriteLine($"[{ClientId}] PARAMETER: {entry[0]}={entry[1]}");
            parameterDict.Add(entry[0], entry[1]);
        }

        if (!parameterDict.TryGetValue("laps", out var lapsStr) ||
            !parameterDict.TryGetValue("segments", out var segmentsStr) ||
            !parameterDict.TryGetValue("players", out var playersStr))
        {
            Console.WriteLine($"[{ClientId}] PARAMETERS_ERROR: You need to provide all 3 parameters: laps, segments, players.");
            return;
        }
        
        if(!int.TryParse(lapsStr, out var laps) ||
           !int.TryParse(segmentsStr, out var segments) ||
           !int.TryParse(playersStr, out var players))
        {
            Console.WriteLine($"[{ClientId}] ERROR_INVALID_START_RACE_MESSAGE: Invalid or incomplete parameters.");
            return;
        }

        Console.WriteLine($"[{ClientId}] REQUESTING RACE: {laps} laps, {segments} segments, {players} players");
    
        // Ereignis auslösen
        _RaceRequested?.Invoke(laps, segments, players);
    }
    private void _OnPlayerFinishedMessageReceived(string sender, string topic, string message, Partition partition, Offset offset)
    {
        Console.WriteLine($"[{ClientId}] RECOGNIZED PlayerFinishedMessage");
        
        // PlayerToken aus der Nachricht extrahieren
        var playerToken = PlayerToken.FromJson(message);

        // Validierung der Nachricht
        if (playerToken == null || !sender.Equals(SegmentClients[0].Segment.segmentId))
        {
            Console.WriteLine($"[{ClientId}] ERROR_INVALID_FINISHED_RACE_MESSAGE: Sender is not the start-goal segment or message is malformed.");
            return;
        }

        // Rennen abgeschlossen: Ergebnis ausgeben
        Console.WriteLine($"[{ClientId}] Player {playerToken.PlayerID} finished the race: Time={playerToken.TotalTimeMs}ms, " +
                          $"Laps={playerToken.CurrentLap}/{playerToken.MaxLaps}");

        // Ereignis auslösen
        FinishedPlayers.Add(playerToken);
        _PlayerFinished?.Invoke(playerToken);
        
        // Wenn alle Spieler das Rennen beendet haben, dann das Rennen beenden
        if (FinishedPlayers.Count == CurrentRace.Players)
        {
            Console.WriteLine($"[{ClientId}] ALL_PLAYERS_FINISHED_RACE!");
            var json = JsonSerializer.Serialize(FinishedPlayers, JsonSerializerOptions.Web);
            Console.WriteLine($"[{ClientId}] json={json}");
            
            foreach(var client in SegmentClients)
                client.Close();
            
            CurrentRace = null;
            SegmentClients.Clear();
            FinishedPlayers.Clear();
        }
    }
    
    private void _OnMessageReceived(string sender, string topic, string message, Partition partition, Offset offset)
    {
        //(0) Listen to only "race_api" messages
        if(!topic.Equals(Race.TOPIC_NAME))
            return;
        
        //(1) IF: It was sent from the first segment? => It is a PlayerFinished message
        if (SegmentClients.Count > 0 && sender.Equals(SegmentClients[0].Segment.segmentId))
            _OnPlayerFinishedMessageReceived(sender, topic, message, partition, offset);
        
        //(2) ELSE IF: Message starts with "start_race"? => Its a StartRace message
        else if (message.StartsWith(Race.START_PREFIX))
            _OnStartRaceMessageReceived(sender, topic, message, partition, offset);
    }
}