using Core.Application.Interfaces.Infrastructure;

namespace Driven.RabbitMQ.Settings;

/// <summary>
/// Configurações do RabbitMQ
/// </summary>
public class RabbitMQSettings
{
    /// <summary>
    /// Nome do host do RabbitMQ
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Porta do RabbitMQ
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Nome de usuário
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Senha
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Nome da virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Timeout de conexão em milissegundos
    /// </summary>
    public int ConnectionTimeout { get; set; } = 10000;

    /// <summary>
    /// Número máximo de tentativas de reconexão
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay entre tentativas em milissegundos
    /// </summary>
    public int RetryDelay { get; set; } = 1000;

    /// <summary>
    /// Configurações de filas do RabbitMQ
    /// </summary>
    public RabbitMQQueuesSettings Queues { get; set; } = new();
}

/// <summary>
/// Configurações de filas do RabbitMQ
/// </summary>
public class RabbitMQQueuesSettings : IRabbitMQQueuesSettings
{
    /// <summary>
    /// Fila para eventos de cliente cadastrado (recebido do serviço de Clientes)
    /// </summary>
    public string ClienteCadastrado { get; set; } = "cliente.cadastrado";

    /// <summary>
    /// Fila para eventos de análise de crédito completa (enviado para serviço de Clientes)
    /// </summary>
    public string AnaliseCreditoComplete { get; set; } = "analise.credito.complete";

    /// <summary>
    /// Fila para eventos de análise de crédito falhou (enviado para serviço de Clientes)
    /// </summary>
    public string AnaliseCreditoFalha { get; set; } = "analise.credito.falha";
}
