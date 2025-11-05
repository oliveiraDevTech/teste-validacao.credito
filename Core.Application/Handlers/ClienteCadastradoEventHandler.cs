using System.Text.Json;
using Core.Application.Interfaces.Infrastructure;
using Core.Infra;
using Microsoft.Extensions.Logging;

namespace Core.Application.Handlers;

/// <summary>
/// Handler para processar eventos de cadastro de cliente
/// Consome ClienteCadastradoIntegrationEvent do serviço de Clientes
/// Realiza análise de crédito e publica resultado
/// </summary>
public class ClienteCadastradoEventHandler
{
    private readonly ICreditoScoringService _creditoScoringService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ClienteCadastradoEventHandler> _logger;
    private readonly IRabbitMQQueuesSettings _queuesSettings;

    public ClienteCadastradoEventHandler(
        ICreditoScoringService creditoScoringService,
        IMessagePublisher messagePublisher,
        ILogger<ClienteCadastradoEventHandler> logger,
        IRabbitMQQueuesSettings queuesSettings)
    {
        _creditoScoringService = creditoScoringService;
        _messagePublisher = messagePublisher;
        _logger = logger;
        _queuesSettings = queuesSettings;
    }

    /// <summary>
    /// Processa o evento de cadastro de cliente
    /// Realiza análise de crédito e publica resultado
    /// </summary>
    public async Task HandleAsync(ClienteCadastradoIntegrationEvent evento)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando análise de crédito para cliente {ClienteId} - {Nome}",
                evento.ClienteId, evento.Nome);

            // Validar dados básicos
            ValidarDadosDoEvento(evento);

            // Calcular score de crédito
            var scoreCredito = _creditoScoringService.CalcularScore(
                renda: evento.Renda,
                historico: evento.HistoricoCredito,
                idade: evento.Idade,
                dataNascimento: evento.DataNascimento,
                cpf: evento.CPF);

            _logger.LogInformation(
                "Score calculado para cliente {ClienteId}: {Score}",
                evento.ClienteId, scoreCredito);

            // Calcular ranking
            var rankingCredito = _creditoScoringService.CalcularRanking(scoreCredito);

            // Determinar elegibilidade
            var elegivelCartao = scoreCredito >= 600 && rankingCredito >= 3;

            // Calcular limite de crédito
            var limiteCredito = _creditoScoringService.CalcularLimiteCredito(
                scoreCredito, evento.Renda, evento.HistoricoCredito);

            // Calcular número máximo de cartões
            var numeroMaximoCartoes = _creditoScoringService.CalcularMaximoCartoes(rankingCredito);

            // Determinar motivo
            var motivo = DeterminarMotivo(elegivelCartao, scoreCredito, rankingCredito, evento.HistoricoCredito);

            _logger.LogInformation(
                "Análise concluída para cliente {ClienteId}. Elegível: {Elegivel}, Ranking: {Ranking}",
                evento.ClienteId, elegivelCartao, rankingCredito);

            // Publicar resultado da análise
            var resultadoAnalise = new AnaliseCartaoCreditoCompleteEvent
            {
                ClienteId = evento.ClienteId,
                ScoreCredito = scoreCredito,
                RankingCredito = rankingCredito,
                ElegivelCartao = elegivelCartao,
                Motivo = motivo,
                DataAnalise = DateTime.UtcNow,
                LimiteCredito = limiteCredito,
                NumeroMaximoCartoes = numeroMaximoCartoes
            };

            await _messagePublisher.PublishAsync(_queuesSettings.AnaliseCreditoComplete, resultadoAnalise);

            _logger.LogInformation(
                "Evento AnaliseCartaoCreditoCompleteEvent publicado para cliente {ClienteId}",
                evento.ClienteId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validação falhou para cliente {ClienteId}: {Motivo}",
                evento.ClienteId, ex.Message);

            // Publicar evento de falha
            await PublicarFalha(evento.ClienteId, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar análise de crédito para cliente {ClienteId}",
                evento.ClienteId);

            // Publicar evento de falha
            await PublicarFalha(evento.ClienteId, "Erro interno ao processar análise de crédito");

            throw;
        }
    }

    /// <summary>
    /// Valida os dados do evento de cadastro
    /// </summary>
    private void ValidarDadosDoEvento(ClienteCadastradoIntegrationEvent evento)
    {
        if (evento.ClienteId == Guid.Empty)
            throw new ArgumentException("ClienteId não pode ser vazio");

        if (string.IsNullOrWhiteSpace(evento.Nome))
            throw new ArgumentException("Nome do cliente não pode ser vazio");

        if (string.IsNullOrWhiteSpace(evento.CPF))
            throw new ArgumentException("CPF do cliente não pode ser vazio");

        if (evento.Renda <= 0)
            throw new ArgumentException("Renda deve ser um valor positivo");

        if (evento.Idade < 18 || evento.Idade > 120)
            throw new ArgumentException("Idade deve estar entre 18 e 120 anos");

        if (string.IsNullOrWhiteSpace(evento.HistoricoCredito))
            throw new ArgumentException("Histórico de crédito não pode ser vazio");

        if (!new[] { "BOM", "REGULAR", "RUIM" }.Contains(evento.HistoricoCredito))
            throw new ArgumentException("Histórico de crédito inválido. Deve ser: BOM, REGULAR ou RUIM");
    }

    /// <summary>
    /// Determina o motivo da decisão de crédito
    /// </summary>
    private string DeterminarMotivo(
        bool elegivel,
        int scoreCredito,
        int rankingCredito,
        string historico)
    {
        if (!elegivel)
        {
            if (scoreCredito < 600)
                return $"Score insuficiente ({scoreCredito}/1000)";

            if (rankingCredito < 3)
                return $"Ranking insuficiente ({rankingCredito}/5)";

            if (historico == "RUIM")
                return "Histórico de crédito inadequado";

            return "Não elegível para cartão";
        }

        return rankingCredito switch
        {
            5 => "Excelente classificação de crédito",
            4 => "Boa classificação de crédito",
            3 => "Classificação adequada de crédito",
            _ => "Cliente aprovado para cartão"
        };
    }

    /// <summary>
    /// Publica um evento de falha na análise de crédito
    /// </summary>
    private async Task PublicarFalha(Guid clienteId, string motivo)
    {
        try
        {
            var eventoFalha = new AnaliseCartaoCreditoFalhouEvent
            {
                ClienteId = clienteId,
                Motivo = motivo,
                DataTentativa = DateTime.UtcNow,
                PoderTentarNovamente = true
            };

            await _messagePublisher.PublishAsync(_queuesSettings.AnaliseCreditoFalha, eventoFalha);

            _logger.LogInformation(
                "Evento AnaliseCartaoCreditoFalhouEvent publicado para cliente {ClienteId}",
                clienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao publicar evento de falha para cliente {ClienteId}",
                clienteId);
        }
    }
}
