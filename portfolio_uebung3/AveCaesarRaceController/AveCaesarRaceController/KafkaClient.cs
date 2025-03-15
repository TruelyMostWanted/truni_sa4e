using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class KafkaClient
{
    public delegate void MessageReceivedDelegate(string key, string topic, string message, Partition partition, Offset offset);
    
    public static readonly string DefaultBootstrapServers = "kafka:9092";
    
    private event MessageReceivedDelegate _MessageReceived;
    public event MessageReceivedDelegate MessageReceived
    {
        add => _MessageReceived += value;
        remove => _MessageReceived -= value;
    }
    
    // Adresse des Kafka-Brokers
    public string BootstrapServers { get; private set; }

    // ID oder Key für den Client
    public string ClientId { get; private set; }

    // Liste aller verfügbaren Topics
    public List<string> AvailableTopics { get; private set; } = new List<string>();

    // Liste der Topics, an die wir uns registriert haben
    public List<string> SubscribedTopics { get; private set; } = new List<string>();

    private IProducer<string, string> producer; // Kafka Producer
    private IConsumer<string, string>? consumer; // Kafka Consumer
    
    public CancellationTokenSource _CancellationTokenSource;
    public Task _ReceivingTask; // Task für den Empfang von Nachrichten

    // Angepasster Konstruktor, der auch eine ClientId benötigt
    public KafkaClient(string bootstrapServers, string clientId)
    {
        BootstrapServers = bootstrapServers;
        ClientId = clientId;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            ClientId = ClientId // Der Client-Id wird in der Config hinterlegt.
        };
        producer = new ProducerBuilder<string, string>(producerConfig).Build();
        
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = $"KafkaClientConsumerGroup-{ClientId}", // Gruppen-ID umfasst die ClientId
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    /// <summary>
    /// Erstellt ein Kafka Topic.
    /// </summary>
    public async Task CreateTopicAsync(string topicName, int numPartitions = 1, short replicationFactor = 1)
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build())
        {
            try
            {
                var topicSpec = new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = numPartitions,
                    ReplicationFactor = replicationFactor
                };

                await adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });
                AvailableTopics.Add(topicName);
                Console.WriteLine($"Kafka Topic '{topicName}' wurde erfolgreich erstellt.");
            }
            catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                Console.WriteLine($"Kafka Topic '{topicName}' existiert bereits.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Erstellen des Kafka Topics: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Löscht ein Kafka Topic.
    /// </summary>
    public async Task DeleteTopicAsync(string topicName)
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build())
        {
            try
            {
                await adminClient.DeleteTopicsAsync(new List<string> { topicName });
                AvailableTopics.Remove(topicName);
                SubscribedTopics.Remove(topicName);
                Console.WriteLine($"Kafka Topic '{topicName}' wurde erfolgreich gelöscht.");
            }
            catch (DeleteTopicsException ex)
            {
                Console.WriteLine($"Fehler beim Löschen des Kafka Topics: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Aktualisiert die Liste aller verfügbaren Topics in Kafka.
    /// </summary>
    public void GetAllTopics()
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build())
        {
            try
            {
                // Metadaten vom Broker abrufen
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

                // Extrahiere die Namen aller Topics und aktualisiere die Liste
                AvailableTopics = metadata.Topics.Select(t => t.Topic).ToList();

                Console.WriteLine("Verfügbare Topics wurden erfolgreich abgerufen:");
                foreach (var topic in AvailableTopics)
                {
                    Console.WriteLine($" - {topic}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der Topics: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Abonniert ein Topic lokal und bei Kafka.
    /// </summary>
    public void SubscribeToTopic(string topicName)
    {
        if (!AvailableTopics.Contains(topicName))
        {
            Console.WriteLine($"Topic '{topicName}' existiert nicht.");
            return;
        }

        if (SubscribedTopics.Contains(topicName))
        {
            Console.WriteLine($"Du bist bereits zum Topic '{topicName}' registriert.");
            return;
        }

        // Hinzufügen zur Liste der abonnierten Topics
        SubscribedTopics.Add(topicName);
        
        // Bei Kafka subscriben
        consumer.Subscribe(SubscribedTopics);
        Console.WriteLine($"Das Topic '{topicName}' wurde erfolgreich abonniert.");
    }

    /// <summary>
    /// Unsubscribe von einem Topic.
    /// </summary>
    public void UnsubscribeFromTopic(string topicName)
    {
        if (!SubscribedTopics.Contains(topicName))
        {
            Console.WriteLine($"Du bist nicht zum Topic '{topicName}' registriert.");
            return;
        }

        SubscribedTopics.Remove(topicName);
        consumer?.Unsubscribe();
        consumer?.Subscribe(SubscribedTopics);
        Console.WriteLine($"Das Topic '{topicName}' wurde erfolgreich abbestellt.");
    }

    /// <summary>
    /// Sendet eine Nachricht an ein bestimmtes Topic.
    /// </summary>
    public async Task SendMessageAsync(string topicName, string key, string value)
    {
        if (!AvailableTopics.Contains(topicName))
        {
            Console.WriteLine($"Topic '{topicName}' existiert nicht. Nachricht wurde nicht gesendet.");
            return;
        }

        try
        {
            var message = new Message<string, string>
            {
                Key = key, // Genutzter Schlüssel
                Value = value
            };

            var deliveryReport = await producer.ProduceAsync(topicName, message);
            Console.WriteLine($"Nachricht wurde erfolgreich von Client '{ClientId}' an das Topic '{topicName}' gesendet. Offset: {deliveryReport.Offset}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Senden der Nachricht an das Topic '{topicName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Empfängt kontinuierlich Nachrichten von abonnierten Topics.
    /// </summary>
    public async Task ReceiveMessagesAsync()
    {
        Console.WriteLine($"Starte den Konsum von Topics: {string.Join(", ", SubscribedTopics)}");

        try
        {
            while (!_CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, string> consumeResult = consumer.Consume(_CancellationTokenSource.Token);
                    var key = consumeResult.Message.Key;
                    var message = consumeResult.Message.Value;
                    var topic = consumeResult.Topic;
                    var partition = consumeResult.Partition;
                    var offset = consumeResult.Offset;
                    
                    Console.WriteLine($"[{ClientId}] ReceivedMessage:\n" +
                                      $"Key: {key}, Value: {message}, Topic: {topic}, Partition: {partition}, Offset: {offset}");
                    
                    _MessageReceived?.Invoke(key, topic, message, partition, offset);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Empfang von Nachrichten wurde abgebrochen.");
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Fehler beim Empfangen der Nachricht: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Allgemeiner Fehler beim Empfangen der Nachricht: {ex.Message}");
                }
            }
        }
        finally
        {
            consumer?.Close();
            Console.WriteLine("Kafka Consumer wurde beendet.");
        }
    }

    public bool BeginReceivingMessagesAsync()
    {
        if (_ReceivingTask is not null)
            return false;

        _CancellationTokenSource = new CancellationTokenSource();
        _ReceivingTask = Task.Run(ReceiveMessagesAsync, _CancellationTokenSource.Token);
        return true;
    }
    public bool StopReceivingMessagesAsync()
    {
        if (_ReceivingTask is null)
            return false;

        _CancellationTokenSource.Cancel();
        _ReceivingTask.Wait();
        _ReceivingTask = null;
        return true;
    }
    
    /// <summary>
    /// Schließt den Producer.
    /// </summary>
    public void Close()
    {
        producer?.Flush();
        producer?.Dispose();
        consumer?.Dispose();
        Console.WriteLine("Kafka Client wurde geschlossen.");
    }
}