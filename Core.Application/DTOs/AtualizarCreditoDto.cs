namespace Core.Application.DTOs;

/// <summary>
/// DTO para atualizar o crédito de um cliente
/// Contém apenas o score de crédito que será utilizado para calcular os limites
/// </summary>
public class AtualizarCreditoDto
{
    /// <summary>
    /// Score de crédito do cliente (0-1000)
    ///
    /// Regras aplicadas:
    /// - 0 a 100: Sem liberação de cartão de crédito
    /// - 101 a 500: Permitido liberação de cartão de crédito (limite R$ 1.000,00)
    /// - 501 a 1000: Permitido liberação de até 2 cartões (limite R$ 5.000,00 cada)
    /// </summary>
    public int ScoreCredito { get; set; }
}
