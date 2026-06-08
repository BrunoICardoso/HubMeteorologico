using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HubMeteorologico.Domain.Entities;

[Table("MapaFazenda", Schema = "public")]
[Index("AnoAgricolaId", Name = "IX_MapaFazenda_AnoAgricolaId")]
[Index("FazendaId", "AnoAgricolaId", "SedeFazendaId", Name = "IX_MapaFazenda_FazendaId_AnoAgricolaId_SedeFazendaId", IsUnique = true)]
[Index("SedeFazendaId", Name = "IX_MapaFazenda_SedeFazendaId")]
public partial class MapaFazenda
{
    [Key]
    public int Id { get; set; }

    public int FazendaId { get; set; }

    public int AnoAgricolaId { get; set; }

    [Column(TypeName = "geography")]
    public Geometry Centroide { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }

    [Column(TypeName = "geography")]
    public Geometry Envelope { get; set; } = null!;

    [Column(TypeName = "geography")]
    public Geometry EnvelopeInterpolacao { get; set; } = null!;

    public int SedeFazendaId { get; set; }
}
