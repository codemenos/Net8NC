namespace Solucao.Infrastructure.Shared.Models.Base;

using Solucao.Infrastructure.Shared.Interfaces;

public class NotificacaoErro : INotificacaoErro
{
    public string Mensagem { get; }

    public NotificacaoErro(string mensagem)
    {
        Mensagem = mensagem;
    }
}
