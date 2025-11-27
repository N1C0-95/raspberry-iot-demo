# RaspberryIoT.Worker - Background Service

Worker Service per il monitoraggio GPIO su Raspberry Pi.

## 🎯 Funzionalità

- Monitora stato LED ogni 5 secondi (configurabile)
- Rileva cambi di stato (Green → Red → Off)
- Scrive automaticamente su DB tramite `SensorOrchestrator`
- Crea sia `SensorStatus` che `SensorEvent` in modo atomico

## 🔧 Configurazione

### appsettings.json

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

## 🚀 Avvio

### Locale (Development)

```bash
dotnet run --project RaspberryIoT.Worker/RaspberryIoT.Worker.csproj
```

### Raspberry Pi (Production)

```bash
# Build
dotnet publish -c Release -r linux-arm64 --self-contained

# Copia su Raspberry
scp -r bin/Release/net8.0/linux-arm64/publish/ pi@raspberry:/home/pi/worker

# Su Raspberry
cd /home/pi/worker
./RaspberryIoT.Worker
```

## 🔄 Flusso di Funzionamento

```
1. Worker legge GPIO (simulato con Random)
    ↓
2. Se stato cambia:
    - Green → nessuna azione (normale)
    - Red → chiama Orchestrator.HandleErrorDetectedAsync()
    - Off → chiama Orchestrator.HandleRebootStartedAsync()
    ↓
3. Orchestrator scrive:
    - SensorStatus (nuovo record con stato)
    - SensorEvent (log dell'evento)
    ↓
4. DB aggiornato
    ↓
5. Power Automate polling rileva il cambio
```

## 📊 Simulazione GPIO

Al momento il Worker **simula** la lettura GPIO con valori random:
- **80%** → LED Green (normale)
- **15%** → LED Red (errore)
- **5%** → LED Off (reboot)

### Sostituire con GPIO Reale

Quando sarà il momento, sostituire il metodo `SimulateGpioRead()` con:

```csharp
using System.Device.Gpio;

private GpioController _gpio;
private const int LED_PIN = 17;

private LedColor ReadGpioPin()
{
    var value = _gpio.Read(LED_PIN);
    return value == PinValue.High ? LedColor.Green : LedColor.Red;
}
```

## 🐛 Log

Il Worker logga tutti i cambi di stato:

```
[INF] Worker started - SensorId: RASPBERRY-DEMO-001, Polling: 5000ms
[WRN] LED State Changed: Green -> Red
[ERR] Error Detected! Calling Orchestrator...
[INF] Error status and event created successfully
```

## 🔗 Integrazione con API

Worker e API condividono:
- ✅ Stesso database (SQLite)
- ✅ Stessi Services/Repositories
- ✅ Stesso Orchestrator
- ✅ Zero duplicazione logica

```
Worker → Orchestrator → DB ← API → Power Automate
```

## 📦 Dipendenze

- **RaspberryIoT.Application** (Services + Orchestrator)
- **RaspberryIoT.Infrastructure** (Repositories + DB)
- **Microsoft.Extensions.Hosting** (Worker Service framework)

## 🎯 TODO per Raspberry Pi

- [ ] Installare pacchetto GPIO: `System.Device.Gpio`
- [ ] Configurare pin GPIO corretto
- [ ] Sostituire `SimulateGpioRead()` con lettura reale
- [ ] Configurare come systemd service per auto-start
