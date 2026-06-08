using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HubMeteorologico.Domain.Entities;

[Table("Equipamentos", Schema = "public")]
[Index("FazendaId", "Codigo", Name = "IX_Equipamentos_FazendaId_Codigo", IsUnique = true)]
[Index("FazendaId", "SedeFazendaId", "Codigo", Name = "IX_Equipamentos_FazendaId_SedeFazendaId_Codigo", IsUnique = true)]
[Index("SedeFazendaId", Name = "IX_Equipamentos_SedeFazendaId")]
public partial class Equipamentos
{
    [Key]
    public int Id { get; set; }

    public int FazendaId { get; set; }

    /// <summary>
    /// Estação Meteorológica = 1
    /// Pluviômetro = 2
    /// 
    /// </summary>
    public int TipoEquipamento { get; set; }

    public int FonteMeteorologica { get; set; }

    public string Codigo { get; set; } = null!;

    [StringLength(50)]
    public string Nome { get; set; } = null!;

    public string Modelo { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }

    [Column(TypeName = "geography")]
    public Geometry? Coordenada { get; set; }

    public bool Ativo { get; set; }

    public int SedeFazendaId { get; set; }
}
