# XmasWishes Project

## Setup Instructions

1) Öffnen Sie Docker
- Dadurch wird der Docker-Dienst und die Engine gestartet



2) Starten Sie die docker_run_compose.bat
- Dadurch werden die Docker-Images erstellt
- Die Docker-Container werden gestartet



3) Starten die die initialize.sql via Command-Line
- Dadurch werden die benötigten Tabellen initialisiert

```
mysql -h localhost -P 3306 -u root -p xmaswishes_db < initialize.sql
```
Das Passwort ist:
```
rootpassword
```

4) Starten sie einen der Tests
z.B /tests/AddWish.http
z.B /tests/GetWishes.http

ALTERNATIV:
- Rufen Sie die URL http://localhost:8080/api/requests im Browser auf (GET-Request für alle Wünsche in der DB zu lesen)
- Rufen Sie die URL http://localhost:8080/gui/make-a-wish auf. Das ist eine GUI zum erstellen von Wünschen

