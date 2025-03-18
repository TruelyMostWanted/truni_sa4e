# AveCaesar RaceController - Kafka Cluster Setup

## Ziel

In "Aufgabe2" wird der zentrale Streaming-Server durch ein **Kafka-Cluster** mit mindestens 3 Serverinstanzen ersetzt, um die Skalierbarkeit und Zuverlässigkeit der Nachrichtenplattform zu verbessern. Diese Anleitung beschreibt die notwendigen Schritte, um ein Kafka-Cluster bereitzustellen und den RaceController korrekt zu konfigurieren.

---

## 1) Kafka Cluster mit Docker Compose einrichten

Hier wird die vorhandene `docker-compose.yaml` angepasst, um ein Kafka-Cluster mit **3 Broker-Instanzen** zu erstellen. Zookeeper wird weiterhin als Koordinationsservice eingesetzt.

### `docker-compose.yaml`

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
    ports:
      - "9092:9092" # Expose Broker 1
    environment:
      KAFKA_BROKER_ID: 1                      # ID des ersten Brokers
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka1:9092
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 2
      KAFKA_OFFSETS_TOPIC_NUM_PARTITIONS: 3
    depends_on:
      - zookeeper
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.11

  # Kafka Broker 2
  kafka2:
    image: wurstmeister/kafka
    container_name: kafka2
    ports:
      - "9093:9093" # Expose Broker 2
    environment:
      KAFKA_BROKER_ID: 2                      # ID des zweiten Brokers
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka2:9093
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9093
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 2
      KAFKA_OFFSETS_TOPIC_NUM_PARTITIONS: 3
    depends_on:
      - zookeeper
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.12

  # Kafka Broker 3
  kafka3:
    image: wurstmeister/kafka
    container_name: kafka3
    ports:
      - "9094:9094" # Expose Broker 3
    environment:
      KAFKA_BROKER_ID: 3                      # ID des dritten Brokers
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka3:9094
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9094
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 2
      KAFKA_OFFSETS_TOPIC_NUM_PARTITIONS: 3
    depends_on:
      - zookeeper
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.13

  # RaceController Service
  racecontroller:
    build:
      context: ./AveCaesarRaceController  # Setze den Kontext auf das richtige Verzeichnis
      dockerfile: ./AveCaesarRaceController/Dockerfile
    container_name: racecontroller
    depends_on:
      - kafka1
      - kafka2
      - kafka3
    environment:
      # Die RaceController-Instanz verbindet sich jetzt mit dem gesamten Kafka-Cluster
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

## 2) Kafka-Konfigurationsübersicht

### Wichtige Anpassungen für das Cluster
1. **Mehrere Kafka-Broker:**
   - Drei Kafka-Instanzen `kafka1`, `kafka2` und `kafka3` werden in das System eingebunden.
   - Jeder Broker erhält eine eindeutige **Broker ID**: `KAFKA_BROKER_ID`.

2. **Zookeeper-Verbindung:**
   - Alle Broker verbinden sich mit der zentralen Zookeeper-Instanz unter `zookeeper:2181`.

3. **Replication Factor:**
   - Der `KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR` ist auf **2** gesetzt, um Daten zwischen den Brokern zu replizieren.

4. **Partitionierung:**
   - Das Offset-Management-Topic erhält **3 Partitionen**, damit die Last auf mehrere Broker verteilt werden kann.

---

## 3) RaceController Konfiguration

Der RaceController muss nicht wesentlich geändert werden, da er sich weiterhin nur mit einem Kafka-Service verbindet. Die Verbindung erfolgt jetzt jedoch mit mehreren **Bootstrap-Servern**, die alle Kafka-Broker im Cluster enthalten:

### Verbindung zu Kafka Broker-Cluster
In der Konfiguration für den Kafka-Client (z. B. in C#) wird der Parameter `BootstrapServers` so definiert, dass er alle Broker umfasst:

```csharp
var config = new ProducerConfig
{
    BootstrapServers = "kafka1:9092,kafka2:9093,kafka3:9094",
    // Weitere Konfigurationen, falls nötig
};
```

---

## 4) Vorteile und Verbesserungen

### Vorteile des Kafka-Clusters:
1. **Skalierbarkeit:**
   - Das Cluster kann zusätzliche Kafka-Broker aufnehmen, um die Verarbeitungskapazität zu erweitern.

2. **Zuverlässigkeit:**
   - Bei einem Ausfall eines Brokers übernimmt ein anderer die Rolle der Partitionen.
   - Durch die Replikation werden Nachrichten nicht verloren gehen.

3. **Effizienz:**
   - Die Partitionierung ermöglicht eine effiziente Lastverteilung auf die Broker.

---

## 5) Start der Umgebung

Folgende Schritte sind erforderlich, um das System vollständig einzurichten und zu starten:

1. **Erstellen und Starten der Container:**

   ```bash
   docker-compose up --build
   ```

2. **Überprüfung des Clusters:**
   - Vergewissere dich, dass die drei Kafka-Broker unter den Ports `9092`, `9093` und `9094` laufen.
   - Der Zookeeper-Service sollte ebenfalls auf `localhost:2181` erreichbar sein.

3. **RaceController-Integration:**
   - Der RaceController sollte nun automatisch Nachrichten an das Kafka-Cluster senden und von dort empfangen können.

---

## 6) Fazit

Mit diesen Änderungen nutzt der AveCaesar RaceController ein hochverfügbares und skalierbares Kafka-Cluster. Dies verbessert die Zuverlässigkeit und stellt sicher, dass das System auch bei steigenden Anforderungen stabil bleibt.