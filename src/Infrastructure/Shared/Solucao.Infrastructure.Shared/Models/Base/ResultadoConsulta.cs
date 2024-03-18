namespace Solucao.Infrastructure.Shared.Models.Base;

using Solucao.Infrastructure.Shared.Interfaces;

public class ResultadoConsulta<T> : IResultadoConsulta<T>
{
    public T Dados { get; private set; }
    public IList<INotificacaoErro> NotificacoesErro { get; private set; } = new List<INotificacaoErro>();
    public bool Sucesso => NotificacoesErro?.Count == 0;

    public ResultadoConsulta(T dados, IList<INotificacaoErro> notificacoesErro)
    {
        Dados = dados;
        NotificacoesErro = notificacoesErro ?? new List<INotificacaoErro>();
    }

    public async Task GravarResultadoAsync(T t)
    {
        Dados = t;

        await Task.CompletedTask;
    }

    public async Task GravarErroAsync(string erro)
    {
        NotificacoesErro.Add(new NotificacaoErro(erro));

        await Task.CompletedTask;
    }
}