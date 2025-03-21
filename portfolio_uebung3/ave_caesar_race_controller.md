# **Projektdokumentation AveCaesar RaceController**

---

## **Übersicht**

Das Projekt **AveCaesar RaceController** ist eine leistungsfähige Simulation eines antiken Wagenrennens. Es umfasst
mehrere virtuelle Streitwagen, die auf einer simulierten Rennstrecke gegeneinander antreten. Mithilfe einer modernen,
verteilten Systemarchitektur werden innovative Ansätze für die Steuerung und Verteilung der einzelnen Rennsegmente
entwickelt. Das Projekt basiert auf **Docker-Containern** und **Apache Kafka** für Skalierung und Kommunikation.

---

## **Aufgabenstellung und Ziele**

### **Aufgabe 1** – „Nur der Schnellste gewinnt“

- Verwendung von Apacha Kafka als Messaging-System
- Einbindung eines Strecken-Generators
- Implementierung von Segmenten als Clients die über Kafka kommunizieren
- **Streitwagen (Token)** werden zwischen den Segmenten weitergereicht.
- Simulation eines Rundkurs-Rennens ausgehend von **Start-Goal**
- Einfache **CLI-Steuerung** zum Starten des Rennens.
- Erstellen und Starten von Docker-Containern

### **Aufgabe 2** – **Cluster**

- Ersatz des zentralen Streaming-Servers durch ein **Kafka-Cluster** mit mindestens drei Instanzen.
- Verbesserung von **Skalierbarkeit** und **Zuverlässigkeit**.

### **Aufgabe 3** – „Ave Caesar“

- Erweiterung der Rennsimulation um **neue Segmenttypen**:
    - **Caesar-Segment** für Interaktion mit „Caesar“-Logik.
    - **Bottleneck**-Segmente für Engpässe.
- Einführung von **Spielerrestriktionen** (begrenzte Spieleranzahl pro Segment) und **zufälligen Verzögerungen** in
  Engpässen.
- Implementierung von **Aufspaltungen in Streckenabschnitte** mit mehreren möglichen Nachfolgersegmenten.

---

## **Spezifikationen und Anforderungen**

### **Technologie-Stack**

1. **Programmiersprache**: C# (Version 13.0, .NET 9.0)
2. **Containerisierung**: Docker und Docker Compose
3. **Streaming-Plattform**: Apache Kafka (mit Zookeeper)
4. **Topologie**:
    - Rennstrecken-Segmente werden individuell als Docker-Container betrieben.
    - Kommunikation erfolgt über das Publish/Subscribe-Muster.

### **Rennsimulation**

- **Rennstrecke**: besteht aus Segmenten, die Spielerbewegungen anhand von Token verarbeiten.
- **CLI-Initiierung**: Rennen starten und Befehle an die Segmente weiterleiten.
- **Endbedingung**: Sobald eine bestimmte Rundenzahl erreicht ist, wird das Rennen beendet und die Laufzeiten aller
  Streitwagen ausgegeben.

---

## **Ansätze zur Lösung**

1. **Streaming-Architektur**:
    - Zentrale Kommunikationsebene mit **Apache Kafka** für verlässlichen Nachrichtenaustausch.
    - **Themen (Topics)** verwalten Nachrichten zwischen RaceController und Segmenten.

2. **Dockerisierung**:
    - Alle Dienste (RaceController, Kafka, Zookeeper) in isolierten **Docker-Containern** betrieben.

3. **Segment-Management**:
    - Jedes Segment wird als eigenständiger Kafka-Client im RaceController betrieben.
    - **Kafka-Listener** in jedem Segment verarbeitet Token und leitet sie an das nächste Segment weiter.

4. **Weiterleitungslogik**:
    - Tokens werden durch Segmente weitergereicht.
    - Zusätzliche Segmenttypen erweitern die Simulation durch Restriktionen und Interaktionen.

5. **Cluster-Konfiguration**:
    - Einrichten eines Kafka-Cluster für höhere Verfügbarkeit und Redundanz.

---

## **Projektstruktur**

```plaintext
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

---

## **Datenstrukturen**

### **Spieler (Token)**

Dient als zentraler Punkt zur Verfolgung der Spielerbewegungen.

```csharp
public class PlayerToken
{
    //(1) Spieler-ID
    public int PlayerID { get; set; }
    
