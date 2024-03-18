namespace Solucao.Application.Seguranca.Comandos.AplicacaoCliente;

using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Interfaces;
using Solucao.Infrastructure.Shared.Models;
using Solucao.Infrastructure.Shared.Models.Base;
using Solucao.Infrastructure.Shared.Models.Module.Seguranca;

public class ComandoSalvarAplicacaoCliente : IRequest<IResultadoComando<string>>
{
    public ISecurityApplication NovoCliente { get; }

    public ComandoSalvarAplicacaoCliente(ISecurityApplication novoCliente)
    {
        NovoCliente = novoCliente;
    }
}

public class ManipuladorSalvarAplicacaoCliente : IRequestHandler<ComandoSalvarAplicacaoCliente, IResultadoComando<string>>
{
    private readonly IMapper _mapper;
    public ManipuladorSalvarAplicacaoCliente(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<IResultadoComando<string>> Handle(ComandoSalvarAplicacaoCliente request, CancellationToken cancellationToken)
    {
        var resultadoComando = new ResultadoComando<string>(default, null);

        try
        {
            var cli = _mapper.Map<SecurityApplication>(request.NovoCliente as ClienteModel);

            var clienteId = request.NovoCliente.ClientId;

            if (request.NovoCliente == null)
            {
                resultadoComando.NotificacoesErro.Add(new NotificacaoErro("Cliente não pode ser nulo."));
            }

            await resultadoComando.GravarResultadoAsync(clienteId);

            return resultadoComando;
        }
        catch (Exception ex)
        {
            await resultadoComando.GravarErroAsync($"Erro ao processar comando: {ex.Message}");

            return resultadoComando;
        }
    }
}