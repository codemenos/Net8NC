namespace Solucao.Service.API.Seguranca.Coltrollers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Controlador base para operações comuns que tratam respostas de sucesso e erro.
/// </summary>
public class BaseController : ControllerBase
{
    private const string ResponseNotFound = "Recurso não encontrado.";
    private const string ResponseNotDefined = "Ocorreu um erro não tratado.";
    private const string ErroAoExecutarOComando = "Erro ao executar o comando.";
    private const string MensagemDeInicioDoComando = "Iniciando o comando: {0}";
    private const string MensagemDeRetornoDaExecucao = "Valor retornado: {0}";

    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public BaseController(IMediator mediator, ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    protected async Task<IActionResult> ExecutarComando<T>(Func<IRequest<ResultadoOperacao<T>>> comandoFactory)
    {
        var comando = comandoFactory();
        var comandoEmTexto = JsonConvert.SerializeObject(comando);

        _logger.LogInformation(MensagemDeInicioDoComando, comandoEmTexto);

        var resultadoDoComando = await _mediator.Send(comando);

        if (resultadoDoComando is ResultadoOperacao<T> resultado)
        {
            var resultadoEmTexto = JsonConvert.SerializeObject(resultado);

            _logger.LogInformation(MensagemDeRetornoDaExecucao, resultadoEmTexto);

            return TratarResposta(resultado);
        }
        else
        {
            _logger.LogError(ErroAoExecutarOComando);

            return BadRequest(new Resposta<string>(ErroAoExecutarOComando + Environment.NewLine + comandoEmTexto));
        }
    }

    protected async Task<IActionResult> ExecutarComando<T>(Func<IRequest<ResultadoConsulta<T>>> comandoFactory)
    {
        var comando = comandoFactory();

        var resultadoDoComando = await _mediator.Send(comando);

        if (resultadoDoComando is ResultadoConsulta<T> resultado)
        {
            return TratarResposta(resultado);
        }
        else
        {
            return BadRequest(new Resposta<string>(ErroAoExecutarOComando));
        }
    }

    /// <summary>
    /// Trata a resposta de uma operação, retornando uma resposta apropriada dependendo do resultado.
    /// </summary>
    /// <typeparam name="T">Tipo dos dados da operação.</typeparam>
    /// <param name="resultado">Resultado da operação.</param>
    /// <returns>ActionResult conforme o resultado da operação.</returns>
    protected IActionResult TratarResposta<T>(ResultadoOperacao<T> resultado)
    {
        return resultado.ComSucesso
            ? TratarSucesso(resultado.Dados)
            : TratarErro(resultado);
    }

    /// <summary>
    /// Trata a resposta de uma consulta, retornando uma resposta apropriada dependendo do resultado.
    /// </summary>
    /// <typeparam name="T">Tipo dos dados da consulta.</typeparam>
    /// <param name="resultado">Resultado da consulta.</param>
    /// <returns>ActionResult conforme o resultado da consulta.</returns>
    protected IActionResult TratarResposta<T>(ResultadoConsulta<T> resultado)
    {
        return resultado.ComSucesso
            ? TratarSucesso(resultado)
            : TratarErro(resultado);
    }

    private IActionResult TratarSucesso<T>(T dados)
    {
        _logger.LogInformation("Execução realizada com sucesso.");

        return Ok(new Resposta<T>(dados));
    }

    private IActionResult TratarErro<T>(ResultadoOperacao<T> resultado)
    {
        _logger.LogError("Operação realizada com erro.");

        return TratarErroComum(resultado.Dados, resultado.MensagemErro);
    }

    private IActionResult TratarErro<T>(ResultadoConsulta<T> resultado)
    {
        _logger.LogError("Consulta realizada com erro.");

        return TratarErroComum(resultado.Dados, resultado.MensagemErro);
    }

    private IActionResult TratarErroComum<T>(T dados, IList<string> mensagensErro)
    {
        if (mensagensErro?.Any() ?? false)
        {
            _logger.LogError("");

            return BadRequest(new Resposta<IList<string>>(mensagensErro));
        }

        if (dados is null)
        {
            _logger.LogError("");

            return NotFound(new Resposta<string>(ResponseNotFound));
        }

        _logger.LogError("");

        return BadRequest(new Resposta<string>(ResponseNotDefined));
    }
}
