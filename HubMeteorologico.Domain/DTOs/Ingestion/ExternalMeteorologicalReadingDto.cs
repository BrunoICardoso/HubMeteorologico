namespace HubMeteorologico.Domain.DTOs.Ingestion;

public class ExternalMeteorologicalReadingDto
{
    public int EquipamentoId { get; set; }
    public int FazendaId { get; set; }
    public DateTime DataHora { get; set; }
    public int AnoAgricolaId { get; set; }
    public bool Consolidada { get; set; }
    public double? PressaoAtmosferica { get; set; }
    public double? UmidadeRelativaAr { get; set; }
    public double VolumeChuva { get; set; }
    public double? Temperatura { get; set; }
    public double? DirecaoVento { get; set; }
    public double? VelocidadeVento { get; set; }
    public double? PontoOrvalho { get; set; }
    public double? Bateria { get; set; }
    public double? FolhaMolhada { get; set; }
    public string? Versao { get; set; }
    public double? RadiacaoSolar { get; set; }
    public double? TemperaturaMaxima { get; set; }
    public double? TemperaturaMinima { get; set; }
    public double? VelocidadeVentoPico { get; set; }
    public double? Evapotranspiracao { get; set; }
}
