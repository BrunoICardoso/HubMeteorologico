using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HubMeteorologico.Domain.Entities;

[Table("FonteMeteorologica", Schema = "public")]
public partial class FonteMeteorologica
{
    [Key]
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public int TipoFonteMeteorologica { get; set; }

    public string Usuario { get; set; } = null!;

    public string Senha { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }
}
