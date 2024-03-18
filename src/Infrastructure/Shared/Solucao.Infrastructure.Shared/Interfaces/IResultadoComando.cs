namespace Solucao.Infrastructure.Shared.Interfaces;

public interface IResultadoComando<T>
{
    T Resultado { get; }
    IList<INotificacaoErro> NotificacoesErro { get; }
    bool Sucesso { get; }
    Task GravarResultadoAsync(T t);
    Task GravarErroAsync(string erro);
}

