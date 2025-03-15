# Projektplan: AveCaesar

## 1) Genutzte Docker-Images

Folgende Docker-Images werden verwendet:
- **Kafka**: Message-Broker für die Kommunikation zwischen den Services
- **Zookeeper**: Koordinationsdienst für Kafka
- **RaceController**: Verantwortlich für das Rennen und das Management der Segment-Services
- **SegmentService**: Einzelne Streckenabschnitte, die die Token (Spieler) verarbeiten

## 2) Selbst erstellte Docker-Container
[readme.md](readme.md)
- **RaceController** (C#/.NET): Verwaltet das Rennen und kommuniziert über Kafka
- **SegmentService** (C#/.NET): Wird dynamisch für jedes Segment erzeugt und verarbeitet Spieler-Token

## 3) Container-Funktionalitäten

- **RaceController**:
  - Empfängt CLI-Befehle über Kafka
  - Erstellt die Rennstrecke
  - Startet die benötigten SegmentServices
  - Verwaltet das Weiterreichen der Spieler-Tokens
  - Gibt am Ende das Rennergebnis aus

- **SegmentService**:
  - Erhält Token von Kafka
  - Aktualisiert Token-Informationen (Zeit, Segment-Position)
  - Leitet Token an das nächste Segment weiter
  
## 4) Projektstruktur

```
AveCaesar/
│── docker-compose.yaml
│── start_project.bat
│── RaceController/
│   ├── Program.cs
│   ├── KafkaClient.cs
│   ├── TrackGenerator.cs
│   ├── TokenManager.cs
│── SegmentService/
│   ├── Program.cs
│   ├── KafkaListener.cs
│   ├── SegmentProcessor.cs
```

## 5) Ausführung des Projekts

### 5.0) Aufbau der Docker Compose-Datei

```yaml
version: '3.8'

services:
  # Zookeeper Service (Container 1)
  zookeeper:
    image: wurstmeister/zookeeper
    container_name: zookeeper
    ports:
      - "2181:2181"
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.10

  # Kafka Broker (Container 2)
  kafka:
    image: wurstmeister/kafka
    container_name: kafka
    ports:
      - "9092:9092"
    environment:
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
    depends_on:
      - zookeeper
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.11

networks:
  avecaesar-net:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

### 5.1) Starten der statischen Docker-Container

- Starte die benötigten Container mit:
  ```sh
  docker-compose up -d
  ```
- Dies startet Kafka, Zookeeper und den RaceController.

- Alternativ kann eine Batch-Datei genutzt werden:
  ```shell
  @echo off
  cd /d %~dp0
  docker-compose -f compose.yaml up
  pause
  ```

### 5.2) Prüfen der laufenden Container

- Stelle sicher, dass die Container laufen:
  ```sh
  docker ps
  ```
- Überprüfe die Logs, falls ein Container fehlschlägt:
  ```sh
  docker logs <container_name>
  ```

### 5.3) Anmeldung des RaceControllers am Kafka Messaging System

- Das `ConsumerConfig`-Objekt definiert die Konfiguration für den Kafka Consumer:
  ```csharp
  using Confluent.Kafka;

  var config = new ConsumerConfig
  {
      BootstrapServers = "localhost:9092",
      GroupId = "race_controller_group",
      AutoOffsetReset = AutoOffsetReset.Earliest
  };
  ```
  - `BootstrapServers`: Adresse des Kafka Brokers
  - `GroupId`: Definiert die Konsumentengruppe
  - `AutoOffsetReset`: Startpunkt für Nachrichten (Earliest = alle vorhandenen Nachrichten abarbeiten)

- Die Methode `Subscribe()` meldet den Consumer für ein bestimmtes Kafka-Topic an:
  ```csharp
  using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
  consumer.Subscribe("race_commands");
  ```

### 5.4) Struktur eines CLI-Befehls

- Der CLI-Befehl wird über Kafka gesendet und sieht beispielsweise so aus:
  ```json
  {
    "Action": "start_race",
    "Tracks": 1,
    "Segments": 5,
    "Players": 4,
    "CircuitLaps": 3
  }
  ```
- Die JSON-Nachricht wird in ein C#-Objekt `StartRaceCommand` deserialisiert:
  ```csharp
  public class StartRaceCommand
  {
      public string Action { get; set; }
      public int Tracks { get; set; }
      public int Segments { get; set; }
      public int Players { get; set; }
      public int CircuitLaps { get; set; }
  }
  ```

### 5.5) Empfangen des CLI-Befehls beim RaceController

- Der RaceController liest Nachrichten von Kafka innerhalb der `Main`-Methode:
  ```csharp
  while (true)
  {
      var consumeResult = consumer.Consume(cancellationToken);
      var command = JsonSerializer.Deserialize<StartRaceCommand>(consumeResult.Value);
      Console.WriteLine($"Empfangen: {command.Action}");
  }
  ```
  - Die Nachricht wird von Kafka konsumiert
  - Das JSON wird in das `StartRaceCommand`-Objekt umgewandelt
  - Der RaceController verarbeitet den Befehl

### 5.6) Generierung der Rennstrecke

- Nach dem Empfangen des Befehls erzeugt der RaceController die Rennstrecke:
  ```json
  {
    "trackId": "1",
    "segments": [
      {
        "segmentId": "start-and-goal-1",
        "type": "start-goal",
        "nextSegments": ["segment-1-1"]
      },
      {
        "segmentId": "segment-1-1",
        "type": "normal",
        "nextSegments": ["segment-1-2"]
      }
    ]
  }
  ```

### 5.7) Starten der SegmentService-Container

- Der RaceController startet für jedes Segment einen neuen Container:
  ```csharp
  Process.Start("docker", "run -d --name SegmentService-1 segment_service_image");
  ```

### 5.8) Übertragen der Segment-Daten an den jeweiligen SegmentService

- Das Segment-JSON wird an Kafka gesendet:
  ```csharp
  var message = JsonSerializer.Serialize(segmentData);
  producer.Produce("segment_topic", new Message<Null, string> { Value = message });
  ```

### 5.9) Empfang und Verarbeitung eines Segment-Tokens

- Der SegmentService empfängt Nachrichten von Kafka:
  ```csharp
  var consumeResult = consumer.Consume(cancellationToken);
  var token = JsonSerializer.Deserialize<PlayerToken>(consumeResult.Value);
  Console.WriteLine($"Spieler {token.PlayerId} erreicht Segment {token.SegmentId}");
  ```

### 5.10) Weiterleitung des Tokens zum nächsten Segment

- Nach der Verarbeitung sendet der SegmentService das Token weiter:
  ```csharp
  token.CurrentTime = DateTime.UtcNow;
  producer.Produce("next_segment_topic", new Message<Null, string> { Value = JsonSerializer.Serialize(token) });
  ```

### 5.11) Abschluss des Rennens

- Wenn alle Runden abgeschlossen sind, wird das Ergebnis an den RaceController gesendet:
  ```json
  [
    { "player_id": 1, "total_time": "00:02:34.567" },
    { "player_id": 2, "total_time": "00:02:34.890" }
  ]
  ```

### 5.12) Ausgabe des Rennergebnisses

- Der RaceController gibt die Endzeiten aller Spieler aus:
  ```csharp
  Console.WriteLine(JsonSerializer.Serialize(raceResults));
  ```

---

Diese Dokumentation beschreibt die vollständige Architektur und Implementierung des AveCaesar-Projekts mit Kafka und dynamischen Docker-Containern. Falls Änderungen oder Ergänzungen nötig sind, gib einfach Bescheid! 🚀

