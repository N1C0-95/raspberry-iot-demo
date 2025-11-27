# RaspberryIoT - Backend API

Sistema backend per monitoraggio IoT su Raspberry Pi con integrazione Power Platform Custom Connector.

## 🏗️ Architettura

```
┌─────────────────────────────────────────────────┐
│ RASPBERRY PI                                    │
│                                                 │
│ ┌─────────────────┐       ┌─────────────────┐ │
│ │ Worker Service  │       │ ASP.NET Core API│ │
│ │ (GPIO Monitor)  │       │ (HTTP Endpoints)│ │
│ └────────┬────────┘       └────────┬────────┘ │
│          │                         │          │
│          └──────────┬──────────────┘          │
│                     ↓                          │
│          ┌─────────────────────────┐          │
│          │ Application Layer       │          │
│          │ (Services + Interfaces) │          │
│          └────────────┬────────────┘          │
│                       ↓                        │
│          ┌─────────────────────────┐          │
│          │ Infrastructure Layer    │          │
│          │ (Repositories + Dapper) │          │
│          └────────────┬────────────┘          │
│                       ↓                        │
│          ┌─────────────────────────┐          │
│          │   SQLite Database       │          │
│          │ • SensorStatus (storico)│          │
│          │ • SensorEvents (log)    │          │
│          └─────────────────────────┘          │
└─────────────────────────────────────────────────┘
                       ↑
                       │ HTTP (ngrok)
                       │
              ┌────────┴────────┐
              │ Power Automate  │
              │ (Polling 30s)   │
              └─────────────────┘
```

## 📁 Struttura Progetti

```
RaspberryIoT.sln
├── RaspberryIoT.Contracts/          # DTOs e ApiRoutes
│   ├── Requests/                    # Request models
│   ├── Responses/                   # Response models
│   └── ApiRoutes.cs                 # Costanti rotte API
│
├── RaspberryIoT.Application/        # Business Logic Layer
│   ├── Models/                      # Entities + Enums
│   ├── Services/                    # Services + Orchestrator
│   ├── Repositories/                # Repository interfaces
│   └── Database/                    # Database interfaces
│
├── RaspberryIoT.Infrastructure/     # Data Access Layer
│   ├── Repositories/                # Repository implementations (Dapper)
│   └── Database/                    # DbConnectionFactory + DbInitializer
│
├── RaspberryIoT.Api/                # Web API
│   └── Controllers/                 # API Controllers
│
└── RaspberryIoT.Worker/             # ⭐ Background Service (GPIO Monitor)
    ├── Worker.cs                    # Background Service
    ├── Program.cs                   # DI Configuration
    └── appsettings.json             # Worker Config
```

### Dipendenze Progetti

- **Contracts**: nessuna dipendenza
- **Application**: → **Contracts**
- **Infrastructure**: → **Application** (transitivamente anche Contracts)
- **Api**: → **Application**, **Infrastructure**, **Contracts**
- **Worker**: → **Application**, **Infrastructure** ⭐

### 🎯 SensorOrchestrator

Coordinatore per operazioni complesse (usato dal Worker):

```csharp
ISensorOrchestrator
  ├── HandleErrorDetectedAsync()    // Crea Status + Event atomicamente
  └── HandleRebootStartedAsync()    // Crea Status + Event atomicamente
```

## 🗄️ Database Schema (SQLite)

### Tabella: SensorStatus

Storico dei cambi di stato del sensore.

```sql
CREATE TABLE SensorStatus (
  RowId INTEGER PRIMARY KEY AUTOINCREMENT,  -- Per polling Power Platform
  Id TEXT NOT NULL UNIQUE,                  -- Guid per business logic
  SensorId TEXT NOT NULL,                   -- "RASPBERRY-DEMO-001"
  Status TEXT NOT NULL,                     -- 'Online', 'Error', 'Rebooting'
  LedColor TEXT NOT NULL,                   -- 'Green', 'Red', 'Off'
  ChangedOn TEXT NOT NULL,                  -- 'worker', 'api_manual', 'power_automate'
  Timestamp DATETIME NOT NULL,              -- Business timestamp
  CreatedAt DATETIME NOT NULL,              -- Audit
  UpdatedAt DATETIME NOT NULL               -- Audit
);
```

### Tabella: SensorEvents

Log eventi del sensore (append-only).

```sql
CREATE TABLE SensorEvents (
  RowId INTEGER PRIMARY KEY AUTOINCREMENT,  -- Per polling Power Platform
  Id TEXT NOT NULL UNIQUE,                  -- Guid per business logic
  EventType TEXT NOT NULL,                  -- 'ErrorDetected', 'RebootStarted', 'RebootCompleted'
  Status TEXT NOT NULL,                     -- Status testuale al momento dell'evento
  TriggeredBy TEXT NOT NULL,                -- 'worker', 'api_manual', ecc.
  Timestamp DATETIME NOT NULL,
  CreatedAt DATETIME NOT NULL
);
```

## 🚀 API Endpoints

### SensorStatus Endpoints

