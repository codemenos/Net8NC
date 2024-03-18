namespace Solucao.Service.API.Seguranca.Core.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Infrastructure.Shared.Models;

public interface IEscopoService
{
    Task SalvarAsync(SecurityScope editado, CancellationToken cancellationToken);
    Task<string> CriarAsync(EscopoModel escopo);
    Task<SecurityScope> ObterPorNomeAsync(string nome);
    Task<SecurityScope> ObterPorIdAsync(Guid id);
    List<string> ObterStrings(string input);
    ValueTask<IEnumerable<EscopoModel>> ObterTodosAsync();
    Task RemoverAsync(Guid id);
}