# Analyse der Hardwarekapazität und Belastungsgrenzen

## Systemübersicht
### Hardware
- **Prozessor**: AMD Ryzen 7 3800X 8-Core Processor @ 3.90 GHz
- **Arbeitsspeicher**: 32 GB DDR4 RAM
- **Speicherplatz**: 2.5 TB HDD
- **Architektur**: x64

### Docker-Netzwerk
Das System besteht aus einem Docker-Netzwerk mit vier Containern:
- **API Gateway**: Schnittstelle für alle eingehenden Anfragen.
- **Validator-Service**: Validierung von Daten und Zugriffstokens.
- **Wishes-Service**: Verarbeitung von Wünschen.
- **MySQL-Datenbank**: Speicherung aller Wunschdaten.

## Struktur eines Wunsches
```csharp
class Wunsch
{
    int Id;                  // 4 Bytes
    string Description;      // max. 500 Zeichen = 500 Bytes
    string FileName;         // max. 100 Zeichen = 100 Bytes
    string Status;           // max. 32 Zeichen = 32 Bytes
}
```
### Speicherbedarf pro Wunsch
| Feld            | Max. Speicher (Bytes) |
|-----------------|------------------------|
| `Id`           | 4                      |
| `Description`  | 500                    |
| `FileName`     | 100                    |
| `Status`       | 32                     |
| **Gesamt**     | 636 Bytes              |

### Zusätzlicher Overhead
- **Datenbankverwaltung**: Indexe, Metadaten (ca. 20% Overhead).
- **Effektiver Speicherbedarf pro Wunsch**: ca. **764 Bytes**.

## Maximale Datenspeicherung
### Verfügbarer Speicherplatz
- Gesamtspeicher: **2.5 TB = 2,500 GB = 2,500,000 MB**.
- Reserviert für Betriebssystem und Logs: **500 GB**.
- Verfügbar für Wunsch-Datenbank: **2,000 GB**.

### Anzahl speicherbarer Wünsche
- Speicher pro Wunsch: **764 Bytes**.
- Maximale Anzahl Wünsche:  
  \( \frac{2,000,000 \text{ MB} \times 1,048,576 \text{ Bytes}}{764 \text{ Bytes}} \approx 2,741,000,000 \).

**Ergebnis:** Es können ca. **2.74 Milliarden Wünsche** gespeichert werden.

## Verarbeitungskapazität
### CPU-Leistung
- Der AMD Ryzen 7 3800X verfügt über 8 Kerne und 16 Threads.
- Geschätzte Verarbeitung pro Thread:
  - API Gateway: 1 ms pro Anfrage.
  - Validator: 2 ms pro Anfrage.
  - Wishes-Service: 3 ms pro Anfrage.
  - MySQL-Query: 2 ms pro Anfrage.
- Gesamtlaufzeit pro Anfrage: **8 ms**.

#### Maximale Anfragen pro Sekunde
- Threads: 16
- Max. Anfragen/Sekunde:  
  \( \frac{1}{0.008 \text{ Sek.}} \times 16 \approx 2,000 \text{ Anfragen/Sekunde} \).

#### Maximale Anfragen pro Tag
- Max. Anfragen/Tag:  
  \( 2,000 \times 86,400 \approx 172,800,000 \text{ Anfragen} \).

### Arbeitsspeicher
- Verfügbarer RAM für Docker-Container: **28 GB** (nach Abzug von Betriebssystem und anderen Prozessen).
- Geschätzter Speicherbedarf pro Anfrage:
  - API Gateway: 512 KB.
  - Validator: 256 KB.
  - Wishes-Service: 512 KB.
  - MySQL-Query: 1 MB.
  - **Gesamt**: ca. **2.25 MB** pro Anfrage.

#### Maximale gleichzeitige Anfragen
- Verfügbarer RAM / Speicher pro Anfrage:  
  \( \frac{28,000 \text{ MB}}{2.25 \text{ MB}} \approx 12,444 \text{ gleichzeitige Anfragen} \).

## Belastungsgrenzen und DDoS-Schwelle
### DDoS-Schwelle
- Bei mehr als **2,000 Anfragen/Sekunde** wird die CPU zum Flaschenhals.
- Arbeitsspeicher wird knapp bei mehr als **12,444 gleichzeitigen Anfragen**.

### Simulierter DDoS-Angriff
- Angenommene Angriffslast: **10,000 Anfragen/Sekunde**.
- Folgen:
  - CPU-Threads überlastet.
  - Latenzzeiten steigen überproportional.
  - API Gateway wird unresponsive.

## Fazit
- Das System ist in der Lage, **2.74 Milliarden Wünsche** zu speichern.
- Es können **2,000 Anfragen/Sekunde** verarbeitet werden.
- Bei mehr als **12,444 gleichzeitigen Anfragen** oder **10,000 Anfragen/Sekunde** würde das System durch DDoS-Angriffe beeinträchtigt.

### Empfehlungen zur Verbesserung
1. **Horizontal skalieren**:
   - Zusätzliche API Gateway-, Validator- und Wishes-Service-Instanzen.
   - Load Balancing (z. B. mit Kubernetes).
2. **Caching**:
   - Redis oder Memcached für häufig angefragte Daten.
3. **DDoS-Schutz**:
   - Verwendung von Cloudflare oder ähnlichen Diensten zur Begrenzung von Angriffen.

