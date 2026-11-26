using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Secretaria")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsService _analytics;

        public AnalyticsController(AnalyticsService analytics)
        {
            _analytics = analytics;
        }

        [HttpGet("aluno")]
        public async Task<IActionResult> Aluno() => Ok(await _analytics.AlunoAsync());

        [HttpGet("geral")]
        public async Task<IActionResult> Geral() => Ok(await _analytics.GeralAsync());

        [HttpGet("aula")]
        public async Task<IActionResult> Aula() => Ok(await _analytics.AulaAsync());

        [HttpGet("turma")]
        public async Task<IActionResult> Turma() => Ok(await _analytics.TurmaAsync());

        [HttpGet("pagamento")]
        public async Task<IActionResult> Pagamento() => Ok(await _analytics.PagamentoAsync());

        [HttpGet("matricula")]
        public async Task<IActionResult> Matricula() => Ok(await _analytics.MatriculaAsync());

        [HttpGet("professor")]
        public async Task<IActionResult> Professor() => Ok(await _analytics.ProfessorAsync());
    }
}