    //(2) Daten zum Senden und Empfangen    
    public string SenderID { get; set; }
    public string ReceiverID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime SentAt { get; set; }

    //(3) Renninformationen
    public int CurrentLap { get; set; }
    public int MaxLaps { get; set; }
    public ulong TotalTimeMs { get; set; }
    
    //(4) Zusatz-Elemente
    //TASK 3: Number of Caesar Greets
    public int CaesarGreets { get; set; }
}
```

### **Segments**

Umfasst Eigenschaften wie **Segment-Typ**, Spielerrestriktionen und Nachfolgersegmente.

```csharp
public class TrackSegment
{
    public string SegmentId { get; set; }
    public string Type { get; set; } // normal, start-goal, caesar, bottleneck
    public int MaxPlayers { get; set; }
    public List<PlayerToken> CurrentPlayers { get; set; }
    public List<string> NextSegments { get; set; }
}
```

---

## **Umsetzung und Implementierung**

1. **Segmentrestriktionen**:
    - Jedes Segment erhält eine **MaxPlayers**-Eigenschaft.
    - Kontrolliert, wie viele Spieler gleichzeitig im Segment erlaubt sind.

2. **Spielerbewegung**:
    - Spieler können nur in Segmente wechseln, wenn Platz verfügbar ist.
    - Spieler werden aus dem aktuellen Segment entfernt und hinzugefügt:
      ```csharp
      public void RemovePlayer(PlayerToken player) 
      { 
          CurrentPlayers.Remove(player);
      }            
      public bool AddPlayer(PlayerToken player)
      {
          if (CanAddPlayer())
          {
              CurrentPlayers.Add(player);
              return true;
          }
          return false;
      }
      ```

3. **Kafka-Integration**:
    - Konsumenten (Consumers) registrieren sich für bestimmte Themen:
      ```csharp
      var config = new ConsumerConfig
      {
          BootstrapServers = "localhost:9092",
          GroupId = "race_controller_group",
          AutoOffsetReset = AutoOffsetReset.Earliest
      };
      ```
    - Token werden verarbeitet und zum nächsten Segment weitergeleitet.

4. **Cluster-Setup**:
    - Nutzung eines Docker-Compose-Files für Kafka mit drei Brokern:
      ```yaml
   version: '3.8'

   services:
   # Zookeeper Service
   zookeeper:
   image: wurstmeister/zookeeper
   container_name: zookeeper
   ports:
    - "2181:2181"
      networks:
      avecaesar-net:
      ipv4_address: 172.20.0.10

   # Kafka Broker 1
   kafka1:
   image: wurstmeister/kafka
   container_name: kafka1
   depends_on:
    - zookeeper
      ports:
        - "9092:9092"
          environment:
          KAFKA_BROKER_ID: 1
          KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
          KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka1:9092
          KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT
          KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
          KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9092
          KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 3
          KAFKA_AUTO_CREATE_TOPICS_ENABLE: "true"
          KAFKA_NUM_PARTITIONS: 3
          KAFKA_DEFAULT_REPLICATION_FACTOR: 3
          KAFKA_HEAP_OPTS: "-Xmx1024M -Xms1024M"
          healthcheck:
          test: ["CMD-SHELL", "kafka-topics --bootstrap-server localhost:9092 --list"]
          interval: 10s
          timeout: 5s
          retries: 5
          networks:
          avecaesar-net:
          ipv4_address: 172.20.0.11

   # Kafka Broker 2
   kafka2:
   image: wurstmeister/kafka
   container_name: kafka2
   depends_on:
    - zookeeper
      ports:
        - "9093:9093"
          environment:
          KAFKA_BROKER_ID: 2
          KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
          KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka2:9093
          KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT
          KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
          KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9093
          KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 3
          KAFKA_AUTO_CREATE_TOPICS_ENABLE: "true"
          KAFKA_NUM_PARTITIONS: 3
          KAFKA_DEFAULT_REPLICATION_FACTOR: 3
          KAFKA_HEAP_OPTS: "-Xmx1024M -Xms1024M"
          healthcheck:
          test: ["CMD-SHELL", "kafka-topics --bootstrap-server localhost:9093 --list"]
          interval: 10s
          timeout: 5s
          networks:
          avecaesar-net:
          ipv4_address: 172.20.0.12

   # Kafka Broker 3
   kafka3:
   image: wurstmeister/kafka
   container_name: kafka3
   depends_on:
    - zookeeper
      ports:
        - "9094:9094"
          environment:
          KAFKA_BROKER_ID: 3
          KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
          KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka3:9094
          KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT
          KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
          KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9094
          KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 3
          KAFKA_AUTO_CREATE_TOPICS_ENABLE: "true"
          KAFKA_NUM_PARTITIONS: 3
          KAFKA_DEFAULT_REPLICATION_FACTOR: 3
          KAFKA_HEAP_OPTS: "-Xmx1024M -Xms1024M"
          healthcheck:
          test: ["CMD-SHELL", "kafka-topics --bootstrap-server localhost:9094 --list"]
          interval: 10s
          timeout: 5s
          retries: 5
          networks:
          avecaesar-net:
          ipv4_address: 172.20.0.13

   # RaceController Service
   racecontroller:
   build:
   context: ./AveCaesarRaceController # Setze den Kontext auf das richtige Verzeichnis
   dockerfile: ./AveCaesarRaceController/Dockerfile
   container_name: racecontroller
   depends_on:
    - kafka1
        - kafka2
        - kafka3
          environment:
   # BootstrapServer für Kafka Client mit allen Kafka-Brokern
   KAFKA_BROKER: kafka1:9092,kafka2:9093,kafka3:9094
   networks:
   avecaesar-net:
   ipv4_address: 172.20.0.14
   restart: always

   networks:
   avecaesar-net:
   driver: bridge
   ipam:
   config:
    - subnet: 172.20.0.0/16
     ```

---

## **Ausführung**

1. **Starten der Docker-Umgebung**:
    - Befehle zum Starten, Stoppen und Neustarten der Umgebung:
      ```shell
      docker-compose down -v
      docker-compose up -d --build
      ```

2. **Starten eines Rennens**:
    - CLI-Befehl zum Initialisieren des Rennens:
      ```shell
      echo "admin:start_race laps=2 segments=3 players=4" | 
      docker exec -i kafka1 kafka-console-producer.sh --broker-list kafka1:9092 --topic race_api
      ```
    - `admin:` - ID für den Absender.
    - `start_race` - Befehl zum Starten des Rennens.
    - `laps=2` - Anzahl der Runden (hier 2)
    - `segments=3` - Anzahl der Segmente (hier 3)
    - `players=4` - Anzahl der Spieler (hier 4)
    - `race_api` - Kafka-Topic für die Rennsteuerung.
    - `docker exec -i` - Führt den Befehl im Kafka-Container aus.
    - `kafka-console-producer.sh` - Sendet Nachrichten an Kafka.
    - `--broker-list kafka1:9092` - Broker-Adresse und Port.

3. **Fehlerbehebung**:
    - Prüfen der laufenden Container:
      ```shell
      docker ps
      ```
    - Logs auslesen:
      ```shell
      docker logs <container_name>
      ```

---

## **Ergebnisse**

- **Simulationsergebnis**: Nach Ende des Rennens werden die Gesamtlaufzeiten aller Spieler ausgegeben.
- **Cluster-Effizienz**: Die Aktualisierung auf ein Kafka-Cluster ermöglicht höhere Skalierbarkeit und Zuverlässigkeit.
- **Neue Segmenttypen**: Einführung von Caesar- und Bottleneck-Segmenten erhöht die Komplexität und Realitätsnähe.

---

## **Starten des Projekts**

### **Schrittweise Anleitung**

1. **Starte Docker-Engine**:
   ```shell
   docker compose up -d --build
   ```

2. **Sende CLI-Kommandos**:
   ```shell
   echo "admin:start_race laps=3 segments=5 players=2" | docker exec -i kafka1 kafka-console-producer.sh --broker-list kafka1:9092 --topic race_api
   ```

3. **Überprüfe Logs und Ergebnisse**:
   ```shell
   docker logs racecontroller
   ```

---