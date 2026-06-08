using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HubMeteorologico.Domain.Entities;

[Table("AnosAgricolas", Schema = "public")]
public partial class AnosAgricolas
{
    [Key]
    public int Id { get; set; }

    public int Codigo { get; set; }

    [StringLength(50)]
    public string Nome { get; set; } = null!;

    [StringLength(100)]
    public string? CreatorUsername { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? ModificationTime { get; set; }

    [StringLength(100)]
    public string? ModifierUsername { get; set; }

    public DateTime DataFinal { get; set; }

    public DateTime DataInicial { get; set; }
}
