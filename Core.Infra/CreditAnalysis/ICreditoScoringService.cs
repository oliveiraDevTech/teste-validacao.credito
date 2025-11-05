namespace Core.Infra;

/// <summary>
/// Interface para serviço de scoring de crédito
/// Fornece cálculos e análises de elegibilidade para cartão de crédito
/// </summary>
public interface ICreditoScoringService
{
    /// <summary>
    /// Calcula o score de crédito baseado em dados do cliente
    /// Score varia de 0 a 1000
    /// </summary>
    /// <param name="renda">Renda mensal em reais</param>
    /// <param name="historico">Histórico de crédito: "BOM", "REGULAR", "RUIM"</param>
    /// <param name="idade">Idade do cliente</param>
    /// <param name="dataNascimento">Data de nascimento</param>
    /// <param name="cpf">CPF do cliente (para análise adicional)</param>
    /// <returns>Score calculado entre 0 e 1000</returns>
    int CalcularScore(
        decimal renda,
        string historico,
        int idade,
        DateTime dataNascimento,
        string cpf);

    /// <summary>
    /// Calcula o ranking de crédito baseado no score
    /// Ranking varia de 1 (pior) a 5 (melhor)
    /// </summary>
    /// <param name="scoreCredito">Score de crédito (0-1000)</param>
    /// <returns>Ranking de 1 a 5</returns>
    int CalcularRanking(int scoreCredito);

    /// <summary>
    /// Calcula o limite de crédito recomendado
    /// Baseado em score, renda e histórico
    /// </summary>
    /// <param name="scoreCredito">Score de crédito</param>
    /// <param name="renda">Renda mensal em reais</param>
    /// <param name="historico">Histórico de crédito</param>
    /// <returns>Limite de crédito sugerido em reais</returns>
    decimal CalcularLimiteCredito(int scoreCredito, decimal renda, string historico);

    /// <summary>
    /// Calcula o número máximo de cartões permitidos
    /// Baseado no ranking de crédito
    /// </summary>
    /// <param name="rankingCredito">Ranking de crédito (1-5)</param>
    /// <returns>Número máximo de cartões</returns>
    int CalcularMaximoCartoes(int rankingCredito);

    /// <summary>
    /// Verifica se o cliente é elegível para emitir cartão de crédito
    /// </summary>
    /// <param name="scoreCredito">Score de crédito</param>
    /// <param name="rankingCredito">Ranking de crédito</param>
    /// <returns>true se elegível, false caso contrário</returns>
    bool EhElegivelParaCartao(int scoreCredito, int rankingCredito);
}
