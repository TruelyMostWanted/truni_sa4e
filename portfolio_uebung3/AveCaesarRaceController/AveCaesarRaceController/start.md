### (0) Starte die Docker-Engine
Zum Ausführen der Befehle wird eine laufende Docker-Engine benötigt.

### (1) Alle Container zu stoppen und entfernen
```shell 
docker-compose down -v
```

### (2) Alle Container zu starten
```shell
docker-compose -f compose.yaml up -d --build
```

### (3) Warte bis die Ausgabe komplett ist
```
[INFO] Starting AvaCaesar Race Controller
[INFO] Preparing...
[INFO] 5...
[INFO] 4...
[INFO] 3...
[INFO] 2...
[INFO] 1...
```
Diese Verzögerung ist notwendig, um sicherzustellen, dass alle Services vollständig gestartet sind.
Erst danach verbinden sich die Services mit dem Kafka-Cluster.

### (4) Sende einen CLI-Befehl an den RaceController
```shell
echo "admin:start_race laps=2 segments=3 players=1" | docker exec -i kafka1 kafka-console-producer.sh --broker-list kafka1:9092 --topic race_api --property "parse.key=true" --property "key.separator=:"
```
- `admin:start_race`: Befehl um ein Rennen zu starten
- `laps=2`: Anzahl der Runden (2)
- `segments=3`: Anzahl der Segmente (3)
- `players=1`: Anzahl der Spieler (1)
- `kafka1:9092`: Kafka-Server und Port
- `race_api`: Topic für die Rennsteuerung
- `parse.key=true`: Aktiviert die Verwendung von Schlüsseln
- `key.separator=:`: Trennzeichen für Schlüssel und Wert
- `docker exec -i`: Führt den Befehl im Kafka-Container aus
- `kafka-console-producer.sh`: Sendet Nachrichten an Kafka
- `--broker-list`: Liste der Kafka-Server
- `--topic`: Topic für die Nachrichten
- `--property`: Eigenschaften für die Nachrichten
- `echo`: Sendet die Nachricht an den Producer
