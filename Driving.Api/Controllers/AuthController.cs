using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Application.Interfaces.Services;

namespace Driving.Api.Controllers;

/// <summary>
/// Controller para autenticação de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Construtor do controller
    /// </summary>
    /// <param name="authenticationService">Serviço de autenticação injetado por DI</param>
    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Realiza login do usuário e retorna um token JWT
    /// </summary>
    /// <remarks>
    /// Credenciais padrão para desenvolvimento:
    /// - Usuário: "user"
    /// - Senha: "password"
    /// </remarks>
    /// <param name="login">Dados de login (usuário e senha)</param>
    /// <returns>Token JWT para uso nas requisições subsequentes</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Credenciais inválidas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(new ApiResponseDto<LoginResponseDto>
            {
                Sucesso = false,
                Mensagem = "Dados de login inválidos",
                Erros = errors
            });
        }

        var resultado = await _authenticationService.AutenticarAsync(login);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

}
