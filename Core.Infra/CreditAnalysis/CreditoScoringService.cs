namespace Core.Infra;

/// <summary>
/// Implementação do serviço de scoring de crédito
/// Realiza cálculos baseados em dados do cliente
/// Não acessa banco de dados - totalmente stateless
/// </summary>
public class CreditoScoringService : ICreditoScoringService
{
    // Constantes de scoring
    private const int SCORE_MINIMO = 0;
    private const int SCORE_MAXIMO = 1000;
    private const int RANKING_MINIMO = 1;
    private const int RANKING_MAXIMO = 5;

    // Thresholds de score para ranking
    private const int SCORE_RANKING_1 = 200;   // Até 200 = Ranking 1
    private const int SCORE_RANKING_2 = 400;   // 201-400 = Ranking 2
    private const int SCORE_RANKING_3 = 600;   // 401-600 = Ranking 3
    private const int SCORE_RANKING_4 = 800;   // 601-800 = Ranking 4
    // Acima de 800 = Ranking 5

    // Requisitos mínimos para cartão
    private const int SCORE_MINIMO_CARTAO = 600;
    private const int RANKING_MINIMO_CARTAO = 3;

    // Limites e percentuais
    private const decimal PERCENTUAL_RENDA_LIMITE_MINIMO = 2m;      // 2x a renda mensal
    private const decimal PERCENTUAL_RENDA_LIMITE_BOM = 5m;         // 5x a renda mensal
    private const decimal PERCENTUAL_RENDA_LIMITE_EXCELENTE = 10m;  // 10x a renda mensal

    /// <summary>
    /// Calcula o score de crédito baseado em múltiplos critérios
    /// </summary>
    public int CalcularScore(
        decimal renda,
        string historico,
        int idade,
        DateTime dataNascimento,
        string cpf)
    {
        int score = 0;

        // 1. Score baseado no histórico de crédito (até 400 pontos)
        score += CalcularScoreHistorico(historico);

        // 2. Score baseado na renda (até 300 pontos)
        score += CalcularScoreRenda(renda);

        // 3. Score baseado na idade (até 200 pontos)
        score += CalcularScoreIdade(idade, dataNascimento);

        // 4. Score baseado no CPF (até 100 pontos)
        // Validação básica: se CPF é válido, ganha pontos
        score += CalcularScoreCPF(cpf);

        // Garantir que está dentro dos limites
        return Math.Min(Math.Max(score, SCORE_MINIMO), SCORE_MAXIMO);
    }

    /// <summary>
    /// Calcula o ranking baseado no score
    /// </summary>
    public int CalcularRanking(int scoreCredito)
    {
        if (scoreCredito <= SCORE_RANKING_1)
            return 1;

        if (scoreCredito <= SCORE_RANKING_2)
            return 2;

        if (scoreCredito <= SCORE_RANKING_3)
            return 3;

        if (scoreCredito <= SCORE_RANKING_4)
            return 4;

        return 5;
    }

    /// <summary>
    /// Calcula o limite de crédito baseado em score, renda e histórico
    /// </summary>
    public decimal CalcularLimiteCredito(int scoreCredito, decimal renda, string historico)
    {
        decimal percentual = historico switch
        {
            "BOM" => PERCENTUAL_RENDA_LIMITE_BOM,           // 5x
            "REGULAR" => PERCENTUAL_RENDA_LIMITE_MINIMO,    // 2x
            "RUIM" => 1m,                                    // 1x
            _ => 0m
        };

        // Aplicar bônus baseado no score
        if (scoreCredito >= 800)
            percentual = PERCENTUAL_RENDA_LIMITE_EXCELENTE; // 10x

        var limite = renda * percentual;

        // Aplicar limites mínimos e máximos
        const decimal limiteMinimo = 500m;
        const decimal limiteMaximo = 100000m;

        return Math.Min(Math.Max(limite, limiteMinimo), limiteMaximo);
    }

    /// <summary>
    /// Calcula o número máximo de cartões baseado no ranking
    /// </summary>
    public int CalcularMaximoCartoes(int rankingCredito)
    {
        return rankingCredito switch
        {
            1 => 1,  // Apenas 1 cartão
            2 => 2,  // Até 2 cartões
            3 => 2,  // Até 2 cartões
            4 => 3,  // Até 3 cartões
            5 => 5,  // Até 5 cartões
            _ => 0
        };
    }

    /// <summary>
    /// Verifica elegibilidade para cartão de crédito
    /// </summary>
    public bool EhElegivelParaCartao(int scoreCredito, int rankingCredito)
    {
        return scoreCredito >= SCORE_MINIMO_CARTAO &&
               rankingCredito >= RANKING_MINIMO_CARTAO;
    }

    // ========== MÉTODOS PRIVADOS DE CÁLCULO ==========

    /// <summary>
    /// Calcula score baseado no histórico de crédito (até 400 pontos)
    /// </summary>
    private int CalcularScoreHistorico(string historico)
    {
        return historico switch
        {
            "BOM" => 400,       // Score máximo para histórico bom
            "REGULAR" => 200,   // Score médio para histórico regular
            "RUIM" => 0,        // Sem pontos para histórico ruim
            _ => 0
        };
    }

    /// <summary>
    /// Calcula score baseado na renda (até 300 pontos)
    /// Quanto maior a renda, maior o score
    /// </summary>
    private int CalcularScoreRenda(decimal renda)
    {
        // Basicamente: mais renda = mais score
        // Escala: até 300 pontos para renda > R$ 10.000
        if (renda >= 10000)
            return 300;

        if (renda >= 5000)
            return 225;

        if (renda >= 2000)
            return 150;

        if (renda >= 1000)
            return 75;

        return 0;
    }

    /// <summary>
    /// Calcula score baseado na idade (até 200 pontos)
    /// Faixas: jovens adultos (melhores) até idosos (piores)
    /// </summary>
    private int CalcularScoreIdade(int idade, DateTime dataNascimento)
    {
        // Melhor faixa: 25-45 anos
        if (idade >= 25 && idade <= 45)
            return 200;

        // Boa faixa: 22-24 e 46-55
        if ((idade >= 22 && idade < 25) || (idade > 45 && idade <= 55))
            return 150;

        // Faixa aceitável: 18-21 e 56-65
        if ((idade >= 18 && idade < 22) || (idade > 55 && idade <= 65))
            return 100;

        // Faixa de risco: abaixo de 18 ou acima de 65
        return 50;
    }

    /// <summary>
    /// Calcula score baseado na validação do CPF (até 100 pontos)
    /// </summary>
    private int CalcularScoreCPF(string cpf)
    {
        // Validação básica: se CPF está preenchido e parece válido
        if (string.IsNullOrWhiteSpace(cpf))
            return 0;

        // Remover formatação
        var cpfLimpo = cpf.Replace(".", "").Replace("-", "");

        // Verificar tamanho
        if (cpfLimpo.Length != 11)
            return 0;

        // Verificar se é apenas dígitos
        if (!cpfLimpo.All(char.IsDigit))
            return 0;

        // Se passou nas validações básicas, dá pontos
        return 100;
    }
}
