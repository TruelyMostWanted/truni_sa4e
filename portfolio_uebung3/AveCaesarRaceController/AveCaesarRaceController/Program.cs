using System.Diagnostics;
using System.Text.Json;
using AveCaesarRaceController.races;
using Confluent.Kafka;

namespace AveCaesarRaceController;

public class Program
{
    private static void Main(string[] args)
    {
        var raceKafkaClient = new RaceKafkaClient();
        
        var createTopicTask1 = raceKafkaClient.CreateTopicAsync("race_api");
        createTopicTask1.Wait();
        var createTopicTask2 = raceKafkaClient.CreateTopicAsync("segments_api");
        createTopicTask2.Wait();
        
        raceKafkaClient.SubscribeToTopic("race_api");
        
        raceKafkaClient.BeginReceivingMessagesAsync();
        while (!raceKafkaClient._CancellationTokenSource.IsCancellationRequested)
        {
            
        }
        raceKafkaClient.Close();
    }
}
