using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HubMeteorologico.Domain.Entities;

[Table("FusoHorarios", Schema = "public")]
public partial class FusoHorarios
{
    [Key]
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public TimeSpan Offset { get; set; }

    public string Localidade { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }
}
