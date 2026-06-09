namespace HubMeteorologico.Domain.Appsettings;

public class IngestionWorkerOptions
{
    public bool Enabled { get; set; }
    public int IntervalMinutes { get; set; } = 5;
    public int LookbackHours { get; set; } = 1;
}
