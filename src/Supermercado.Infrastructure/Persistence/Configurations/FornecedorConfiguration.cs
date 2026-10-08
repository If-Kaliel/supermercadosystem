using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.RazaoSocial).HasMaxLength(150).IsRequired(true);
        builder.Property(x => x.NomeFantasia).HasMaxLength(150).IsRequired(true);
        builder.Property(x => x.Cnpj).HasMaxLength(14).IsRequired(true);
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired(true);
        builder.Property(x => x.Telefone).HasMaxLength(20).IsRequired(false);
        builder.HasIndex(x => x.Cnpj).IsUnique();
    }
}
