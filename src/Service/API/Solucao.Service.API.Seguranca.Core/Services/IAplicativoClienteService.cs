namespace Solucao.Service.API.Seguranca.Core.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Models;

public interface IAplicativoClienteService
{
    Task<SecurityApplication> ObterClientePorClientIdAsync(string clientid);
    Task<SecurityApplication> ObterClientePorIdAsync(Guid id);
    List<string> ObterStrings(string input);
    ValueTask<IEnumerable<ClienteModel>> ObterTodosOsClientesAsync();
    Task SalvarClienteAsync(SecurityApplication editado, CancellationToken cancellationToken);
    Task RemoverClienteAsync(Guid id);
}