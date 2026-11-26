using FalareBackend.DTOs;
using FalareBackend.Extensions;
using FalareBackend.Extensions;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> BuscarAlunos([FromQuery] bool apenasEmRisco = false)
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            return Ok(await _alunoService.BuscarAlunosService(professorId, apenasEmRisco));
        }

        [Authorize(Roles = "Aluno")]
        [HttpGet("aluno/status/inicio")]
        public async Task<IActionResult> AlunoStatusInicio()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(claim, out var alunoId))
                return Unauthorized();

            return Ok(await _alunoService.AlunoStatusInicioService(alunoId));
        }

        [Authorize(Roles = "Aluno")]
        [HttpGet("aluno/certificado")]
        public async Task<IActionResult> EmitirCertificado()
        {
            try
            {
                var (pdf, nomeArquivo) = await _alunoService.EmitirCertificadoService(User.UsuarioId());
                return File(pdf, "application/pdf", nomeArquivo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [Authorize(Roles = "Aluno")]
        [HttpPatch("aluno/trancar")]
        public async Task<IActionResult> AlternarTrancamentoMatricula()
        {
            try
            {
                var novoStatus = await _alunoService.AlternarTrancamentoMatriculaService(User.UsuarioId());
                return Ok(new { status = novoStatus.ToString() });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("alunos/por-turma/{turmaId:guid}")]
        public async Task<IActionResult> BuscarAlunosPorTurma(Guid turmaId)
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            var alunos = await _alunoService.BuscarAlunosPorTurmaService(turmaId, professorId);
            return Ok(alunos);
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("aluno/{id:guid}")]
        public async Task<IActionResult> BuscarAluno(Guid id)
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            var aluno = await _alunoService.BuscarAlunoService(id, professorId);
            return aluno == null
                ? NotFound(new { mensagem = "Aluno não encontrado" })
                : Ok(aluno);
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("aluno/completo/{id:guid}")]
        public async Task<IActionResult> BuscarAlunoCompleto(Guid id)
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            var aluno = await _alunoService.BuscarAlunoCompletoService(id, professorId);
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

        [Authorize(Roles = "Secretaria,Professor")]
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


        [Authorize(Roles = "Aluno")]
        [HttpGet("aluno/frequencia")]
        public async Task<IActionResult> FrequenciaAluno()
        {
            return Ok(await _alunoService.FrequenciaAlunoService(User.UsuarioId()));
        }

        [Authorize(Roles = "Aluno")]
        [HttpGet("aluno/nota")]
        public async Task<IActionResult> NotaAluno()
        {
            return Ok(await _alunoService.NotaAlunoService(User.UsuarioId()));
        }

        [Authorize(Roles = "Aluno")]
        [HttpGet("aluno/financeiro")]
        public async Task<IActionResult> FinanceiroAluno()
        {
            return Ok(await _alunoService.FinanceiroAlunoService(User.UsuarioId()));
        }

    }
}