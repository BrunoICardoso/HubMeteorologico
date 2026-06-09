using HubMeteorologico.Domain.Entities;

namespace HubMeteorologico.Tests.Domain;

public class RegistrosMeteorologicosEntityTests
{
    [Fact]
    public void RegistroMeteorologico_PropriedadesNumericas_AtribuidasCorretamente()
    {
        var registro = new RegistrosMeteorologicos
        {
            DataHora = new DateTime(2024, 6, 1, 12, 0, 0),
            FazendaId = 1,
            EquipamentoId = 10,
            AnoAgricolaId = 5,
            Temperatura = 28.5,
            UmidadeRelativaAr = 70.0,
            VolumeChuva = 12.3,
            VelocidadeVento = 15.0,
            DirecaoVento = 180.0,
            PressaoAtmosferica = 1013.25,
            RadiacaoSolar = 500.0,
            Consolidada = true
        };

        Assert.Equal(28.5, registro.Temperatura);
        Assert.Equal(70.0, registro.UmidadeRelativaAr);
        Assert.Equal(12.3, registro.VolumeChuva);
        Assert.True(registro.Consolidada);
    }

    [Fact]
    public void RegistroMeteorologico_CamposOpcionais_PodemSerNulos()
    {
        var registro = new RegistrosMeteorologicos
        {
            DataHora = DateTime.UtcNow,
            FazendaId = 1,
            EquipamentoId = 1,
            AnoAgricolaId = 1,
            VolumeChuva = 0
        };

        Assert.Null(registro.Temperatura);
        Assert.Null(registro.UmidadeRelativaAr);
        Assert.Null(registro.PressaoAtmosferica);
        Assert.Null(registro.VelocidadeVento);
        Assert.Null(registro.DirecaoVento);
        Assert.Null(registro.PontoOrvalho);
        Assert.Null(registro.Bateria);
        Assert.Null(registro.FolhaMolhada);
        Assert.Null(registro.RadiacaoSolar);
        Assert.Null(registro.Evapotranspiracao);
        Assert.Null(registro.TemperaturaMaxima);
        Assert.Null(registro.TemperaturaMinima);
        Assert.Null(registro.VelocidadeVentoPico);
        Assert.Null(registro.Versao);
    }
}
