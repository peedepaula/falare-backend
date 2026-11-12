using FalareBackend.DTOs;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlunoController : ControllerBase
    {
        private readonly AlunoService _alunoService;

        public AlunoController(AlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("alunos")]
        public async Task<IActionResult> BuscarAlunos()
        {
            return Ok(await _alunoService.BuscarAlunosService());
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("aluno/{id:guid}")]
        public async Task<IActionResult> BuscarAluno(Guid id)
        {
            var aluno = await _alunoService.BuscarAlunoService(id);
            return aluno == null
                ? NotFound(new { mensagem = "Aluno não encontrado" })
                : Ok(aluno);
        }

        [Authorize(Roles = "Secretaria")]
        [HttpPost("aluno")]
        public async Task<IActionResult> CadastrarAluno([FromBody] CadastrarAlunoRequest request)
        {
            var (aluno, erro) = await _alunoService.CadastrarAlunoService(request);

            return erro == "email_em_uso"
                ? Conflict(new { mensagem = "E-mail já cadastrado" })
                : StatusCode(201, aluno);
        }

        [Authorize(Roles = "Secretaria")]
        [HttpPatch("aluno/{id:guid}")]
        public async Task<IActionResult> AtualizarAluno(Guid id, [FromBody] AtualizarAlunoRequest request)
        {
            var (aluno, erro) = await _alunoService.AtualizarAlunoService(id, request);

            return erro switch
            {
                "nao_encontrado" => NotFound(new { mensagem = "Aluno não encontrado" }),
                "email_em_uso" => Conflict(new { mensagem = "E-mail já cadastrado" }),
                _ => Ok(aluno)
            };
        }

        [Authorize(Roles = "Secretaria")]
        [HttpDelete("aluno/{id:guid}")]
        public async Task<IActionResult> DeletarAluno(Guid id)
        {
            var apagou = await _alunoService.DeletarAlunoService(id);
            return apagou ? NoContent() : NotFound(new { mensagem = "Aluno não encontrado" });
        }
    }
}