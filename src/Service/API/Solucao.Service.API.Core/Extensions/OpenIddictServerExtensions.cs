namespace Solucao.Service.API.Core.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreHandlerFilters;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreHandlers;
using static OpenIddict.Server.OpenIddictServerEvents;


public static class OpenIddictServerExtensions
{
    public static OpenIddictServerBuilder AdicionarManipuladoresDeSolicitacaoDeToken(this OpenIddictServerBuilder builder)
    {
        // Remove o manipulador de eventos integrado responsável por extrair solicitações de token padrão do OAuth 2.0
        // (que sempre usam codificação de URL de formulário) e substitua por um equivalente que suporta payloads JSON.
        builder.RemoveEventHandler(ExtractPostRequest<ExtractTokenRequestContext>.Descriptor);
        builder.AddEventHandler<ExtractTokenRequestContext>(builder =>
        {
            builder.UseInlineHandler(async context =>
            {
                var request = context.Transaction.GetHttpRequest() ?? throw new InvalidOperationException();

                //if (!HttpMethods.IsPost(request.Method) || string.IsNullOrEmpty(request.ContentType) ||
                //    !request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                //{
                //    context.Reject(Errors.InvalidRequest, "Requisição errada");
                //    return;
                //}

                // Habilite o buffering e refaça o corpo da solicitação após extrair o payload JSON para garantir que o
                // endpoint da API de Identidade do ASP.NET Core também possa resolver os parâmetros da solicitação.
                request.EnableBuffering();

                //try
                //{
                //    context.Request = await request.ReadFromJsonAsync<OpenIddictRequest>() ?? new();
                //}

                //finally
                //{
                //    request.Body.Position = 0L;
                //}

                // Ao contrário de uma implementação padrão do OAuth 2.0, o endpoint de login da API de Identidade do ASP.NET Core
                // não especifica o parâmetro grant_type. Como é o único método de autenticação suportado de qualquer maneira,
                // assume-se que todas as solicitações de token são solicitações de credenciais do proprietário do recurso (ROPC).
                //context.Request.GrantType = GrantTypes.Password;

                // A versão mais recente do pacote de API de Identidade do ASP.NET Core usa "email" em vez do
                // parâmetro de nome de usuário padrão do OAuth 2.0. Para contornar isso, o parâmetro de email é
                // mapeado manualmente para o parâmetro de nome de usuário padrão do OAuth 2.0.
                //context.Request.Username = (string)context.Request["email"];
            });

            builder.AddFilter<RequireHttpRequest>();
            builder.SetOrder(ExtractPostRequest<ExtractTokenRequestContext>.Descriptor.Order);
        });

        builder.AddEventHandler<ProcessSignInContext>(builder =>
        {
            builder.UseInlineHandler(context =>
            {
                Debug.Assert(context.Principal is not null);

                // O OpenIddict requer a especificação do claim "sub" padrão do OpenID Connect que identifica
                // o usuário. Como não é especificado pelo endpoint de login da API de Identidade do ASP.NET Core,
                // ele é mapeado manualmente do claim ClaimTypes.NameIdentifier que é adicionado pelo Identity.
                context.Principal.SetClaim(Claims.Subject, context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                //Permita que o OpenIddict armazene todos os claims gerados pela API de Identidade do ASP.NET Core nos tokens de acesso.
                context.Principal.SetDestinations(static claim => [Destinations.AccessToken]);

                return default;
            });

            builder.SetOrder(int.MinValue);
        });

        return builder;
    }
}

