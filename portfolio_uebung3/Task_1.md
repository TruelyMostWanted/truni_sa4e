# AveCaesar RaceController - README

## Übersicht

Das Projekt AveCaesar RaceController ist eine Simulation, in der mehrere Spieler (Streitwagen) auf einer Rennstrecke in einer virtuellen Umgebung gegeneinander antreten. Die Architektur besteht aus drei Docker-Containern:

1. **Kafka**: Kommunikationsplattform für asynchronen Nachrichtenaustausch.
2. **Zookeeper**: Verwaltung der Kafka-Umgebung.
3. **RaceController**: Stellt die Rennlogik bereit.

Im RaceController interagieren mehrere Klassen, um einen Rennablauf zu simulieren. Dieses Dokument beschreibt, wie die Klassen im Kontext des Projekts verwendet werden und wie die Kommunikation zwischen ihnen stattfindet.

---

# AveCaesar RaceController - README

## 0) Setup mit Docker

Um das Projekt auszuführen, benötigen Sie **Docker Compose** sowie die bereitgestellte `docker-compose.yaml`-Datei. Mit diesen Dateien werden die drei erforderlichen Container (Zookeeper, Kafka, RaceController) eingerichtet und gestartet.

### Schritte zur Einrichtung:

1. Stellen Sie sicher, dass **Docker** und **Docker Compose** auf Ihrem System installiert sind.
2. Wechseln Sie in das Projektverzeichnis, in dem sich die `docker-compose.yaml` befindet.
3. Starten Sie die Container mit dem folgenden Befehl:
   ```bash
   docker-compose up --build
   ```
4. Docker Compose wird:
  - **Zookeeper** starten.
  - **Kafka** konfigurieren und starten.
  - Den **RaceController** bauen und starten, basierend auf der `Dockerfile`.

### Docker Compose Konfiguration:

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
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://172.20.0.11:9092
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
    depends_on:
      - zookeeper
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.11

  # RaceController Service (Container 3)
  racecontroller:
    build:
      context: ./AveCaesarRaceController  # Setze den Kontext auf das richtige Verzeichnis
      dockerfile: ./AveCaesarRaceController/Dockerfile  # Pfad zur Dockerfile bleibt gleich
    container_name: racecontroller
    depends_on:
      - kafka
    environment:
      KAFKA_BROKER: kafka:9092
    networks:
      avecaesar-net:
        ipv4_address: 172.20.0.12
    restart: always

networks:
  avecaesar-net:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

### Dockerfile für den RaceController:

```dockerfile
# Basis-Laufzeitumgebung
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Build-Umgebung
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Nur csproj und Restore für besseres Caching
COPY ["AveCaesarRaceController/AveCaesarRaceController.csproj", "AveCaesarRaceController/"]
RUN dotnet restore "AveCaesarRaceController/AveCaesarRaceController.csproj"

# Restlichen Code kopieren und bauen
COPY . .
WORKDIR "/src/AveCaesarRaceController"
RUN dotnet build "AveCaesarRaceController.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Veröffentlichung des Builds
FROM build AS publish
RUN dotnet publish "AveCaesarRaceController.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Finale Laufzeitumgebung
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Port öffnen (falls erforderlich)
EXPOSE 5000

ENTRYPOINT ["dotnet", "AveCaesarRaceController.dll"]
```

5. Nach erfolgreichem Aufbau und Start sollten alle Container wie folgt laufen:
  - **Zookeeper**: Bereit auf `localhost:2181`
  - **Kafka**: Verfügbar unter `localhost:9092`
  - **RaceController**: Der Dienst ist bereit und wartet auf Eingaben.

---


## Aufbau und Ablauf des Projekts

### 1. **Starte das Rennen mit einem CLI-Befehl**

Der Ablauf startet mit einem Befehl, der den KafkaClient des RaceController aktiviert:

```bash
start_race laps=3 segments=5 players=4
```

- `laps`: Anzahl der Runden, die die Spieler absolvieren müssen.
- `segments`: Anzahl der Segmente der Rennstrecke.
- `players`: Anzahl der teilnehmenden Spieler/Streitwagen.

---

### 2. **Erzeugung der Rennstrecke und der Clients**

#### a) **Rennstrecke generieren**
- Die Klasse `TrackGenerator` erzeugt eine Rennstrecke mit der geforderten Anzahl an Segmenten.
- Eine Rennstrecke besteht aus mehreren Teilen:
  - **Start-Ziel-Segment**: Der Anfang und das Ende der Strecke.
  - **Normale Segmente**: Verbindungselemente zwischen den Abschnitten.
  - **Segmente sind zyklisch**: Die Strecke schließt sich selbst.

#### b) **Segment-KafkaClients erstellen**
- Für jedes Segment wird ein eigener `SegmentKafkaClient` erzeugt, der auf das entsprechende Kafka-Topic der Segmente hört und Nachrichten verarbeitet.

#### c) **Spieler erzeugen**
- Für jede Spieler-ID wird ein **PlayerToken** erstellt. Jedes Token enthält Informationen wie:
  - `PlayerID`: Eindeutige ID des Spielers.
  - `SenderID` und `ReceiverID`: Von welchem Segment das Token kommt bzw. wohin es weitergeleitet wird.
  - `CurrentLap`: Die aktuelle Runde des Spielers.
  - `MaxLaps`: Maximale Anzahl der zu absolvierenden Runden.
  - `TotalTimeMs`: Gesamtzeit, die der Spieler benötigt hat.

