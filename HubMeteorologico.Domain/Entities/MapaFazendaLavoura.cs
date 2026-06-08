using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HubMeteorologico.Domain.Entities;

[Table("MapaFazendaLavoura", Schema = "public")]
[Index("MapaFazendaId", Name = "IX_MapaFazendaLavoura_MapaFazendaId")]
public partial class MapaFazendaLavoura
{
    [Key]
    public int Id { get; set; }

    public int MapaFazendaId { get; set; }

    public string CodigoLavoura { get; set; } = null!;

    public float Area { get; set; }

    [Column(TypeName = "geography")]
    public Geometry Poligono { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }
}
