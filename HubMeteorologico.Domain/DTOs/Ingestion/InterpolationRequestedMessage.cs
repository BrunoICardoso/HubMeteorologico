namespace HubMeteorologico.Domain.DTOs.Ingestion;

public class InterpolationRequestedMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public int FazendaId { get; set; }
    public DateTime DataHora { get; set; }
    public int ImportedReadings { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
}
