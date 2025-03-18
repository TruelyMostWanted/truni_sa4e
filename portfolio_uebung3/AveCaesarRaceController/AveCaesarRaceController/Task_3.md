# Änderungen in der Software

## 1. Neue Anforderungen
Neben den bereits bestehenden Anforderungen wird **eine zusätzliche Einschränkung** eingeführt:

1. **Segmente haben eine maximale Anzahl von Spielern**:
    - Zur Verursachung von Engpässen und Wettbewerbssituationen sollen die Segmente eine **maximale Anzahl von Spielern** aufnehmen können.
    - Dadurch wird die Spielerbewegung realistischer und die Interaktionen zwischen den Spielern intensiviert.
    - **Start-Goal-Segmente** können unbegrenzt viele Spieler speichern (`MaxPlayers = int.MaxValue`).
    - Alle anderen Segment-Typen (z. B. `Caesar`, `Bottleneck`, `Normal`) speichern nur **1 Spieler gleichzeitig** (`MaxPlayers = 1`).

2. **Speichern von PlayerToken pro Segment**:
    - Segmente registrieren Spieler, wenn sie das Segment betreten,
    - Segmenten entfernen Spieler, wenn sie das Segment verlassen.
    - Die registrierten Spieler werden in einer Liste von PlayerToken gespeichert
    - Das Hinzufügen eines Spielers zum Segment ist nur möglich, wenn die Segmentkapazität (`MaxPlayers`) noch nicht überschritten ist.

3. **Spieler-Bewegung bzw. Segment-Übergang**:
    - Spieler dürfen nur dann zu einem Segment wechseln, wenn Platz verfügbar ist.
    - Beim Betreten eines Segments wird der Spieler zur Liste des Segments hinzugefügt.
    - Beim Verlassen eines Segments wird der Spieler aus der Liste entfernt.
---

## 2. Änderungen und betroffene Klassen

### **TrackSegment**
1. **Neue Eigenschaften**:
    - `MaxPlayers`: Gibt an, wie viele Spieler auf dem Segment gleichzeitig erlaubt sind.
    - `CurrentPlayers`: Liste der sich aktuell auf dem Segment befindlichen Spieler (`PlayerToken`).

   Beispielimplementierung:
   ```csharp
   public class TrackSegment
   {
       public static readonly string TYPE_NORMAL = "normal";
       public static readonly string TYPE_START_GOAL = "start-goal";
       public static readonly string TYPE_CAESAR = "caesar";
       public static readonly string TYPE_BOTTLENECK = "bottleneck";

       public string segmentId { get; set; }
       public string type { get; set; }
       public int MaxPlayers { get; set; }
       public List<PlayerToken> CurrentPlayers { get; set; } = new List<PlayerToken>();
       public List<string> nextSegments { get; set; }
   }
   ```

2. **Initialisierung von `MaxPlayers` und `CurrentPlayers`**:
    - **Start-Goal-Segment**:
      ```csharp
      var startSegment = new TrackSegment
      {
          segmentId = startSegmentId,
          type = TrackSegment.TYPE_START_GOAL,
          nextSegments = nextSegments,
          MaxPlayers = int.MaxValue // Unbegrenzte Spieleranzahl
      };
      ```
    - **Nicht-Start-Goal-Segmente (z. B. Caesar, Bottleneck)**:
      ```csharp
      var caesarSegment = new TrackSegment
      {
          segmentId = $"caesar-segment-{t}-1",
          type = TrackSegment.TYPE_CAESAR
          nextSegments = new List<string> { /* Weitere Segmente */ },
          MaxPlayers = 1, // Immer nur ein Spieler erlaubt
      };
      ```

