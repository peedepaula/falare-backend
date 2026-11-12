using FalareBackend.DTOs;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatriculaController : ControllerBase
    {
        private readonly MatriculaService _matriculaService;

        public MatriculaController(MatriculaService matriculaService)
        {
            _matriculaService = matriculaService;
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("matricula")]
        public async Task<IActionResult> BuscarMatriculas()
        {
            return Ok(await _matriculaService.BuscarMatriculasService());
        }

        [Authorize(Roles = "Secretaria")]
        [HttpPost("matricula")]
        public async Task<IActionResult> CadastrarMatricula([FromBody] CadastrarMatriculaRequest request)
        {
            var (matricula, erro) = await _matriculaService.CadastrarMatriculaService(request);

            return erro switch
            {
                "aluno_nao_encontrado" => NotFound(new { mensagem = "Aluno não encontrado" }),
                "turma_nao_encontrada" => NotFound(new { mensagem = "Turma não encontrada" }),
                "turma_cheia" => Conflict(new { mensagem = "Turma sem vagas" }),
                "ja_matriculado" => Conflict(new { mensagem = "Aluno já matriculado nesta turma" }),
                "pendencia_financeira" => Conflict(new { mensagem = "Aluno com 2 ou mais mensalidades em aberto" }),
                "conflito_horario" => Conflict(new { mensagem = "Aluno já tem turma neste dia e horário" }),
                _ => StatusCode(201, matricula)
            };
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpPatch("matricula/{id:guid}")]
        public async Task<IActionResult> AtualizarMatricula(Guid id, [FromBody] AtualizarMatriculaRequest request)
        {
            var (matricula, erro) = await _matriculaService
                .AtualizarMatriculaService(id, request, User.IsInRole("Secretaria"));

            return erro switch
            {
                "nao_encontrada" => NotFound(new { mensagem = "Matrícula não encontrada" }),
                "matricula_certificada" => Conflict(new { mensagem = "Matrícula já certificada não pode ser alterada" }),
                "sem_permissao_status" => StatusCode(403, new { mensagem = "Apenas a secretaria altera o status" }),
                "transicao_invalida" => Conflict(new { mensagem = "Transição de status não permitida" }),
                "nao_aprovado" => Conflict(new { mensagem = "Aluno não atingiu os critérios de aprovação" }),
                "trancamento_fora_do_prazo" => Conflict(new { mensagem = "Trancamento só é permitido até a metade do semestre" }),
                "pendencia_financeira" => Conflict(new { mensagem = "Aluno com 2 ou mais mensalidades em aberto" }),
                _ => Ok(matricula)
            };
        }

        [Authorize(Roles = "Secretaria")]
        [HttpDelete("matricula/{id:guid}")]
        public async Task<IActionResult> DeletarMatricula(Guid id)
        {
            var apagou = await _matriculaService.DeletarMatriculaService(id);
            return apagou ? NoContent() : NotFound(new { mensagem = "Matrícula não encontrada" });
        }
    }
}