# Aufgabe 2: Konkretisierung

## Einführung in Microservices und Tech-Stacks

Microservices sind eine Architekturform, bei der Anwendungen in unabhängige, kleine Dienste zerlegt werden. Jeder Dienst ist eigenständig, fokussiert auf eine spezifische Aufgabe und kommuniziert über klar definierte Schnittstellen, wie REST APIs. Diese Architektur bietet Vorteile wie Skalierbarkeit, Flexibilität und einfachere Wartung.

## Geeignete Tech-Stacks
Für die Umsetzung einer solchen Architektur gibt es eine Vielzahl an möglichen Technologien. Nachfolgend eine Übersicht:

### Backend-Frameworks und Servertechnologien
- **ASP.NET**: Ein Framework von Microsoft für den Aufbau moderner Web-APIs und Webanwendungen.
- **Blazor**: Ein Framework für Single Page Applications (SPAs) in .NET.
- **Node.js**: JavaScript-basiertes Framework für serverseitige Anwendungen.
- **Spring Boot**: Java-basiertes Framework für Microservices.
- **Flask/Django**: Python-basierte Frameworks für Webanwendungen.
- **Apache Camel**: Framework für Integrationsmuster und Microservices.
- **XAMPP**: Entwicklungsumgebung mit Apache, MariaDB, PHP und Perl.

### Datenbanken
- **MySQL**: Relationales Datenbankmanagementsystem mit breiter Unterstützung.
- **PostgreSQL**: Leistungsstarke relationale Open-Source-Datenbank.
- **SQLite**: Leichtgewichtige Datenbank für kleinere Anwendungen.
- **MongoDB**: NoSQL-Datenbank für dokumentenbasierte Daten.
- **Redis**: In-Memory-Datenbank für schnelle Zugriffe.

### Containerisierung und Orchestrierung
- **Docker**: Container-Plattform für die Bereitstellung und Skalierung von Anwendungen.
- **Kubernetes**: Orchestrierungsplattform für containerisierte Anwendungen.

### Frontend-Frameworks (für Clients)
- **React**: JavaScript-Bibliothek für Benutzeroberflächen.
- **Angular**: Framework für komplexe Webanwendungen.
- **Vue.js**: Leichtgewichtiges Framework für UIs.

## Entscheidung für die Architektur
### Gewählte Technologien
Für diese Architektur haben wir folgende Technologien ausgewählt:

- **API Gateway**: ASP.NET Core, um eine zentralisierte API-Verwaltung zu ermöglichen.
- **Auth/Validator**: ASP.NET Core, um Zugriffstoken zu prüfen und Datenvalidierung sicherzustellen.
- **Wishes-Service**: ASP.NET Core, um CRUD-Operationen (Create, Read, Update, Delete) für Wünsche bereitzustellen.
- **Datenbank**: MySQL, um relationale Daten mit hoher Konsistenz zu speichern.
- **Containerisierung**: Docker, um die Dienste unabhängig voneinander zu deployen und zu skalieren.

### Funktion der Dienste
1. **API Gateway**
    - Routing der Anfragen an die richtigen Microservices.
    - Integration von Authentifizierungs- und Validierungslogik über den Auth/Validator-Service.
    - Aggregation und Weiterleitung der Antworten an den Client.
    - Lastverteilung auf mehrere Instanzen von Validator- und Wishes-Services.

2. **Auth/Validator**
    - Validierung von Zugriffstokens und Wunschdaten.
    - Rückgabe eines Validierungsstatus (z. B. gültig/ungültig).

3. **Wishes-Service**
    - Verarbeitung von Anfragen für Wünsche (Lesen, Hinzufügen, Aktualisieren, Löschen).
    - Kommunikation mit der MySQL-Datenbank für Datenspeicherung.

4. **MySQL-Datenbank**
    - Persistente Speicherung der Wünsche in einer relationalen Struktur.

## Datenstrukturen
### Auth/Validator: ValidationRequest und ValidationResponse

#### ValidationRequest (vom API Gateway zum Validator-Service)
```json
{
  "AccessToken": "string",       // C#: string
  "Wish": {
    "Id": "int",               // C#: int
    "Description": "string",    // C#: string
    "Status": "string"          // C#: Enum
  },
  "Method": "string"            // C#: string (z. B. GET, POST, etc.)
}
```
**Größe im Speicher (ca.):** 500 Bytes (je nach Wunschlänge).

#### ValidationResponse (vom Validator-Service zurück zum API Gateway)
```json
{
  "IsValid": true,                // C#: bool
  "Message": "string",          // C#: string
  "ValidatedWish": {
    "Id": "int",               // C#: int
    "Description": "string",    // C#: string
    "Status": "string"          // C#: Enum
  }
}
```
**Größe im Speicher (ca.):** 300–500 Bytes.