#### 1. Get All Statuses
```http
GET /api/sensor/status
```
Ritorna tutti gli stati (ordinati per RowId DESC).

**Response:** `200 OK`
```json
[
  {
    "rowId": 1,
    "id": "guid...",
    "sensorId": "RASPBERRY-DEMO-001",
    "status": "Online",
    "ledColor": "Green",
    "changedOn": "worker",
    "timestamp": "2024-11-27T10:00:00Z",
    "createdAt": "2024-11-27T10:00:00Z",
    "updatedAt": "2024-11-27T10:00:00Z"
  }
]
```

#### 2. Get Status by ID
```http
GET /api/sensor/status/{id}
```

**Response:** `200 OK` | `404 Not Found`

#### 3. Get Current Status by SensorId
```http
GET /api/sensor/status/current/{sensorId}
```
Ritorna l'ultimo stato per il sensorId specificato.

**Response:** `200 OK` | `404 Not Found`

#### 4. Poll for New Statuses (Power Platform)
```http
GET /api/sensor/status/poll?sinceRowId=0
```
Ritorna solo record con `RowId > sinceRowId`.

**Response:** `200 OK`
```json
{
  "data": [
    {
      "rowId": 125,
      "id": "guid...",
      "sensorId": "RASPBERRY-DEMO-001",
      "status": "Error",
      "ledColor": "Red",
      "changedOn": "worker",
      "timestamp": "2024-11-27T10:05:00Z",
      "createdAt": "2024-11-27T10:05:00Z",
      "updatedAt": "2024-11-27T10:05:00Z"
    }
  ]
}
```

#### 5. Update Status
```http
PUT /api/sensor/status/{id}
Content-Type: application/json

{
  "sensorId": "RASPBERRY-DEMO-001",
  "status": "Online",
  "ledColor": "Green",
  "changedOn": "api_manual"
}
```

**Response:** `204 No Content` | `404 Not Found`

#### 6. Force Error
```http
POST /api/sensor/error
Content-Type: application/json

{
  "sensorId": "RASPBERRY-DEMO-001",
  "triggeredBy": "power_automate"
}
```
Crea un nuovo record con `Status=Error` e `LedColor=Red`.

**Response:** `201 Created`

#### 7. Force Reboot
```http
POST /api/sensor/reboot
Content-Type: application/json

{
  "sensorId": "RASPBERRY-DEMO-001",
  "triggeredBy": "power_automate"
}
```
Crea un nuovo record con `Status=Rebooting` e `LedColor=Off`.

**Response:** `201 Created`

---

### SensorEvents Endpoints

#### 1. Get All Events
```http
GET /api/sensor/events?eventType=ErrorDetected
```
Ritorna tutti gli eventi (opzionale filtro per `eventType`).

**Query Parameters:**
- `eventType` (optional): `ErrorDetected`, `RebootStarted`, `RebootCompleted`

**Response:** `200 OK`
```json
[
  {
    "rowId": 1,
    "id": "guid...",
    "eventType": "ErrorDetected",
    "status": "Error",
    "triggeredBy": "worker",
    "timestamp": "2024-11-27T10:00:00Z",
    "createdAt": "2024-11-27T10:00:00Z"
  }
]
```

#### 2. Get Event by ID
```http
GET /api/sensor/events/{id}
```

**Response:** `200 OK` | `404 Not Found`

#### 3. Poll for New Events (Power Platform)
```http
GET /api/sensor/events/poll?sinceRowId=0
```
Ritorna solo eventi con `RowId > sinceRowId`.

**Response:** `200 OK`
```json
{
  "data": [
    {
      "rowId": 50,
      "id": "guid...",
      "eventType": "RebootStarted",
      "status": "Rebooting",
      "triggeredBy": "api_manual",
      "timestamp": "2024-11-27T10:10:00Z",
      "createdAt": "2024-11-27T10:10:00Z"
    }
  ]
}
```

#### 4. Create Event
```http
POST /api/sensor/events
Content-Type: application/json

{
  "eventType": "ErrorDetected",
  "status": "Error",
  "triggeredBy": "worker"
}
```

**Response:** `201 Created`

---

## 🔧 Configurazione

### appsettings.json

```json
{
  "Database": {
    "ConnectionString": "Data Source=sensor.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Dependency Injection (Program.cs)

```csharp
// Database
builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(connectionString));
builder.Services.AddSingleton<IDbInitializer, DbInitializer>();

// Repositories
builder.Services.AddSingleton<ISensorStatusRepository, SensorStatusRepository>();
builder.Services.AddSingleton<ISensorEventRepository, SensorEventRepository>();

// Services
builder.Services.AddSingleton<ISensorStatusService, SensorStatusService>();
builder.Services.AddSingleton<ISensorEventService, SensorEventService>();
```

---

## 🚦 Come Utilizzare

### 1. Avvio API

```bash
cd RaspberryIoT.Api
dotnet run
```

L'API sarà disponibile su: `http://localhost:5193`

### 2. Swagger UI

Durante lo sviluppo, accedi a Swagger per testare gli endpoint:
```
http://localhost:5193/swagger
```

