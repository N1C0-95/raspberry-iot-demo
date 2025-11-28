using System.Device.Gpio;
using RaspberryIoT.Application.Database;
using RaspberryIoT.Application.Repositories;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Infrastructure.Database;
using RaspberryIoT.Infrastructure.Repositories;
using RaspberryIoT.Worker;
using RaspberryIoT.Worker.Hardware;

var builder = Host.CreateApplicationBuilder(args);

// Database Configuration
var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString")!;
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
builder.Services.AddSingleton<IDbInitializer, DbInitializer>();

// Repositories
builder.Services.AddSingleton<ISensorStatusRepository, SensorStatusRepository>();
builder.Services.AddSingleton<ISensorEventRepository, SensorEventRepository>();

// Orchestrator
builder.Services.AddSingleton<ISensorOrchestrator, SensorOrchestrator>();

// GPIO Hardware Controllers
builder.Services.AddSingleton<GpioController>();
builder.Services.AddSingleton<LedController>();
builder.Services.AddSingleton<BuzzerController>();
builder.Services.AddSingleton<ButtonMonitor>();

// Worker Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Initialize Database
var dbInitializer = host.Services.GetRequiredService<IDbInitializer>();
await dbInitializer.InitializeAsync();

host.Run();
