using FalareBackend.DTOs;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Secretaria")]
    public class PagamentoController : ControllerBase
    {
        private readonly PagamentoService _pagamentoService;

        public PagamentoController(PagamentoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        [HttpGet("pagamentos")]
        public async Task<IActionResult> BuscarPagamentos([FromQuery] Guid? alunoId)
        {
            return Ok(await _pagamentoService.BuscarPagamentosService(alunoId));
        }

        [HttpGet("pagamento/{id:guid}")]
        public async Task<IActionResult> BuscarPagamento(Guid id)
        {
            var pagamento = await _pagamentoService.BuscarPagamentoService(id);
            return pagamento == null
                ? NotFound(new { mensagem = "Pagamento não encontrado" })
                : Ok(pagamento);
        }

        [HttpPost("pagamento")]
        public async Task<IActionResult> CadastrarPagamento([FromBody] CadastrarPagamentoRequest request)
        {
            var (pagamento, erro) = await _pagamentoService.CadastrarPagamentoService(request);

            return erro == "aluno_nao_encontrado"
                ? NotFound(new { mensagem = "Aluno não encontrado" })
                : StatusCode(201, pagamento);
        }

        [HttpPatch("pagamento/{id:guid}")]
        public async Task<IActionResult> AtualizarPagamento(Guid id, [FromBody] AtualizarPagamentoRequest request)
        {
            var pagamento = await _pagamentoService.AtualizarPagamentoService(id, request);
            return pagamento == null
                ? NotFound(new { mensagem = "Pagamento não encontrado" })
                : Ok(pagamento);
        }

        [HttpDelete("pagamento/{id:guid}")]
        public async Task<IActionResult> DeletarPagamento(Guid id)
        {
            var apagou = await _pagamentoService.DeletarPagamentoService(id);
            return apagou ? NoContent() : NotFound(new { mensagem = "Pagamento não encontrado" });
        }
    }
}