3. **Hilfsmethoden**:
    - Überprüfen, ob neue Spieler hinzugefügt werden können:
      ```csharp
      public bool CanAddPlayer()
      {
          return CurrentPlayers.Count < MaxPlayers;
      }
      ```

    - Spieler hinzufügen:
      ```csharp
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

    - Spieler entfernen:
      ```csharp
      public void RemovePlayer(PlayerToken player)
      {
          CurrentPlayers.Remove(player);
      }
      ```

---

### **SegmentKafkaClient**
1. **Logik für Spielerbewegungen**:
    - **Beim Verlassen eines Segments**:
      Der Spieler wird aus der `CurrentPlayers`-Liste des aktuellen Segments entfernt.
      ```csharp
      Segment.RemovePlayer(playerToken);
      ```

    - **Beim Betreten eines Segments**:
      Es wird geprüft, ob der Spieler das Zielsegment betreten kann (Kapazitätsprüfung).
      ```csharp
      var nextSegmentId = Segment.nextSegments[nextSegmentIndex];
      bool isNextSegmentFree = CheckSegmentAvailability(nextSegmentId);
 
      if (isNextSegmentFree)
      {
          targetSegment.AddPlayer(playerToken);
          Segment.RemovePlayer(playerToken);
          Task.Run(() => SendMessageAsync(TrackSegment.TOPIC_NAME, playerToken.ToJson()));
      }
      ```

2. **Überprüfung der Segmentkapazität**:
    - Kapazitätsprüfung durch `CheckSegmentAvailability`, welches nun die `MaxPlayers`-Beschränkung berücksichtigt:
      ```csharp
      public bool CheckSegmentAvailability(string segmentId)
      {
          // Placeholder-Logik: Kafka-Nachricht könnte den Zustand des Segments überprüfen
          var targetSegment = TrackList.GetSegmentById(segmentId); // Beispiel wie Segment angeschaut wird.
          return targetSegment.CanAddPlayer();
      }
      ```

3. **Caesar-Segmentlogik**:
    - Die Funktionalität zum Erhöhen von `CaesarGreets` bleibt gleich:
      ```csharp
      if (Segment.type.Equals(TrackSegment.TYPE_CAESAR))
      {
          playerToken.CaesarGreets++;
      }
      ```

4. **Bottleneck-Segmentlogik**:
    - Unterstützung für mehrere Nachbarn (`1 → n`-Wege):
      > Hier wird geprüft, ob einer der möglichen Nachbarn den Spieler aufnehmen kann, andernfalls bleibt der Spieler auf dem aktuellen Segment.
      ```csharp
      foreach (var nextSegmentId in Segment.nextSegments)
      {
          var targetSegment = TrackList.GetSegmentById(nextSegmentId);
          if (targetSegment != null && targetSegment.CanAddPlayer())
          {
              targetSegment.AddPlayer(playerToken);
              Segment.RemovePlayer(playerToken);
              break;
          }
      }
      ```

---

### **TrackGenerator**
1. **Initialisierung der neuen Eigenschaften**:
    - Alle Segmente legen `MaxPlayers` entsprechend ihres Typs fest:
        - **Normal-, Caesar- und Bottleneck-Segmente**: `MaxPlayers = 1`
        - **Start-Goal-Segmente**: `MaxPlayers = int.MaxValue`
    - Alle Segmente initialisieren die `CurrentPlayers`-Liste als leer.

2. **Anpassung der Generierungslogik**:
    - Die bisherige Generierung wird um die neuen Attribute (`MaxPlayers`, `CurrentPlayers`) ergänzt.

---

## 3. Testfälle (neue Anforderungen)
1. **Testfall 1 – Segmentkapazität**:
    - Ein Spieler versucht, ein reguläres Segment zu betreten, das bereits mit einem anderen Spieler belegt ist. Der Spieler bleibt blockiert und das Segment lässt ihn nicht zu.

2. **Testfall 2 – Start-Goal-Segment**:
    - Mehrere Spieler versuchen gleichzeitig, ein Start-Goal-Segment zu betreten. Alle Spieler können das Segment betreten, da es keine Kapazitätsbegrenzung gibt.

3. **Testfall 3 – Bottleneck-Segment**:
    - Der Spieler wechselt von einem Segment mit `1 → n` möglichen Nachbarn. Nur Segmente mit freien Kapazitäten können als Ziel ausgewählt werden.

4. **Testfall 4 – Belegungsänderung**:
    - Ein Spieler wechselt von einem Segment A zu einem Segment B. Segment A entfernt den Spieler korrekt aus seiner `CurrentPlayers`-Liste, und Segment B fügt ihn hinzu.

5. **Testfall 5 – Caesar-Segment**:
    - Spieler passiert ein Caesar-Segment und `CaesarGreets` wird um `+1` aktualisiert.

---

## 4. Fazit
Mit der neuen Funktionalität zur Speicherung von Spielern pro Segment und der Begrenzung durch `MaxPlayers` wird die Rennlogik realistischer und ermöglicht eine präzisere Kontrolle der Spielerbewegungen. Die zentralisierte Belegungsprüfung in Kombination mit der Kafka-basierten Kommunikation gewährleistet eine konsistente Segmentverwaltung. Diese Änderungen verbessern die Struktur und erweitern die Spielmöglichkeiten.