### Wishes-Service: WishesRequest und WishesResponse

#### WishesRequest (vom API Gateway zum Wishes-Service)
```json
{
  "Action": "string",           // C#: string (z. B. ADD, DELETE, UPDATE)
  "Wish": {
    "Id": "int",               // C#: int
    "Description": "string",    // C#: string
    "Status": "string"          // C#: Enum
  }
}
```
**Größe im Speicher (ca.):** 300–500 Bytes.

#### WishesResponse (vom Wishes-Service zurück zum API Gateway)
```json
{
  "Success": true,                // C#: bool
  "Message": "string",          // C#: string
  "Data": [
    {
      "Id": "int",             // C#: int
      "Description": "string",  // C#: string
      "Status": "string"        // C#: Enum
    }
  ]
}
```
**Größe im Speicher (ca.):** 1 KB (bei einer Liste von ~10 Wünschen).

### MySQL-Datenbank: Wish
```sql
CREATE TABLE Wish (
  Id INTEGER AUTO_INCREMENT PRIMARY KEY,
  Description VARCHAR(500),
  Status ENUM('Formulated', 'InProgress', 'Delivering', 'UnderTree')
);
```

**Datenvolumen:**
- Bei 8 Milliarden Wünschen mit durchschnittlich 250 Zeichen pro Beschreibung beträgt das Speichervolumen ca. 2 TB (ohne Indizes).

## Docker-Setup
Unsere Dienste werden mithilfe von Docker-Containern bereitgestellt. Die Konfiguration erfolgt über die folgende `docker-compose.yaml`:

```yaml
version: '3.8'

services:
  # API Gateway Service (Container 1)
  xmaswishes-api-gateway:
    image: xmaswishes-api-gateway
    build:
      context: .
      dockerfile: XmasWishes/Dockerfile
    ports:
      - "8080:8080"  # Host-Port:Container-Port
    depends_on:
      - xmaswishes-validator
      - xmaswishes-db
    networks:
      xmaswishes-net:
        ipv4_address: 172.18.0.10

  # Validator Service (Container 2)
  xmaswishes-validator:
    image: xmaswishes-validator
    build:
      context: .
      dockerfile: XmasWishes/Dockerfile
    ports:
      - "8081:8080"  # Host-Port:Container-Port
    networks:
      xmaswishes-net:
        ipv4_address: 172.18.0.11

  # Data Service (Container 3)
  xmaswishes-data-service:
    image: xmaswishes-data-service
    build:
      context: .
      dockerfile: XmasWishes/Dockerfile
    ports:
      - "8082:8080"  # Host-Port:Container-Port
    depends_on:
      - xmaswishes-db
    networks:
      xmaswishes-net:
        ipv4_address: 172.18.0.12

  # MySQL Datenbank (Container 4)
  xmaswishes-db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword  # Setzt das Root Passwort für MySQL
      MYSQL_DATABASE: xmaswishes_db    # Name der Datenbank, die beim Start erstellt wird
    ports:
      - "3306:3306"  # Host-Port:Container-Port
    volumes:
      - mysql-data:/var/lib/mysql  # Persistente Speicherung der Daten
    restart: always
    networks:
      xmaswishes-net:
        ipv4_address: 172.18.0.13

networks:
  xmaswishes-net:
    driver: bridge
    ipam:
      config:
        - subnet: 172.18.0.0/16

volumes:
  mysql-data:  # Volumen für MySQL-Datenbank
```

## Analyse: Datenverkehr und Skalierung
### Annahmen
Um die Skalierbarkeit und Anforderungen unserer Architektur zu analysieren, gehen wir von folgenden Annahmen aus:

- **Bevölkerung**: 8 Milliarden Menschen weltweit.
- **Zeitraum**: Die Einsendung der Wünsche erfolgt innerhalb von 90 Tagen (ca. 3 Monate) vor Weihnachten.
- **Wünsche pro Person**: Jede Person sendet durchschnittlich **einen Wunsch** ein.

### Datenverkehrsberechnung

1. **Gesamtanzahl der Wünsche**: 8 Milliarden.
2. **Dauer**: 90 Tage × 24 Stunden × 60 Minuten = **129.600 Minuten**.
3. **Durchschnittliche API-Aufrufe pro Minute**: 8.000.000.000 Wünsche / 129.600 Minuten = ca. **61.728 API-Aufrufe/Minute**.

