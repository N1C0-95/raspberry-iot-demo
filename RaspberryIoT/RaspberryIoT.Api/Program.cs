using RaspberryIoT.Application.Database;
using RaspberryIoT.Application.Repositories;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Infrastructure.Database;
using RaspberryIoT.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString")!;
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
builder.Services.AddSingleton<IDbInitializer, DbInitializer>();

// Repositories
builder.Services.AddSingleton<ISensorStatusRepository, SensorStatusRepository>();
builder.Services.AddSingleton<ISensorEventRepository, SensorEventRepository>();

// Services
builder.Services.AddSingleton<ISensorStatusService, SensorStatusService>();
builder.Services.AddSingleton<ISensorEventService, SensorEventService>();

// Orchestrator
builder.Services.AddSingleton<ISensorOrchestrator, SensorOrchestrator>();

var app = builder.Build();

// Database initialization is handled by Worker Service
// API only reads/writes to the existing database

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();