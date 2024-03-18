namespace Solucao.Infrastructure.Shared.Models.Base;

using System.Collections.Generic;
using Solucao.Infrastructure.Shared.Interfaces;

public class ResultadoComando<T> : IResultadoComando<T>
{
    public T Resultado { get; private set; }
    public IList<INotificacaoErro> NotificacoesErro { get; private set; } = new List<INotificacaoErro>();
    public bool Sucesso => NotificacoesErro?.Count == 0;

    public ResultadoComando(T resultado, IList<INotificacaoErro> notificacoesErro)
    {
        Resultado = resultado;
        NotificacoesErro = notificacoesErro ?? new List<INotificacaoErro>();
    }

    public async Task GravarResultadoAsync(T t)
    {
        Resultado = t;

        await Task.CompletedTask;
    }

    public async Task GravarErroAsync(string erro)
    {
        NotificacoesErro.Add(new NotificacaoErro(erro));    
    
        await Task.CompletedTask;
    }
}