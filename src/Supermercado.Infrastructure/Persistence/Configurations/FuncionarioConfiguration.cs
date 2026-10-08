using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("Funcionarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired(true);
        builder.Property(x => x.Cpf).HasMaxLength(11).IsRequired(true);
        builder.Property(x => x.Matricula).HasMaxLength(30).IsRequired(true);
        builder.Property(x => x.Cargo).HasMaxLength(80).IsRequired(true);
        builder.HasIndex(x => x.Cpf).IsUnique();
        builder.HasIndex(x => x.Matricula).IsUnique();
    }
}