### 3. Test Manuale con curl

**Creare un nuovo stato:**
```bash
curl -X POST http://localhost:5193/api/sensor/error \
  -H "Content-Type: application/json" \
  -d '{
    "sensorId": "RASPBERRY-DEMO-001",
    "triggeredBy": "test_manual"
  }'
```

**Polling per nuovi stati:**
```bash
curl http://localhost:5193/api/sensor/status/poll?sinceRowId=0
```

---

## 🔌 Integrazione Power Platform

### Custom Connector - Polling Trigger

**Endpoint da configurare:**
```
GET /api/sensor/status/poll?sinceRowId={lastRowId}
```

**Flusso:**
1. Power Automate chiama l'endpoint ogni 30s
2. Passa `sinceRowId` con l'ultimo RowId processato (o 0 al primo giro)
3. API ritorna solo record con `RowId > sinceRowId`
4. Se `data` array è vuoto → nessun nuovo evento
5. Se `data` contiene record → trigger attivato!
6. Power Automate salva il `rowId` massimo per la prossima chiamata

---

## 📦 Pacchetti NuGet Utilizzati

- **Dapper** (2.1.66) - Micro ORM per query SQL
- **Microsoft.Data.Sqlite** (10.0.0) - SQLite provider
- **Microsoft.AspNetCore.OpenApi** - Swagger/OpenAPI

---

## 🤖 RaspberryIoT.Worker - Background Service

### Descrizione

Worker Service che monitora lo stato dei LED del Raspberry Pi (attualmente **simulato** per test, pronto per GPIO reali).

### Funzionalità

- **Polling Periodico**: Monitora lo stato ogni 5 secondi (configurabile)
- **Rilevamento Cambi Stato**: Rileva quando il LED passa da Green → Red → Off
- **Scrittura Atomica**: Usa **SensorOrchestrator** per scrivere Status + Event coordinatamente
- **Simulazione**: Genera stati casuali (80% Green, 15% Red, 5% Off) per test senza hardware

### Esecuzione

```bash
cd RaspberryIoT.Worker
dotnet run
```

### Configurazione (appsettings.json)

```json
{
  "Database": {
    "ConnectionString": "Data Source=sensor.db"
  },
  "Worker": {
    "PollingIntervalMs": 5000,
    "SensorId": "RASPBERRY-DEMO-001"
  }
}
```

### Log di Esempio

```
info: RaspberryIoT.Worker.Worker[0]
      Worker started - SensorId: RASPBERRY-DEMO-001, Polling: 5000ms
info: RaspberryIoT.Worker.Worker[0]
      Current state: Green
info: RaspberryIoT.Worker.Worker[0]
      State changed from Green to Red - calling orchestrator
info: RaspberryIoT.Worker.Worker[0]
      Orchestrator HandleErrorDetected completed successfully
```

### Integrazione GPIO Reale (Futuro)

Sostituire il metodo `SimulateGpioRead()` con:

```csharp
using System.Device.Gpio;

private LedColor ReadRealGpio()
{
    // Pin 17 per LED Green, Pin 27 per LED Red
    bool greenOn = _gpioController.Read(17) == PinValue.High;
    bool redOn = _gpioController.Read(27) == PinValue.High;
    
    if (greenOn) return LedColor.Green;
    if (redOn) return LedColor.Red;
    return LedColor.Off;
}
```

### Architettura

- **Worker.cs**: Background Service con loop di monitoraggio
- **SensorOrchestrator**: Gestisce scrittura coordinata SensorStatus + SensorEvent
- **Dependency Injection**: Condivide gli stessi Services/Repositories dell'API
- **Database Condiviso**: Worker e API scrivono sullo stesso `sensor.db`

---

## 🛠️ Prossimi Passi

1. **Deploy su Raspberry Pi**: Copiare tutto il progetto
2. **Installare ngrok**: Esporre API pubblica per Power Platform
3. **Avviare Worker + API**: Due processi separati, stesso database
4. **Configurare systemd**: Auto-start al boot del Raspberry Pi
5. **Sostituire GPIO Simulation**: Implementare lettura reale pin GPIO
6. **Testare Polling Power Platform**: Verificare chiamate `/api/sensorstatus/poll` e `/api/sensorevents/poll`

---

## 📝 Note Tecniche

### Perché RowId AUTOINCREMENT?

- **Sequenziale garantito**: SQLite genera RowId in modo atomico
- **Performance**: Query `WHERE RowId > X` è velocissima (PRIMARY KEY)
- **Affidabilità**: Nessun problema con timestamp duplicati
- **Compatibilità**: Pattern standard per polling trigger Custom Connector

### Perché Guid per Id?

- **Business Logic**: Identificatore univoco per reference esterne
- **Scalabilità**: Possibile distribuzione futura
- **Best Practice**: Separazione tra PK tecnica (RowId) e business key (Id)

---

## 👨‍💻 Autore

Progetto demo per conferenza - Gestione IoT su Raspberry Pi con Power Platform integration.
