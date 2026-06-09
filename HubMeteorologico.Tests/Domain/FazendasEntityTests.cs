using HubMeteorologico.Domain.Entities;
using Xunit;

namespace HubMeteorologico.Tests.Domain;

public class FazendasEntityTests
{
    [Fact]
    public void Fazenda_PropriedadesBasicas_AtribuidasCorretamente()
    {
        var fazenda = new Fazendas
        {
            Id = 1,
            Codigo = 100,
            Nome = "Fazenda Boa Vista",
            Sigla = "FBV",
            CreationTime = new DateTime(2024, 1, 1),
            MeteorologiaKhomp = true,
            MeteorologiaMetos = false,
            MeteorologiaSolinftec = true,
            MeteorologiaZeus = false
        };

        Assert.Equal(1, fazenda.Id);
        Assert.Equal(100, fazenda.Codigo);
        Assert.Equal("Fazenda Boa Vista", fazenda.Nome);
        Assert.Equal("FBV", fazenda.Sigla);
        Assert.True(fazenda.MeteorologiaKhomp);
        Assert.False(fazenda.MeteorologiaMetos);
    }

    [Fact]
    public void Fazenda_CamposOpcionais_PodemSerNulos()
    {
        var fazenda = new Fazendas
        {
            Id = 2,
            Codigo = 200,
            Nome = "Fazenda Nova",
            Sigla = "FNV",
            CreationTime = DateTime.UtcNow
        };

        Assert.Null(fazenda.FusoHorarioId);
        Assert.Null(fazenda.CreatorUsername);
        Assert.Null(fazenda.ModifierUsername);
        Assert.Null(fazenda.ModificationTime);
        Assert.Null(fazenda.IdExternoSolinftec);
    }
}
