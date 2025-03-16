﻿using AveCaesarRaceController.races;
using AveCaesarRaceController.tracks;

namespace AveCaesarRaceController;

public class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("[INFO] Starting AvaCaesar Race Controller");
        Task.Delay(4000).Wait();
        Console.WriteLine("[INFO] 3...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] 2...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] 1...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] All Systems are ready!");
        
        var raceKafkaClient = new RaceKafkaClient();
        
        var createRaceApi = raceKafkaClient.CreateTopicAsync(Race.TOPIC_NAME);
        createRaceApi.Wait();
        
        var createSegmentsApi = raceKafkaClient.CreateTopicAsync(TrackSegment.TOPIC_NAME);
        createSegmentsApi.Wait();
        
        raceKafkaClient.SubscribeToTopic(Race.TOPIC_NAME);
        
        raceKafkaClient.BeginReceivingMessagesAsync();
        while (!raceKafkaClient._CancellationTokenSource.IsCancellationRequested)
        {
            
        }
        raceKafkaClient.Close();
    }
}
