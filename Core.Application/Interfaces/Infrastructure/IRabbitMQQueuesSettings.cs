namespace Core.Application.Interfaces.Infrastructure;

/// <summary>
/// Configurações de filas do RabbitMQ
/// </summary>
public interface IRabbitMQQueuesSettings
{
    /// <summary>
    /// Fila para eventos de cliente cadastrado (recebido do serviço de Clientes)
    /// </summary>
    string ClienteCadastrado { get; }

    /// <summary>
    /// Fila para eventos de análise de crédito completa (enviado para serviço de Clientes)
    /// </summary>
    string AnaliseCreditoComplete { get; }

    /// <summary>
    /// Fila para eventos de análise de crédito falhou (enviado para serviço de Clientes)
    /// </summary>
    string AnaliseCreditoFalha { get; }
}
