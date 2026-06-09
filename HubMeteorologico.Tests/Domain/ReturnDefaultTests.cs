using System.Net;
using HubMeteorologico.Domain.ResponseDefault;

namespace HubMeteorologico.Tests.Domain;

public class ReturnDefaultTests
{
    [Fact]
    public void ReturnDefault_Padrao_StatusCodeOk()
    {
        var result = new ReturnDefault();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(string.Empty, result.Message);
    }

    [Fact]
    public void ReturnDefault_ComException_StatusCode500()
    {
        var ex = new InvalidOperationException("Erro interno");
        var result = new ReturnDefault(ex);

        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.False(result.IsSuccessStatusCode);
        Assert.Equal("Erro interno", result.Message);
    }

    [Fact]
    public void ReturnDefault_ComStatusCodeESemDados_StatusCodeCorreto()
    {
        var result = new ReturnDefault(HttpStatusCode.NotFound, "Recurso não encontrado");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(result.IsSuccessStatusCode);
        Assert.Equal("Recurso não encontrado", result.Message);
    }

    [Fact]
    public void ReturnDefaultGenerico_ComDados_RetornaDadosCorretos()
    {
        var dados = new List<string> { "item1", "item2" };
        var result = new ReturnDefault<List<string>>(dados);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(dados, result.Data);
    }

    [Fact]
    public void ReturnDefaultGenerico_ComException_DadosPadraoEStatus500()
    {
        var ex = new Exception("Falha inesperada");
        var result = new ReturnDefault<string>(ex);

        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Null(result.Data);
        Assert.Equal("Falha inesperada", result.Message);
    }

    [Fact]
    public void ReturnDefault_StatusCodeNoContent_IsNoContent()
    {
        var result = new ReturnDefault(HttpStatusCode.NoContent);

        Assert.True(result.IsNoContentStatusCode);
    }

    [Fact]
    public void ReturnDefaultGenerico_StatusCodeESemDados_MessageEStatusCorretos()
    {
        var result = new ReturnDefault<int>(HttpStatusCode.BadRequest, "ID inválido");

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("ID inválido", result.Message);
    }
}
