# Docker CLI Befehle

## Allgemeine Befehle

### 1. Compose-Umgebungen abschalten

`docker-compose down`

Dieser Befehl stoppt alle Container, die im aktuellen `docker-compose.yml` definiert sind, und entfernt sie.

**Parameter**:
- Keine spezifischen Parameter erforderlich.

**Beispiel**:
```bash
docker-compose down
```

---

### 2. Docker Compose neu starten

`docker-compose up -d`

Startet die Docker-Container basierend auf der aktuellen `docker-compose.yml` im Hintergrund und erzeugt sie, falls sie noch nicht existieren.

**Parameter**:
- `-d`: Startet die Container im Hintergrund (Detached Mode).

**Beispiel**:
```bash
docker-compose up -d
```

---

### 3. Laufende Container prüfen

`docker ps`

Listet alle derzeit laufenden Container auf und zeigt Details wie die Container-ID, den Namen, den Status und verwendete Ports.

**Parameter**:
- Keine spezifischen Parameter erforderlich.

**Beispiel**:
```bash
docker ps
```

---

### 4. Docker Images installieren

`docker pull <image_name>`

Lädt ein Docker-Image aus dem Docker Hub (oder einem anderen Registry) herunter.

**Parameter**:
- `<image_name>`: Der Name des Images, das heruntergeladen werden soll (z. B. `nginx` oder `mysql`).

**Beispiel**:
```bash
docker pull nginx
```

---

### 5. Ein Image als Container starten

`docker run -d --name <container_name> -p <host_port>:<container_port> <image_name>`

Dieser Befehl startet einen Container aus einem angegebenen Image im Hintergrund, gibt ihm einen Namen und ordnet Ports zu.

**Parameter**:
- `-d`: Führt den Container im Hintergrund aus (Detached Mode).
- `--name <container_name>`: Der benutzerdefinierte Name des Containers.
- `-p <host_port>:<container_port>`: Mapped einen Port auf dem Host-System zu einem Port im Container.
- `<image_name>`: Das Docker-Image, das verwendet werden soll.

**Beispiel**:
```bash
docker run -d --name my_nginx -p 8080:80 nginx
```

---

### 6. Einen laufenden Container stoppen

`docker stop <container_name>`

Stoppt einen laufenden Container anhand seines Namens oder seiner ID.

**Parameter**:
- `<container_name>`: Der Name oder die ID des Containers, der gestoppt werden soll.

**Beispiel**:
```bash
docker stop my_nginx
```

---

### 7. Liste aller Docker-Netzwerke anzeigen

`docker network ls`

Listet alle Docker-Netzwerke auf, die auf deinem Host-System vorhanden sind.

**Parameter**:
- Keine spezifischen Parameter erforderlich.

**Beispiel**:
```bash
docker network ls
```

---

## Kafka Befehle

### 1. Kafka Topics prüfen

`docker exec -it kafka kafka-topics.sh --bootstrap-server localhost:9092 --list`

Listet alle existierenden Kafka-Topics, die auf eurem Kafka-Broker verfügbar sind.

**Parameter**:
- `docker exec`: Führt einen Befehl in einem laufenden Container aus.
- `-it`: Aktiviert interaktiven Modus und ein pseudoterminal.
- `kafka`: Der Name des Kafka-Containers.
- `kafka-topics.sh`: Skript zur Verwaltung von Kafka-Topics.
- `--bootstrap-server localhost:9092`: Die Bootstrap-Adresse des Kafka-Clusters.
- `--list`: Zeigt eine Liste aller Topics an.

**Beispiel**:
```bash
docker exec -it kafka kafka-topics.sh --bootstrap-server localhost:9092 --list
```

---

### 2. Kafka Topic erstellen

`docker exec -it kafka kafka-topics.sh --bootstrap-server localhost:9092 --create --topic <topic_name> --partitions <number> --replication-factor <number>`

Erstellt ein neues Topic mit der angegebenen Anzahl von Partitionen und Replikationen in einem Kafka-Cluster.

**Parameter**:
- `--create`: Gibt an, dass ein Topic erstellt werden soll.
- `--topic <topic_name>`: Der Name des Topics, das erstellt werden soll.
- `--partitions <number>`: Anzahl der Partitionen, die für das Topic erstellt werden sollen.
- `--replication-factor <number>`: Anzahl der Replikationen (Kopien) des Topics.

**Beispiel**:
```bash
docker exec -it kafka kafka-topics.sh --bootstrap-server localhost:9092 --create --topic example_topic --partitions 1 --replication-factor 1
```

---

### 3. Nachricht an Kafka senden

`echo "Deine Nachricht hier" | docker exec -i kafka kafka-console-producer.sh --broker-list localhost:9092 --topic <topic_name>`

Sendet eine Nachricht an ein vorhandenes Kafka-Topic.

**Parameter**:
- `echo "Deine Nachricht hier"`: Erstellt die Nachricht, die gesendet werden soll.
- `|`: Leitet die Nachricht an den Kafka-Befehl weiter.
- `docker exec -i`: Führt den Kafka-Befehl im Container aus und erlaubt dabei Eingaben durch den Pipe.
- `kafka-console-producer.sh`: Kafka-Skript zum Senden von Nachrichten.
- `--broker-list localhost:9092`: Gibt die Liste der Kafka-Bootstrap-Server an.
- `--topic <topic_name>`: Der Name des Topics, an das die Nachricht gesendet wird.

**Beispiel**:
```bash
echo "Hallo Kafka" | docker exec -i kafka kafka-console-producer.sh --broker-list localhost:9092 --topic example_topic
```

---

### 4. Nachricht an Kafka mit Key senden

`echo "key1:Deine Nachricht hier" | docker exec -i kafka kafka-console-producer.sh --broker-list localhost:9092 --topic <topic_name> --property "parse.key=true" --property "key.separator=:"`

Sendet eine Nachricht mit einem Key an ein Kafka-Topic, wobei der Key verwendet wird, um die Partition zu bestimmen.

**Parameter**:
- `echo "key1:Deine Nachricht hier"`: Gibt den Key (`key1`) und die Nachricht an, getrennt durch einen Separator (in diesem Fall `:`).
- `|`: Leitet die Nachricht und den Key an das Kafka-Tool weiter.
- `docker exec -i`: Führt den Kafka-Befehl im Container aus.
- `kafka-console-producer.sh`: Kafka-Skript zum Produzieren der Nachrichten.
- `--broker-list localhost:9092`: Gibt die Liste der Kafka-Bootstrap-Server an.
- `--topic <topic_name>`: Der Name des Topics, an das die Nachricht gesendet wird.
- `--property "parse.key=true"`: Aktiviert das Parsen von Keys.
- `--property "key.separator=:"`: Definiert den Separator, der den Key von der Nachricht trennt (z. B. `:` oder `|`).

**Beispiel**:
```bash
echo "key1:Das ist meine Nachricht" | docker exec -i kafka kafka-console-producer.sh --broker-list localhost:9092 --topic example_topic --property "parse.key=true" --property "key.separator=:"
```