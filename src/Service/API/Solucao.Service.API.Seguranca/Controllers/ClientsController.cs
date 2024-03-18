namespace Solucao.Service.API.Seguranca.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Solucao.Infrastructure.Shared.Models;
using Solucao.Service.API.Seguranca.Core.Services;

[ApiExplorerSettings(GroupName = "Autorizacao")]
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IAplicativoClienteService _aplicativoClienteService;

    public ClientsController(IAplicativoClienteService aplicativoClienteService)
    {
        _aplicativoClienteService = aplicativoClienteService;
    }

    // GET: api/clients
    [HttpGet]
    public async Task<IActionResult> GetClients(int draw, int start, int length, string search, string orderColumn, string orderDirection)
    {
        try
        {
            var aplicacoesCliente = (await _aplicativoClienteService.ObterTodosOsClientesAsync());
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                aplicacoesCliente = aplicacoesCliente.Where(c =>
                    c.ClienteId.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.NomeExibicao.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Obter o número total de registros antes da paginação
            var recordsTotal = aplicacoesCliente.ToList().Count;

            // Ordenar os clientes
            if (!string.IsNullOrWhiteSpace(orderColumn))
            {
                switch (orderColumn)
                {
                    case "0":
                        aplicacoesCliente = orderDirection == "asc" ?
                            aplicacoesCliente.OrderBy(c => c.ClienteId) :
                            aplicacoesCliente.OrderByDescending(c => c.ClienteId);
                        break;
                    case "1":
                        aplicacoesCliente = orderDirection == "asc" ?
                            aplicacoesCliente.OrderBy(c => c.NomeExibicao) :
                            aplicacoesCliente.OrderByDescending(c => c.NomeExibicao);
                        break;
                    default:
                        break;
                }
            }

            // Aplicar paginação
            aplicacoesCliente = aplicacoesCliente.Skip(start).Take(length);

            // Retornar os dados formatados para o DataTables
            return Ok(new
            {
                draw,
                recordsTotal,
                recordsFiltered = recordsTotal,
                data = aplicacoesCliente.ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    // GET: api/clients/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(Guid id)
    {
        try
        {
            var client = await _aplicativoClienteService.ObterClientePorIdAsync(id);
            if (client == null)
                return NotFound();

            return Ok(new ClienteModel { ClienteId = client.ClientId, NomeExibicao = client.DisplayName });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    // POST: api/clients
    [HttpPost]
    public async Task<IActionResult> PostClient([FromBody] ClienteModel cliente)
    {
        await Task.Delay(0);

        try
        {
            //var novoClienteId = await _aplicativoClienteService.CriarClienteAsync(new ClienteModel() 
            //{ 
            //    ClienteId = Guid.NewGuid().ToString(), 
            //    NomeExibicao = cliente.NomeExibicao, 
            //    SegredoCliente = Guid.NewGuid().ToString() 
            //});

            var novoClienteId = Guid.NewGuid().ToString();

            return Ok(new { ClienteId = novoClienteId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    // PUT: api/clients/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutClient(Guid id, [FromBody] ClienteModel cliente)
    {
        try
        {
            var existingClient = await _aplicativoClienteService.ObterClientePorIdAsync(id);
            if (existingClient == null)
                return NotFound();

            existingClient.ClientId = cliente.ClienteId;
            existingClient.DisplayName = cliente.NomeExibicao;

            await _aplicativoClienteService.SalvarClienteAsync(existingClient, CancellationToken.None);
            
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    // DELETE: api/clients/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        try
        {
            await _aplicativoClienteService.RemoverClienteAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}
