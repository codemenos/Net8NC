namespace Solucao.Infrastructure.Shared.Interfaces;

public interface IResultadoConsulta<T>
{
    T Dados { get; }
    IList<INotificacaoErro> NotificacoesErro { get; }
    bool Sucesso { get; }
    Task GravarResultadoAsync(T t);
    Task GravarErroAsync(string erro);
}