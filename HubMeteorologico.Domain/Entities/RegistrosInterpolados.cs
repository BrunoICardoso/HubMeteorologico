using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HubMeteorologico.Domain.Entities;

[Table("RegistrosInterpolados", Schema = "public")]
[PrimaryKey("DataHora", "FazendaId", "MapaFazendaId", "MapaFazendaLavouraId", "MapaFazendaLavouraInterpolacaoId")]
public partial class RegistrosInterpolados
{
    [Key]
    public DateTime DataHora { get; set; }

    [Key]
    public int FazendaId { get; set; }

    [Key]
    public int MapaFazendaId { get; set; }

    [Key]
    public int MapaFazendaLavouraId { get; set; }

    [Key]
    public int MapaFazendaLavouraInterpolacaoId { get; set; }

    public int AnoAgricolaId { get; set; }

    [Column(TypeName = "geography")]
    public Geometry Coordenada { get; set; } = null!;

    public bool Consolidada { get; set; }

    public double VolumeChuva { get; set; }

    public double PressaoAtmosferica { get; set; }

    public double UmidadeRelativaAr { get; set; }

    public double Temperatura { get; set; }

    public double DirecaoVento { get; set; }

    public double VelocidadeVento { get; set; }

    public double PontoOrvalho { get; set; }

    public double FolhaMolhada { get; set; }

    public double RadiacaoSolar { get; set; }

    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    public string? ModifierUsername { get; set; }

    public int? SedeFazendaId { get; set; }

    public double Evapotranspiracao { get; set; }

    public double TemperaturaMaxima { get; set; }

    public double TemperaturaMinima { get; set; }

    public double VelocidadeVentoPico { get; set; }
}
