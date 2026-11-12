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
                _ => StatusCode(201, matricula)
            };
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpPatch("matricula/{id:guid}")]
        public async Task<IActionResult> AtualizarMatricula(Guid id, [FromBody] AtualizarMatriculaRequest request)
        {
            var (matricula, erro) = await _matriculaService.AtualizarMatriculaService(id, request);

            return erro switch
            {
                "nao_encontrada" => NotFound(new { mensagem = "Matrícula não encontrada" }),
                "turma_cheia" => Conflict(new { mensagem = "Turma sem vagas" }),
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