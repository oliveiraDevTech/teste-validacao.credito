using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;

namespace Driven.SqlLite.Data;

/// <summary>
/// Contexto de banco de dados para a aplicação
/// Gerencia a persistência de dados usando Entity Framework Core e SQLite
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// DbSet para a entidade Cliente
    /// </summary>
    public DbSet<Cliente> Clientes { get; set; }

    /// <summary>
    /// Construtor do contexto
    /// </summary>
    /// <param name="options">Opções de configuração do DbContext</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configura o modelo do banco de dados
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar a entidade Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            // Chave primária
            entity.HasKey(e => e.Id);

            // Propriedades
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Telefone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Cpf)
                .IsRequired()
                .HasMaxLength(14);

            entity.Property(e => e.Endereco)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Cidade)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(2);

            entity.Property(e => e.Cep)
                .IsRequired()
                .HasMaxLength(9);

            entity.Property(e => e.DataCriacao)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);

            entity.Property(e => e.DataAtualizacao)
                .IsRequired(false);

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Cliente_Email");

            entity.HasIndex(e => e.Cpf)
                .IsUnique()
                .HasDatabaseName("IX_Cliente_Cpf");

            entity.HasIndex(e => e.Nome)
                .HasDatabaseName("IX_Cliente_Nome");

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_Cliente_Ativo");

            // Nome da tabela
            entity.ToTable("Clientes");
        });

    }
}
