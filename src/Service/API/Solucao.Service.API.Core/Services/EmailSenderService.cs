namespace Solucao.Service.API.Core.Services;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Solucao.Domain.Seguranca.Aggregates;

public class EmailSenderService : IEmailSender<SecurityUser>
{
    public async Task SendConfirmationLinkAsync(SecurityUser user, string email, string confirmationLink)
    {
        // falta implementar o envio de e-mail de confirmação aqui
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetCodeAsync(SecurityUser user, string email, string resetCode)
    {
        // falta implementar o envio de e-mail com o código de redefinição de senha aqui
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetLinkAsync(SecurityUser user, string email, string resetLink)
    {
        // falta implementar o envio de e-mail com o link de redefinição de senha aqui
        await Task.CompletedTask;
    }
}
