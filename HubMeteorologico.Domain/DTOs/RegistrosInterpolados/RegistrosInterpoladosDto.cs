
namespace HubMeteorologico.Domain.DTOs.RegistrosInterpolados;

public class RegistrosInterpoladosDto
{
    public DateTime DataHora { get; set; }
    public int FazendaId { get; set; }
    public int MapaFazendaId { get; set; }
    public int MapaFazendaLavouraId { get; set; }
    public int MapaFazendaLavouraInterpolacaoId { get; set; }
    public int AnoAgricolaId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool Consolidada { get; set; }
    public double VolumeChuva { get; set; }
    public double PressaoAtmosferica { get; set; }
    public double UmidadeRelativaAr { get; set; }
    public double Temperatura { get; set; }
    public double TemperaturaMaxima { get; set; }
    public double TemperaturaMinima { get; set; }
    public double DirecaoVento { get; set; }
    public double VelocidadeVento { get; set; }
    public double VelocidadeVentoPico { get; set; }
    public double PontoOrvalho { get; set; }
    public double FolhaMolhada { get; set; }
    public double RadiacaoSolar { get; set; }
    public double Evapotranspiracao { get; set; }
}