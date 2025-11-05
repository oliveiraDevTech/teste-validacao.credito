namespace Test.XUnit.Domain;

/// <summary>
/// Testes unitários para a entidade Cliente
/// Testa criação, atualização e regras de crédito
/// </summary>
public class ClienteTests
{
    [Fact]
    public void Criar_DeveRetornarClienteComDadosCorretos()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";
        var telefone = "11987654321";
        var cpf = "111.444.777-35";
        var endereco = "Rua das Flores, 123";
        var cidade = "São Paulo";
        var estado = "SP";
        var cep = "01234-567";

        // Act
        var cliente = Cliente.Criar(nome, email, telefone, cpf, endereco, cidade, estado, cep);

        // Assert
        cliente.Should().NotBeNull();
        cliente.Id.Should().NotBe(Guid.Empty);
        cliente.Nome.Should().Be(nome);
        cliente.Email.Should().Be(email);
        cliente.Telefone.Should().Be(telefone);
        cliente.Cpf.Should().Be(cpf);
        cliente.Endereco.Should().Be(endereco);
        cliente.Cidade.Should().Be(cidade);
        cliente.Estado.Should().Be(estado);
        cliente.Cep.Should().Be(cep);
        cliente.Ativo.Should().BeTrue();
        cliente.DataCriacao.Should().NotBe(default(DateTime));

        // Crédito inicialmente vazio
        cliente.ScoreCredito.Should().Be(0);
        cliente.NumeroMaximoCartoes.Should().Be(0);
        cliente.LimiteCreditoPorCartao.Should().Be(0);
    }

    [Fact]
    public void Atualizar_DeveAlterarDadosDoCliente()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        var novoNome = "João Silva Santos";
        var novoEmail = "joao.santos@example.com";
        var novoTelefone = "11999999999";
        var novoEndereco = "Avenida Paulista, 1000";
        var novaCidade = "São Paulo";
        var novoEstado = "SP";
        var novoCep = "01310-100";

        // Act
        cliente.Atualizar(novoNome, novoEmail, novoTelefone, novoEndereco,
            novaCidade, novoEstado, novoCep);

        // Assert
        cliente.Nome.Should().Be(novoNome);
        cliente.Email.Should().Be(novoEmail);
        cliente.Telefone.Should().Be(novoTelefone);
        cliente.Endereco.Should().Be(novoEndereco);
        cliente.Cidade.Should().Be(novaCidade);
        cliente.Estado.Should().Be(novoEstado);
        cliente.Cep.Should().Be(novoCep);
        cliente.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Desativar_DeveMarcarClienteComoInativo()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act
        cliente.Desativar();

        // Assert
        cliente.Ativo.Should().BeFalse();
        cliente.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Ativar_DeveMarcarClienteComoAtivo()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");
        cliente.Desativar();

        // Act
        cliente.Ativar();

        // Assert
        cliente.Ativo.Should().BeTrue();
    }

    [Fact]
    public void ClienteNovo_DeveHerdarDaBaseEntity()
    {
        // Arrange & Act
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Assert
        cliente.Should().BeAssignableTo<BaseEntity>();
        cliente.Id.Should().NotBe(Guid.Empty);
        cliente.DataCriacao.Should().NotBe(default);
        cliente.Ativo.Should().BeTrue();
    }

    #region Testes de Crédito

    [Theory]
    [InlineData(50, 0, 0)]     // 0-100: Sem cartão
    [InlineData(100, 0, 0)]    // 0-100: Sem cartão
    [InlineData(101, 1, 1000)] // 101-500: 1 cartão, R$ 1.000
    [InlineData(300, 1, 1000)] // 101-500: 1 cartão, R$ 1.000
    [InlineData(500, 1, 1000)] // 101-500: 1 cartão, R$ 1.000
    [InlineData(501, 2, 5000)] // 501+: 2 cartões, R$ 5.000
    [InlineData(750, 2, 5000)] // 501+: 2 cartões, R$ 5.000
    [InlineData(1000, 2, 5000)]// 501+: 2 cartões, R$ 5.000
    public void AtualizarCredito_DeveAplicarRegrasDeScoreCorretas(int score, int cartoeEsperados, decimal limiteEsperado)
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act
        cliente.AtualizarCredito(score);

        // Assert
        cliente.ScoreCredito.Should().Be(score);
        cliente.NumeroMaximoCartoes.Should().Be(cartoeEsperados);
        cliente.LimiteCreditoPorCartao.Should().Be(limiteEsperado);
        cliente.DataUltimaAvaliacaoCredito.Should().NotBeNull();
    }

    [Fact]
    public void AtualizarCredito_ComScoreBaixo_DeveRetornarSemCartao()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act
        cliente.AtualizarCredito(75);

        // Assert
        cliente.ScoreCredito.Should().Be(75);
        cliente.NumeroMaximoCartoes.Should().Be(0);
        cliente.LimiteCreditoPorCartao.Should().Be(0);
    }

    [Fact]
    public void AtualizarCredito_ComScoreMedio_DevePermitirUmCartao()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act
        cliente.AtualizarCredito(250);

        // Assert
        cliente.ScoreCredito.Should().Be(250);
        cliente.NumeroMaximoCartoes.Should().Be(1);
        cliente.LimiteCreditoPorCartao.Should().Be(1000);
    }

    [Fact]
    public void AtualizarCredito_ComScoreAlto_DevePermitirDoisCartoes()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act
        cliente.AtualizarCredito(850);

        // Assert
        cliente.ScoreCredito.Should().Be(850);
        cliente.NumeroMaximoCartoes.Should().Be(2);
        cliente.LimiteCreditoPorCartao.Should().Be(5000);
    }

    [Fact]
    public void AtualizarCredito_DeveValidarScoreEntre0E1000()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act & Assert - Score negativo
        var ex = Assert.Throws<ArgumentException>(() => cliente.AtualizarCredito(-1));
        ex.Message.Should().Contain("deve estar entre 0 e 1000");

        // Act & Assert - Score > 1000
        ex = Assert.Throws<ArgumentException>(() => cliente.AtualizarCredito(1001));
        ex.Message.Should().Contain("deve estar entre 0 e 1000");
    }

    [Fact]
    public void AtualizarCredito_DeveMarcarDataAtualizacao()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");
        var dataAntes = DateTime.UtcNow;

        // Act
        cliente.AtualizarCredito(500);

        // Assert
        cliente.DataUltimaAvaliacaoCredito.Should().NotBeNull();
        cliente.DataUltimaAvaliacaoCredito.Should().BeOnOrAfter(dataAntes);
    }

    [Fact]
    public void AtualizarCredito_Multiplas_DeveAtualizar()
    {
        // Arrange
        var cliente = Cliente.Criar("João Silva", "joao@example.com", "11987654321",
            "111.444.777-35", "Rua das Flores, 123", "São Paulo", "SP", "01234-567");

        // Act - Primeira atualização
        cliente.AtualizarCredito(200);
        cliente.NumeroMaximoCartoes.Should().Be(1);
        cliente.LimiteCreditoPorCartao.Should().Be(1000);

        // Act - Segunda atualização com score melhor
        cliente.AtualizarCredito(750);
        cliente.ScoreCredito.Should().Be(750);
        cliente.NumeroMaximoCartoes.Should().Be(2);
        cliente.LimiteCreditoPorCartao.Should().Be(5000);
    }

    #endregion
}


