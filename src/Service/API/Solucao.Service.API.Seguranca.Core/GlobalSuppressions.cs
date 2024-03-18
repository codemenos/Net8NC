﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "SYSLIB1045:Converter em 'GeneratedRegexAttribute'.", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AplicativoClienteService.ObterStrings(System.String)~System.Collections.Generic.List{System.String}")]
[assembly: SuppressMessage("Performance", "SYSLIB1045:Converter em 'GeneratedRegexAttribute'.", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.EscopoService.ObterStrings(System.String)~System.Collections.Generic.List{System.String}")]
[assembly: SuppressMessage("Reliability", "CA2016:Encaminhe o parâmetro 'CancellationToken' para os métodos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AplicativoClienteService.SalvarClienteAsync(Solucao.Domain.Seguranca.Aggregates.SecurityApplication,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Style", "IDE0290:Usar construtor primário", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AplicativoClienteService.#ctor(OpenIddict.Abstractions.IOpenIddictApplicationManager)")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AplicativoClienteService.Modificar(System.Object,Solucao.Domain.Seguranca.Aggregates.SecurityApplication,System.Threading.CancellationToken)~Solucao.Domain.Seguranca.Aggregates.SecurityApplication")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.EncerrarSessao(Microsoft.AspNetCore.Mvc.ControllerBase)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.IActionResult}")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.ConstruirUrlRedirecionamento(Microsoft.AspNetCore.Http.HttpRequest,System.Collections.Generic.IDictionary{System.String,Microsoft.Extensions.Primitives.StringValues})~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.CriarPropriedadesDeAutenticacao(System.String,System.String)~Microsoft.AspNetCore.Authentication.AuthenticationProperties")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.ObterObjetivoDaRequisicao(OpenIddict.Abstractions.OpenIddictRequest)~System.ValueTuple{System.Boolean,System.Boolean}")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.ObterParametrosDaConsulta(Microsoft.AspNetCore.Http.HttpContext,System.Collections.Generic.List{System.String})~System.Collections.Generic.Dictionary{System.String,Microsoft.Extensions.Primitives.StringValues}")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.ObterParametrosDoFormulario(Microsoft.AspNetCore.Http.HttpContext,System.Collections.Generic.List{System.String})~System.Collections.Generic.Dictionary{System.String,Microsoft.Extensions.Primitives.StringValues}")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.ObterValorReivindicacaoUsuario(System.Security.Claims.ClaimsPrincipal,System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.AutorizacaoService.RetornarErroDeConsentimento(Microsoft.AspNetCore.Mvc.ControllerBase,System.String)~Microsoft.AspNetCore.Mvc.IActionResult")]
[assembly: SuppressMessage("Style", "IDE0290:Usar construtor primário", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.CargaPadraoOpenIddictService.#ctor(System.IServiceProvider)")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.CargaPadraoOpenIddictService.CriarOuAtualizarCliente(OpenIddict.Abstractions.IOpenIddictApplicationManager,System.String,System.String,System.String,System.String,System.String,System.Collections.Generic.List{System.String},System.Boolean)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.CargaPadraoOpenIddictService.CriarOuAtualizarEscopo(OpenIddict.Abstractions.IOpenIddictScopeManager,System.String,System.String,System.Collections.Generic.List{System.String},System.Boolean)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Style", "IDE0290:Usar construtor primário", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.EscopoService.#ctor(OpenIddict.Abstractions.IOpenIddictScopeManager)")]
[assembly: SuppressMessage("Performance", "CA1822:Marcar membros como estáticos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.EscopoService.Modificar(System.Object,Solucao.Domain.Seguranca.Entities.SecurityScope,System.Threading.CancellationToken)~Solucao.Domain.Seguranca.Entities.SecurityScope")]
[assembly: SuppressMessage("Reliability", "CA2016:Encaminhe o parâmetro 'CancellationToken' para os métodos", Justification = "<Pendente>", Scope = "member", Target = "~M:Solucao.Service.API.Seguranca.Core.Services.EscopoService.SalvarAsync(Solucao.Domain.Seguranca.Entities.SecurityScope,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]