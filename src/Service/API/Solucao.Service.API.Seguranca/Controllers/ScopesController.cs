namespace Solucao.Service.API.Seguranca.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Solucao.Infrastructure.Shared.Models;
using Solucao.Service.API.Seguranca.Core.Services;

[ApiExplorerSettings(GroupName = "Autorizacao")]
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScopesController : ControllerBase
{
    private readonly IEscopoService _escopoService;

    public ScopesController(IEscopoService escopoService)
    {
        _escopoService = escopoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetScopes(int draw, int start, int length, string search, string orderColumn, string orderDirection)
    {
        try
        {
            var scopes = await _escopoService.ObterTodosAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                scopes = scopes.Where(s =>
                    s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Obter o número total de registros antes da paginação
            var recordsTotal = scopes.Count();

            // Ordenar os escopos
            if (!string.IsNullOrWhiteSpace(orderColumn))
            {
                switch (orderColumn)
                {
                    case "0":
                        scopes = orderDirection == "asc" ?
                            scopes.OrderBy(s => s.Name) :
                            scopes.OrderByDescending(s => s.Name);
                        break;
                    case "1":
                        scopes = orderDirection == "asc" ?
                            scopes.OrderBy(s => s.DisplayName) :
                            scopes.OrderByDescending(s => s.DisplayName);
                        break;
                    default:
                        break;
                }
            }

            // Aplicar paginação
            scopes = scopes.Skip(start).Take(length);

            // Retornar os dados formatados para o DataTables
            return Ok(new
            {
                draw,
                recordsTotal,
                recordsFiltered = recordsTotal,
                data = scopes.ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetScope(Guid id)
    {
        try
        {
            var scope = await _escopoService.ObterPorIdAsync(id);
            if (scope == null)
                return NotFound();

            return Ok(new EscopoModel { /* Preencher com os dados do escopo */ });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostScope([FromBody] EscopoModel escopo)
    {
        try
        {
            var novoEscopoId = await _escopoService.CriarAsync(escopo);
            return Ok(new { EscopoId = novoEscopoId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutScope(Guid id, [FromBody] EscopoModel escopo)
    {
        try
        {
            if (id.Equals(default) || escopo is null)
            {
                throw new ArgumentException("O id ou o escopo informado é inválido.");
            }

            var existingScope = await _escopoService.ObterPorIdAsync(id);
            if (existingScope == null)
                return NotFound();

            // Atualizar os dados do escopo existente com os dados recebidos no corpo da requisição

            await _escopoService.SalvarAsync(existingScope, CancellationToken.None);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScope(Guid id)
    {
        try
        {
            await _escopoService.RemoverAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}
