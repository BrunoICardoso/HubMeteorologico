using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HubMeteorologico.Domain.Entities;

[Table("MapaFazendaLavouraInterpolacao", Schema = "public")]
[Index("MapaFazendaLavouraId", Name = "IX_MapaFazendaLavouraInterpolacao_MapaFazendaLavouraId")]
public partial class MapaFazendaLavouraInterpolacao
{
    [Key]
    public int Id { get; set; }

    public int MapaFazendaLavouraId { get; set; }

    [Column(TypeName = "geography")]
    public Geometry Poligono { get; set; } = null!;

    [Column(TypeName = "geography")]
    public Geometry Centroide { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }
}
