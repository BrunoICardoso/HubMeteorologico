using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HubMeteorologico.Domain.Entities;

[Table("RegistrosMeteorologicos", Schema = "public")]
[PrimaryKey("DataHora", "FazendaId", "EquipamentoId")]
[Index("AnoAgricolaId", Name = "IX_RegistrosMeteorologicos_AnoAgricolaId")]
[Index("DataHora", "FazendaId", "AnoAgricolaId", Name = "IX_RegistrosMeteorologicos_DataHora_FazendaId_AnoAgricolaId")]
[Index("DataHora", "FazendaId", "AnoAgricolaId", "EquipamentoId", Name = "IX_RegistrosMeteorologicos_DataHora_FazendaId_AnoAgricolaId_Eq~")]
[Index("EquipamentoId", Name = "IX_RegistrosMeteorologicos_EquipamentoId")]
[Index("FazendaId", Name = "IX_RegistrosMeteorologicos_FazendaId")]
public partial class RegistrosMeteorologicos
{
    public int Id { get; set; }

    [Key]
    public DateTime DataHora { get; set; }

    [Key]
    public int FazendaId { get; set; }

    [Key]
    public int EquipamentoId { get; set; }

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

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }

    public int AnoAgricolaId { get; set; }

    public double? TemperaturaMaxima { get; set; }

    public double? TemperaturaMinima { get; set; }

    public double? VelocidadeVentoPico { get; set; }

    public double? Evapotranspiracao { get; set; }
}
