﻿using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Text.Json;

/// <summary>
/// This class is a wrapper for the Confluent.Kafka library.
/// It provides methods to create, delete, subscribe and unsubscribe to topics, send and receive messages.
/// It also provides events for received messages.
/// It can be used as a base class for more specific Kafka clients.
/// </summary>
public class KafkaClient
{
    public delegate void MessageReceivedDelegate(string key, string topic, string message, Partition partition, Offset offset);
    
    public static readonly string OldBootstrapServers = "kafka:9092";
    public static readonly string DefaultBootstrapServers = "kafka1:9092,kafka2:9093,kafka3:9094";
    
    private event MessageReceivedDelegate _MessageReceived;
    public event MessageReceivedDelegate MessageReceived
    {
        add => _MessageReceived += value;
        remove => _MessageReceived -= value;
    }
    
    public string BootstrapServers { get; private set; }
    public string ClientId { get; private set; }
    public List<string> AvailableTopics { get; private set; } = new List<string>();
    public List<string> SubscribedTopics { get; private set; } = new List<string>();

    private IProducer<string, string> producer;
    private IConsumer<string, string>? consumer;
    
    public CancellationTokenSource _CancellationTokenSource;
    public Task _ReceivingTask;
    
    public KafkaClient(string bootstrapServers, string clientId)
    {
        BootstrapServers = bootstrapServers;
        ClientId = clientId;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            ClientId = ClientId
        };
        producer = new ProducerBuilder<string, string>(producerConfig).Build();
        
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = $"KafkaClientConsumerGroup-{ClientId}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }
    
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
                if (!AvailableTopics.Contains(topicName))
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

                Console.WriteLine($"[{ClientId}] Verfügbare Topics wurden erfolgreich abgerufen:");
                foreach (var topic in AvailableTopics)
                {
                    Console.WriteLine($" - {topic}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ClientId}] Fehler beim Abrufen der Topics: {ex.Message}");
            }
        }
    }
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
        
        SubscribedTopics.Add(topicName);
        
        consumer.Subscribe(SubscribedTopics);
        Console.WriteLine($"Das Topic '{topicName}' wurde erfolgreich abonniert.");
    }
    public void Resubscribe()
    {
        consumer?.Unsubscribe();
        consumer?.Subscribe(SubscribedTopics);
    }
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

    public async Task SendMessageAsync(string topicName, string messageString)
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
                Key = ClientId,
                Value = messageString
            };

            var deliveryReport = await producer.ProduceAsync(topicName, message);
            //Console.WriteLine($"Nachricht wurde erfolgreich von Client '{ClientId}' an das Topic '{topicName}' gesendet. Offset: {deliveryReport.Offset}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Senden der Nachricht an das Topic '{topicName}': {ex.Message}");
        }
    }
    public async Task SendMessageAsync(string topicName, object messageObject)
    {
        var objectString = JsonSerializer.Serialize(messageObject);
        await SendMessageAsync(topicName, objectString);
    }
    
    public async Task ReceiveMessagesAsync()
    {
        Console.WriteLine($"[{ClientId}]: Starts listening to messages on topics [{string.Join(", ", SubscribedTopics)}]");

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
                    
                    //Console.WriteLine($"[{ClientId}] ReceivedMessage:\n" +
                    //                  $"Key: {key}, Value: {message}, Topic: {topic}, Partition: {partition}, Offset: {offset}");
                    _MessageReceived?.Invoke(key, topic, message, partition, offset);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"[{ClientId}]: Empfang von Nachrichten wurde abgebrochen.");
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"[{ClientId}]: Fehler beim Empfangen der Nachricht: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{ClientId}]: Allgemeiner Fehler beim Empfangen der Nachricht: {ex.Message}");
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
    
    
    public void Close()
    {
        try
        {
            _CancellationTokenSource.Cancel();
            consumer?.Unsubscribe();
            producer?.Flush();
            producer?.Dispose();
            consumer?.Dispose();
            _ReceivingTask = null;
            Console.WriteLine($"[{ClientId}] CLOSED");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ClientId}] CLOSING_ERROR: {ex.Message}");
        }
    }
}