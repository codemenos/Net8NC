namespace Solucao.Service.API.Core.Services;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Solucao.Domain.Seguranca.Aggregates;

public class EmailSenderService : IEmailSender<SecurityUser>
{
    public async Task SendConfirmationLinkAsync(SecurityUser user, string email, string confirmationLink)
    {
        // Implemente o envio de e-mail de confirmação aqui
        // Por exemplo, você pode usar um serviço de e-mail como SendGrid, SMTP, etc.
        // Aqui está um exemplo de implementação fictícia:
        Console.WriteLine($"Sending confirmation link to {email} for user {user.UserName}. Link: {confirmationLink}");
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetCodeAsync(SecurityUser user, string email, string resetCode)
    {
        // Implemente o envio de e-mail com o código de redefinição de senha aqui
        // Por exemplo, você pode usar um serviço de e-mail como SendGrid, SMTP, etc.
        // Aqui está um exemplo de implementação fictícia:
        Console.WriteLine($"Sending password reset code to {email} for user {user.UserName}. Code: {resetCode}");
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetLinkAsync(SecurityUser user, string email, string resetLink)
    {
        // Implemente o envio de e-mail com o link de redefinição de senha aqui
        // Por exemplo, você pode usar um serviço de e-mail como SendGrid, SMTP, etc.
        // Aqui está um exemplo de implementação fictícia:
        Console.WriteLine($"Sending password reset link to {email} for user {user.UserName}. Link: {resetLink}");
        await Task.CompletedTask;
    }
}
