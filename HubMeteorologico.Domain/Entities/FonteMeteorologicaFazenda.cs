using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HubMeteorologico.Domain.Entities;

[Table("FonteMeteorologicaFazenda", Schema = "public")]
[Index("FazendaId", "FonteMeteorologicaId", Name = "IX_FonteMeteorologicaFazenda_FazendaId_FonteMeteorologicaId", IsUnique = true)]
[Index("FonteMeteorologicaId", Name = "IX_FonteMeteorologicaFazenda_FonteMeteorologicaId")]
public partial class FonteMeteorologicaFazenda
{
    [Key]
    public int Id { get; set; }

    public int FazendaId { get; set; }

    public int FonteMeteorologicaId { get; set; }

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }
}
