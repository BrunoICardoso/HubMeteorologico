namespace HubMeteorologico.Domain.DTOs.Ingestion;

public class EquipamentoIngestionDto
{
    public int Id { get; set; }
    public int FazendaId { get; set; }
    public int TipoEquipamento { get; set; }
    public int FonteMeteorologica { get; set; }
    public string Codigo { get; set; } = string.Empty;
}
