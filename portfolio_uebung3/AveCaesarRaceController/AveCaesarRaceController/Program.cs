using AveCaesarRaceController.races;
using AveCaesarRaceController.tracks;

namespace AveCaesarRaceController;

public class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("[INFO] Starting AvaCaesar Race Controller");
        Task.Delay(6000).Wait();
        Console.WriteLine("[INFO] Preparing...");
        Task.Delay(3000).Wait();
        Console.WriteLine("[INFO] 5...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] 4...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] 3...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] 2...");
        Task.Delay(1000).Wait();
        Console.WriteLine("[INFO] 1...");
        Task.Delay(1000).Wait();
        
        //(1) First we create a KafkaClient especially for the Race related logic. 
        var raceKafkaClient = new RaceKafkaClient();
        
        //(2) It will try to create both the RACE_API and the SEGMENTS_API
        raceKafkaClient.CreateTopicAsync(Race.TOPIC_NAME).Wait();
        raceKafkaClient.CreateTopicAsync(TrackSegment.TOPIC_NAME).Wait();
        
        //(3) Subscribe to only the RACE_API
        raceKafkaClient.SubscribeToTopic(Race.TOPIC_NAME);

        //(4) Begin listening to messages on the RACE_API and keep the thread alive
        raceKafkaClient.BeginReceivingMessagesAsync();
        Console.WriteLine("[INFO] All Systems are ready!");
        while (!raceKafkaClient._CancellationTokenSource.IsCancellationRequested)
        {
            Task.Delay(5000).Wait();
        }
        
        //(5) If connections get closed, we need to clean up the resources
        raceKafkaClient.Close();
    }
}
