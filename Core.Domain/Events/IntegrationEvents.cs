namespace Core.Domain.Events;

/// <summary>
/// Evento de integração: Cliente foi cadastrado (enviado pelo serviço de Clientes)
/// Este evento contém todos os dados necessários para análise de crédito
/// </summary>
public class ClienteCadastradoIntegrationEvent : DomainEvent
{
    /// <summary>
    /// ID único do cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF do cliente
    /// </summary>
    public string CPF { get; set; } = string.Empty;

    /// <summary>
    /// Email do cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Renda mensal em reais
    /// </summary>
    public decimal Renda { get; set; }

    /// <summary>
    /// Idade do cliente
    /// </summary>
    public int Idade { get; set; }

    /// <summary>
    /// Histórico de crédito: "BOM", "REGULAR", "RUIM"
    /// </summary>
    public string HistoricoCredito { get; set; } = "REGULAR";

    /// <summary>
    /// Data de nascimento para cálculos complementares
    /// </summary>
    public DateTime DataNascimento { get; set; }
}

/// <summary>
/// Evento de integração: Análise de crédito foi concluída (enviado pelo serviço de Creditos)
/// Este evento é consumido pelo serviço de Clientes para atualizar dados
/// </summary>
public class AnaliseCartaoCreditoCompleteEvent : DomainEvent
{
    /// <summary>
    /// ID do cliente analisado
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Score de crédito calculado (0-1000)
    /// </summary>
    public int ScoreCredito { get; set; }

    /// <summary>
    /// Ranking de crédito (1-5, onde 5 é o melhor)
    /// </summary>
    public int RankingCredito { get; set; }

    /// <summary>
    /// Indica se o cliente é elegível para emitir cartão
    /// </summary>
    public bool ElegivelCartao { get; set; }

    /// <summary>
    /// Motivo da decisão (aprovado, negado, etc.)
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora da análise
    /// </summary>
    public DateTime DataAnalise { get; set; }

    /// <summary>
    /// Limite de crédito recomendado
    /// </summary>
    public decimal LimiteCredito { get; set; }

    /// <summary>
    /// Número máximo de cartões permitidos
    /// </summary>
    public int NumeroMaximoCartoes { get; set; }
}

/// <summary>
/// Evento de integração: Análise de crédito falhou (erro ou rejeição)
/// Enviado pelo serviço de Creditos para o serviço de Clientes
/// </summary>
public class AnaliseCartaoCreditoFalhouEvent : DomainEvent
{
    /// <summary>
    /// ID do cliente que falhou na análise
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Motivo da falha ou rejeição
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Data da tentativa de análise
    /// </summary>
    public DateTime DataTentativa { get; set; }

    /// <summary>
    /// Indicador se pode tentar novamente
    /// </summary>
    public bool PoderTentarNovamente { get; set; } = true;
}
