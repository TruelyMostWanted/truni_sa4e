# Aufgabe 1: Konzeptuelle Modellierung

## Überblick
Dieses Konzept beschreibt die Architektur und das Design einer Microservices-basierten Anwendung, die eine Weihnachtswünsche-Verwaltung ermöglicht. Ziel ist es, eine skalierbare und wartbare Anwendung zu entwerfen, bei der einzelne Dienste klar voneinander getrennt sind und spezifische Verantwortlichkeiten haben.

### Grundlegende Interaktionskette
1. **Client ⇒ API Gateway**: Der Client sendet Anfragen an das API Gateway.
2. **API Gateway ⇒ Auth/Validator**: Das API Gateway prüft Anfragen, indem es den Authentifizierungs- und Validierungsdienst aufruft.
3. **API Gateway ⇐ Auth/Validator**: Der Auth/Validator gibt das Ergebnis der Prüfung an das API Gateway zurück.
4. **API Gateway ⇒ Wishes-Service**: Nach erfolgreicher Validierung leitet das API Gateway die Anfrage an den Wishes-Service weiter.
5. **Wishes-Service ⇒ MySQL-Datenbank**: Der Wishes-Service kommuniziert mit der MySQL-Datenbank, um Daten zu speichern oder abzurufen.
6. **Wishes-Service ⇐ MySQL-Datenbank**: Die Datenbank sendet die Ergebnisse der Abfragen an den Wishes-Service.
7. **API Gateway ⇐ Wishes-Service**: Der Wishes-Service leitet die Ergebnisse an das API Gateway weiter.
8. **Client ⇐ API Gateway**: Das API Gateway liefert die finalen Ergebnisse an den Client.

## Dienste und Endpunkte
### API Gateway
Das API Gateway ist der zentrale Einstiegspunkt für alle Client-Anfragen und koordiniert die Kommunikation zwischen den verschiedenen Diensten.

**Endpunkte:**
- `GET /api/requests/`: Liefert alle Wünsche aus der Datenbank.
- `POST /api/requests/`: Fügt einen neuen Wunsch hinzu. { "Description": Wunschtext }
- `POST /api/requests/`: Fügt einen neuen Wunsch mit einer Datei hinzu. { "Description": Wunschtext, "FileName": "" }

**Funktionen:**
- Routing und Weiterleitung von Anfragen an die entsprechenden Dienste.
- Integration von Authentifizierungs- und Validierungsmechanismen über den Auth/Validator-Dienst.
- Aggregation der Ergebnisse aus verschiedenen Diensten.

### Auth/Validator
Der Authentifizierungs- und Validierungsdienst übernimmt die Prüfung von Zugriffstokens und validiert die Anfrageinhalte.

**Endpunkte:**
- `POST /api/validator/`: Prüft Zugriffstoken und validiert die Inhalte (z. B. Wunschlänge).

**Funktionen:**
- Validierung des AccessTokens zur Authentifizierung des Clients.
- Validierung von Wünschen (z. B. maximale Länge von 250 Zeichen).
- Rückgabe eines Validierungsstatus und optional modifizierter Daten.

### Wishes-Service
Der Wishes-Service ist für die Datenmanipulation von Wünschen verantwortlich und kommuniziert direkt mit der MySQL-Datenbank.

**Endpunkte:**
- `GET /api/wishes/`: Liefert alle Wünsche aus der Datenbank.
- `POST /api/wishes/`: Fügt einen neuen Wunsch zur Datenbank hinzu.
- `DELETE /api/wishes/`: Entfernt einen Wunsch aus der Datenbank.
- `PATCH /api/wishes/`: Aktualisiert einen bestehenden Wunsch in der Datenbank.

**Funktionen:**
- Entgegennahme von Anfragen, die über das API Gateway geleitet wurden.
- Verarbeitung der Anfragen durch direkte Interaktion mit der MySQL-Datenbank.
- Sicherstellung, dass nur validierte und authentifizierte Anfragen bearbeitet werden.

### MySQL-Datenbank
Die MySQL-Datenbank speichert alle Wünsche in einer relationalen Struktur.

**Tabelle:**
- **Wishes**
    - `Id` (INT): Eindeutige ID für jeden Wunsch.
    - `Description` (Text): Beschreibung des Wunsches.
	- `FileName` (Text): Wird der Wunsch als Datei gespeichert? Wo?
    - `Status` (Enum): Status des Wunsches (z. B. `Formulated`, `InProgress`, `Delivering`, `UnderTree`).

**Funktionen:**
- Speicherung und Abruf von Wünschen.
- Sicherstellung von Datenkonsistenz und Integrität.

## Datenstrukturen
### Wish
Das zentrale Modell zur Repräsentation eines Wunsches.
- `Id`: Eindeutige Kennung.
- `Description`: Beschreibung des Wunsches.
- `FileName`: Dateiname/Pfad falls der Wunsch in einer Datei steht
- `Status`: Aktueller Status des Wunsches.

### ValidationRequest
Datenstruktur zur Validierung von Anfragen.
- `AccessToken`: Token zur Authentifizierung des Clients.
- `Wish`: Der Wunsch, der validiert werden soll.

### ValidationResponse
Antwortstruktur des Auth/Validator-Dienstes.
- `IsValid`: Gibt an, ob die Anfrage gültig ist.
- `Message`: Zusätzliche Informationen zur Validierung.
- `ValidatedWish`: (Optional) Validierte Wunschdaten.

## Ablaufdiagramm (Pseudocode)
1. **Client sendet Anfrage an API Gateway.**
    - API Gateway leitet Anfrage an Auth/Validator weiter.
2. **Auth/Validator prüft die Anfrage.**
    - AccessToken und Wunschinhalt werden validiert.
    - Antwort wird an das API Gateway zurückgegeben.
3. **API Gateway leitet Anfrage an Wishes-Service weiter (bei positiver Validierung).**
    - Wishes-Service interagiert mit der MySQL-Datenbank.
4. **Datenbank verarbeitet Anfrage und liefert Ergebnisse an Wishes-Service.**
    - Ergebnisse werden an das API Gateway weitergeleitet.
5. **API Gateway gibt die Antwort an den Client zurück.**

