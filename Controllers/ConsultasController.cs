using GapsiMVC.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ConsultasController : ControllerBase
{
    private readonly ConsultaService _consultaService;

    public ConsultasController(ConsultaService consultaService)
    {
        _consultaService = consultaService;
    }

    [HttpPost("agendar")]
    public async Task<IActionResult> AgendarConsulta([FromBody] AgendarConsultaRequest request)
    {
        bool resultado = await _consultaService.AgendarConsulta(request.PacienteId, request.GrupoId, request.Data);
        if (!resultado) return BadRequest("Não foi possível agendar a consulta.");

        return Ok("Consulta agendada com sucesso!");
    }

    [HttpPost("cancelar")]
    public async Task<IActionResult> CancelarConsulta([FromBody] CancelarConsultaRequest request)
    {
        bool resultado = await _consultaService.CancelarConsulta(request.UsuarioId, request.ConsultaId);
        if (!resultado) return BadRequest("Não foi possível cancelar a consulta.");

        return Ok("Consulta cancelada com sucesso!");
    }
}

public class AgendarConsultaRequest
{
    public string PacienteId { get; set; }
    public int GrupoId { get; set; }
    public DateTime Data { get; set; }
}

public class CancelarConsultaRequest
{
    public string UsuarioId { get; set; }
    public int ConsultaId { get; set; }
}