---

### 3. **Start des Rennens**

Nachdem die Rennlogik initialisiert ist:

- Der `RaceKafkaClient` sendet alle Spieler (PlayerTokens) an das **Start-Ziel-Segment**.
- Das Rennen beginnt nun.

---

### 4. **Ablauf des Rennens**

#### a) **Segment-Verarbeitung**
- Jeder `SegmentKafkaClient` erhält Nachrichten von den Spieler-Token.
- Nur wenn die eigene ID der des Message Receivers entspricht, dann wird dieser Client die Nachricht annehmen 
- Ein Segment verarbeitet die Nachricht eines Spieler-Token wie folgt:
  1. Die Ankunftszeit wird geloggt.
  2. Die Gesamtzeit des Tokens wird aktualisiert.
  3. Das Token wird umgeleitet:
     - Innerhalb des Rennens: Zum nächsten Streckensegment.
     - Zielüberquerung (Start-Ziel-Segment): Läuft der Spieler über Start-Ziel, wird die Rundenzahl hochgezählt.

#### b) **Aktualisierung der Runden**
- Jedes Token wird solange durch die Segmente geschickt, bis es die maximale Rundenzahl (`laps`) erreicht hat.

#### c) **Rennende**
- Sobald ein Spieler (`PlayerToken`) alle Runden absolviert hat, wird es an den `RaceKafkaClient` zurückgesendet.
- Dieser speichert die Ergebnisse.

---

### 5. **Ergebnisse prüfen**

- Sobald alle Spieler die Zielbedingungen erfüllen, gibt der `RaceKafkaClient` die Rennresultate aus. Sie werden in der Konsole als JSON-Array gezeigt:

```json
[
  {
    "playerID": 1,
    "senderID": "start-and-goal-1",
    "receiverID": "start-and-goal-1",
    "currentLap": 4,
    "maxLaps": 3,
    "sentAt": "2025-03-16T19:59:32.7435114+00:00",
    "totalTimeMs": 1909
  },
  {
    "playerID": 4,
    "senderID": "start-and-goal-1",
    "receiverID": "start-and-goal-1",
    "currentLap": 4,
    "maxLaps": 3,
    "sentAt": "2025-03-16T19:59:32.7435788+00:00",
    "totalTimeMs": 1910
  }
]
```

Die Ausgabe enthält eine JSON-Liste mit allen Spielern und deren Renndaten.

---

### 6. **Beendigung**

- Nach dem Abschluss des Rennens:
  - Alle `SegmentKafkaClients` werden vom entsprechenden Segment-Topic abgemeldet und deaktiviert.
  - Alle Listen und interne Datenstrukturen werden geleert, sodass das System für ein neues Rennen bereit ist.

---

## Klassenübersicht

### 1. **Main-Klasse (`Program.cs`)**
- Verantwortlich für den Start und die Steuerung des gesamten RaceController.
- Initialisiert den `RaceKafkaClient` und startet die Kafka-Topics.

### 2. **`RaceKafkaClient`**
- Der zentrale KafkaClient, der das Renngeschehen verwaltet.
- Wichtige Aufgaben:
  - Starten des Rennens (`start_race` CLI-Befehl).
  - Erzeugen der `SegmentKafkaClients` und `PlayerTokens`.
  - Sammeln der Ergebnisse und Ausgabe der JSON-Liste.

### 3. **`SegmentKafkaClient`**
- Kafka-Client für ein einzelnes Segment der Strecke.
- Verantwortlich für:
  - Empfang und Umleitung von Spieler-Tokens.
  - Berechnung von Zeiten sowie Updates der Runden- und Segmentdaten.

### 4. **`PlayerToken`**
- Datenstruktur, die die Informationen eines Spielers enthält:
  - Rundenfortschritt
  - Zeitmessung und Sender/Receiver.

### 5. **`TrackGenerator`**
- Erzeugt die Segmente und Strecken auf Basis der angegebenen Parameter:
  - Anzahl Segmente
  - Start-Ziel-Mechanik
  - Verbindungen zwischen Segmenten.

---

## Gesamtübersicht der Kommunikation

1. Der `RaceKafkaClient` startet den Prozess, indem er das Rennen aufsetzt und `PlayerTokens` an das Start-Ziel-Segment verteilt.
2. `SegmentKafkaClients` verarbeiten Spieler-Tokens und leiten diese weiter.
3. Die Tokens durchlaufen zyklisch die Strecke.
4. Sobald ein Spieler das Ziel (nach `laps`-Anzahl) erreicht, übermittelt das Start-Ziel-Segment das Token an den `RaceKafkaClient`.
5. Der `RaceKafkaClient` sammelt die Daten aller Spieler und gibt die Resultate aus.

---

## Voraussetzungen

1. Docker mit den Containern:
   - **Kafka**
   - **Zookeeper**
   - **RaceController**
2. Ein CLI-Befehl mit korrekten Parametern zur Initierung des Rennens.

---

## Fazit

Das Projekt AveCaesar RaceController bietet eine vollständige Simulation eines Wagenrennens mit Kafka als Backbone für die Kommunikation zwischen verschiedenen Elementen (Race- und Segment-Clients). Es ermöglicht eine modulare Erweiterung und ist flexibel gestaltet, um zukünftige Anpassungen an der Rennlogik zu erlauben.