using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HubMeteorologico.Domain.Entities;

[Table("Fazendas", Schema = "public")]
[Index("FusoHorarioId", Name = "IX_Fazendas_FusoHorarioId")]
public partial class Fazendas
{
    [Key]
    public int Id { get; set; }

    public int Codigo { get; set; }

    [StringLength(100)]
    public string Nome { get; set; } = null!;

    [StringLength(4)]
    public string Sigla { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }

    public string? IdExternoSolinftec { get; set; }

    public int? FusoHorarioId { get; set; }

    public bool MeteorologiaKhomp { get; set; }

    public bool MeteorologiaMetos { get; set; }

    public bool MeteorologiaSolinftec { get; set; }

    public bool MeteorologiaZeus { get; set; }
}
