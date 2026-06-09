namespace HubMeteorologico.Domain.DTOs.RegistrosInterpolados;

public class RegistrosInterpoladosFilterDto
{
    public int FazendaId { get; set; }
    public string? CodigoLavoura { get; set; }
    public DateTime DataHora { get; set; }
}
