using System.Security.Claims;
using FalareBackend.DTOs;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChamadaController : ControllerBase
    {
        private readonly ChamadaService _chamadaService;
        public ChamadaController(ChamadaService chamadaService) => _chamadaService = chamadaService;

        private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("chamada")]
        public async Task<IActionResult> BuscarChamada([FromQuery] Guid turmaId, [FromQuery] DateTime data)
        {
            var (chamada, erro) = await _chamadaService
                .BuscarChamadaService(turmaId, data, UsuarioId, User.IsInRole("Secretaria"));
            return Mapear(chamada, erro);
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpPost("chamada")]
        public async Task<IActionResult> LancarChamada([FromBody] LancarChamadaRequest request)
        {
            var (chamada, erro) = await _chamadaService
                .LancarChamadaService(request, UsuarioId, User.IsInRole("Secretaria"));
            return Mapear(chamada, erro);
        }

        private IActionResult Mapear(ChamadaResponse? chamada, string? erro) => erro switch
        {
            "turma_nao_encontrada" => NotFound(new { mensagem = "Turma não encontrada" }),
            "sem_permissao" => StatusCode(403, new { mensagem = "Esta turma pertence a outro professor" }),
            "data_futura" => BadRequest(new { mensagem = "Não é possível lançar presença de aula com data futura" }),
            "dia_fora_da_grade" => BadRequest(new { mensagem = "A turma não tem aula neste dia da semana" }),
            "item_duplicado" => BadRequest(new { mensagem = "Matrícula repetida na chamada" }),
            "matricula_invalida" => BadRequest(new { mensagem = "Há matrícula que não pertence a esta turma" }),
            "chamada_incompleta" => BadRequest(new { mensagem = "A chamada precisa incluir todos os alunos da turma" }),
            "limite_de_aulas" => Conflict(new { mensagem = "A turma já tem as 32 aulas do semestre" }),
            _ => Ok(chamada)
        };
    }
}