**Peaks:**
Da der Traffic ungleichmäßig verteilt ist, können zu Spitzenzeiten (z. B. in den letzten Tagen vor Weihnachten) Peaks von bis zu **10×** der durchschnittlichen Last auftreten. Dies ergibt:
- **Maximaler Traffic**: 617.280 API-Aufrufe/Minute.

## Datenvolumen und Speicherkapazität

### Speicherbedarf für Wünsche
Ein Wunsch wird in der MySQL-Datenbank wie folgt gespeichert:
```sql
CREATE TABLE Wish (
  Id INTEGER AUTO_INCREMENT PRIMARY KEY,
  Description VARCHAR(500),
  Status ENUM('Formulated', 'InProgress', 'Delivering', 'UnderTree')
);
```

- **Id**: 4 Bytes (INTEGER).
- **Description**: Durchschnittlich 250 Zeichen, entspricht 250 Bytes (1 Zeichen = 1 Byte in UTF-8).
- **Status**: 1 Byte (ENUM).

**Gesamtgröße pro Wunsch**: 255 Bytes.

- **Gesamtspeicher für 8 Milliarden Wünsche**: 255 Bytes × 8.000.000.000 = ca. **2 Terabyte** (TB).
- Zusätzlicher Speicherbedarf für Indizes, Backups und Replikation: ca. **4–6 TB**.

### Speicherbedarf für API-Requests
Die Requests und Responses werden in JSON-Format verarbeitet. Ein durchschnittlicher API-Request hat folgende Größe:

- **ValidationRequest**: ~500 Bytes pro Request.
- **ValidationResponse**: ~300 Bytes pro Response.
- **WishesRequest**: ~400 Bytes pro Request.
- **WishesResponse**: ~1 KB pro Response (bei 10 Wünschen).

Daraus ergibt sich:
- **Durchschnittlicher Traffic pro Minute**: 61.728 × (500 Bytes + 300 Bytes + 400 Bytes + 1.000 Bytes) = ca. **130 MB/Minute**.
- **Gesamter Traffic in 90 Tagen**: 130 MB × 129.600 Minuten = ca. **15 Terabyte (TB)** an eingehendem und ausgehendem Traffic.

## Skalierungsstrategie

### Load Balancer
Das API-Gateway verteilt die Last gleichmäßig auf die **N Instanzen** der Validator- und Wishes-Services:
- **Validator-Services**: Prüfen Authentifizierung und Validierung.
- **Wishes-Services**: Verarbeiten die Wünsche und speichern sie in der Datenbank.

Ein Load Balancer (z. B. AWS Elastic Load Balancer, NGINX oder Traefik) stellt sicher, dass die Dienste basierend auf der aktuellen Auslastung skaliert werden.

### Horizontale Skalierung
- **API Gateway**: Zusätzliche Instanzen bei hoher Anfragezahl.
- **Validator-Services**: Skalieren auf 10–20 Instanzen für Traffic-Peaks.
- **Wishes-Services**: Zusätzliche Instanzen für parallele Verarbeitung von Wünschen.
- **Datenbank**: Verwendung von MySQL-Read-Replicas zur Lastverteilung bei Leseoperationen.

### Vertikale Skalierung
- Einsatz leistungsfähigerer Server für zentrale Dienste wie die Datenbank.

## Hardwareanforderungen
Basierend auf dem Traffic und den Speicherkapazitäten:

- **API Gateway und Microservices**:
    - 4 CPU-Kerne.
    - 8–16 GB RAM pro Instanz.
    - 10–20 Instanzen bei Peaks.

- **MySQL-Datenbank**:
    - Primäre Datenbank: 32 CPU-Kerne, 128 GB RAM.
    - Speicher: 4–6 TB SSD (inkl. Backups).
    - Read-Replicas: 3–5 Instanzen mit je 16 CPU-Kernen und 64 GB RAM.

- **Netzwerkbandbreite**:
    - Mindestens 1 Gbps pro Server.
    - Load Balancer: 10 Gbps Bandbreite.

## Kostenübersicht
### Cloud-Bereitstellung (z. B. AWS, Azure, GCP)
- **Compute**: 50–60 Instanzen × $0,10/Stunde = $4.320–5.184/Monat.
- **Speicher**: 6 TB SSD × $0,12/GB = $720/Monat.
- **Netzwerk**: 15 TB × $0,09/GB = $1.350/Monat.

**Gesamtkosten pro Monat**: ca. **$6.390–7.254**.

### Optimierungsmöglichkeiten
- Verwendung von Spot-Instanzen für Validator- und Wishes-Services.
- Datenkompression bei API-Requests und -Responses.
- Caching für häufige Leseanfragen.



