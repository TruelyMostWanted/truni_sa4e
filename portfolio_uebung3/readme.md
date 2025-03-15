# Project Structure

## 1) Welche Docker-Images werden benutzt?

- **Kafka**: Messaging-System zur Kommunikation zwischen den Services
- **Zookeeper**: Notwendig für den Betrieb von Kafka
- **.NET SDK/Runtime**: Wird für die C#-Projekte benötigt
- **RaceController**: Hauptlogik des Rennens
- **SegmentService**: Dynamisch erstellte Services für jedes Streckensegment

## 2) Welche C#-Projekte werden erstellt?

- **RaceController**: Hauptsteuerung des Rennens, empfängt CLI-Befehle, erstellt Strecken und koordiniert das Rennen.
- **SegmentService**: Einzelne Segmente der Rennstrecke, die über Kafka kommunizieren und Spieler weiterleiten.

## 3) Aufgaben der Docker-Container?

- **Statische Container:**
  - **Kafka**: Message Broker für die Kommunikation zwischen RaceController und SegmentServices.
  - **Zookeeper**: Unterstützt Kafka.
  - **DotNet**: Basiscontainer für sämtliche .NET-Anwendungen.
  - **RaceController**: Nimmt CLI-Befehle an, startet Rennen, verwaltet Segment Services, führt das Rennen aus.
- **Dynamische Container:**
  - **SegmentService-**: Einzelne Streckenabschnitte, die Spieler-Token weiterleiten.

## 4) Ablauf des Projekts

1. **Starten der Docker Compose.yaml**

   - Mit `docker-compose up -d` werden alle statischen Container gestartet: Kafka, Zookeeper, RaceController.

2. **Prüfen der laufenden Container / Healthchecks**

   - Mit `docker ps` wird überprüft, ob alle Container laufen.
   - Healthchecks für Kafka und Zookeeper müssen erfolgreich sein.

3. **Struktur und Aufbau eines CLI-Befehls**

   - Ein Rennen wird mit einem CLI-Befehl gestartet, z. B.:
     ```sh
     start_race segments=5 laps=3 players=4
     ```
   - Parameter:
     - `segments=5`: Anzahl der Streckenabschnitte
     - `laps=3`: Anzahl der Runden
     - `players=4`: Anzahl der Spieler

4. **Senden eines CLI-Befehls an den RaceController**

   - Der RaceController empfängt den CLI-Befehl und beginnt mit der Rennvorbereitung.

5. **Verwenden der Parameter zur Generierung der Rennstrecke**

   - Der TrackGenerator (als Teil des RaceControllers) erzeugt eine Rennstrecke im JSON-Format.
   - Beispielhafte JSON-Ausgabe:
    ```json
	{
      "trackId": "1",
      "segments": [
        {
          "segmentId": "start-and-goal-1",
          "type": "start-goal",
          "nextSegments": [
            "segment-1-1"
          ]
        },
        {
          "segmentId": "segment-1-1",
          "type": "normal",
          "nextSegments": [
            "segment-1-2"
          ]
        },
        {
          "segmentId": "segment-1-2",
          "type": "normal",
          "nextSegments": [
            "segment-1-3"
          ]
        },
        {
          "segmentId": "segment-1-3",
          "type": "normal",
          "nextSegments": [
            "segment-1-4"
          ]
        },
        {
          "segmentId": "segment-1-4",
          "type": "normal",
          "nextSegments": [
            "start-and-goal-1"
          ]
        }
      ]
    }
    ```

6. **Starten der Segment Services**

   - Der RaceController erstellt für jedes Segment dynamisch einen **SegmentService-Container**.
   - Insgesamt werden `segments` viele SegmentServices als Docker-Container erzeugt.

7. **Starten des Rennens / Tokengenerierung**

   - Der RaceController generiert pro Spieler ein Token:
     ```json
     {
       "player_id": 1,
       "start_time": "TIMESTAMP",
       "current_time": "TIMESTAMP",
       "current_lap": 1,
       "final_lap": 3
     }
     ```
   - Diese Tokens werden an das erste Segment (`start-goal`) gesendet.

8. **Senden und Weiterleiten des Tokens**

   - Jedes Segment empfängt ein Token, aktualisiert `current_time` und leitet es an das nächste Segment weiter.
   - Nach einer Runde wird `current_lap` erhöht.
   - Sobald `current_lap == final_lap`, wird das Token an `start-goal` zurückgesendet.

9. **Abschluss des Rennens**

   - Wenn alle Spieler das Ziel erreicht haben, beendet der RaceController das Rennen.
   - Die dynamisch erstellten SegmentServices werden gestoppt und entfernt.

10. **Ergebnis**

- Die endgültige Rennzeit pro Spieler wird geloggt und zurückgegeben.
- Das Ergebnis könnte folgendermaßen aussehen:
  ```json
  {
    "player_id": 1,
    "total_time": "00:02:34.567"
  }
  ```

