namespace RaspberryIoT.Contracts;

public static class ApiRoutes
{
    private const string Base = "api";
    
    public static class SensorStatus
    {
        private const string SensorStatusBase = $"{Base}/sensor/status";
        
        public const string GetAll = SensorStatusBase;
        public const string GetById = $"{SensorStatusBase}/{{id}}";
        public const string GetCurrent = $"{SensorStatusBase}/current/{{sensorId}}";
        public const string Poll = $"{Base}/trigger/sensor/status/poll";
        public const string Update = $"{SensorStatusBase}/{{id}}";
        public const string ForceError = $"{Base}/sensor/error";
        public const string ForceReboot = $"{Base}/sensor/reboot";
    }
    
    public static class SensorEvents
    {
        private const string SensorEventsBase = $"{Base}/sensor/events";
        
        public const string GetAll = SensorEventsBase;
        public const string GetById = $"{SensorEventsBase}/{{id}}";
        public const string Poll = $"{SensorEventsBase}/poll";
        public const string Create = SensorEventsBase;
    }
}
