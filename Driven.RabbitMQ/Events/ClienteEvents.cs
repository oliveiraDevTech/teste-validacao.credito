namespace Driven.RabbitMQ.Events;

/// <summary>
/// Eventos de domínio para cliente (Event-Driven Architecture)
/// NOTA: Estes eventos estão definidos mas não são publicados em versão atual.
/// Para ativar, injete IMessagePublisher em ClienteService.
/// </summary>

/// <summary>
/// Evento disparado quando um cliente é criado
/// </summary>
public class ClienteCreatedEvent : DomainEvent
{
    public Guid ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
}

/// <summary>
/// Evento de resposta com análise de crédito do cliente
/// Disparado quando o score de crédito é atualizado via fila ou API
/// </summary>
public class ClienteCreditoAtualizadoEvent : DomainEvent
{
    /// <summary>
    /// ID do cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Score de crédito (0-1000)
    /// </summary>
    public int ScoreCredito { get; set; }

    /// <summary>
    /// Número máximo de cartões permitidos
    /// </summary>
    public int NumeroMaximoCartoes { get; set; }

    /// <summary>
    /// Limite de crédito por cartão
    /// </summary>
    public decimal LimiteCreditoPorCartao { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
