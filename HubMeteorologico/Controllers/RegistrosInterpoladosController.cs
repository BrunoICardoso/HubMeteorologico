using HubMeteorologico.API.ConfigController;
using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.Interfaces;
using HubMeteorologico.Domain.ResponseDefault;
using Microsoft.AspNetCore.Mvc;

namespace HubMeteorologico.API.Controllers;

[ApiController]
[Route("registros-interpolados")]
public class RegistrosInterpoladosController : ControllerBaseCustom
{
    private readonly IRegistrosInterpoladosService _service;

    public RegistrosInterpoladosController(IRegistrosInterpoladosService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna todos os registros interpolados para uma fazenda, lavoura e hora cheia.
    /// </summary>
    /// <param name="fazendaId">Id da fazenda</param>
    /// <param name="codigoLavoura">Código da lavoura (opcional — omitir retorna todas as lavouras)</param>
    /// <param name="dataHora">Data/hora cheia no formato ISO 8601 (ex: 2024-01-15T12:00:00Z)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnDefault<IReadOnlyCollection<RegistrosInterpoladosDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnDefault), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReturnDefault<IReadOnlyCollection<RegistrosInterpoladosDto>>>> Get(
        [FromQuery] RegistrosInterpoladosFilterDto filter,
        CancellationToken cancellationToken)
    {
        return StatusCode(await _service.GetAsync(filter, cancellationToken));
    }
}