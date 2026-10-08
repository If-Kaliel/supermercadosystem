using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.CodigoBarras).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PrecoVenda).HasPrecision(10, 2);
        builder.Property(x => x.PrecoCusto).HasPrecision(10, 2);
        builder.HasIndex(x => x.CodigoBarras).IsUnique();

        builder.Property(x => x.CategoriaId).HasColumnType("char(36)");
        builder.HasOne<Categoria>().WithMany().HasForeignKey(x => x.CategoriaId)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.FornecedorId).HasColumnType("char(36)");
        builder.HasOne<Fornecedor>().WithMany().HasForeignKey(x => x.FornecedorId)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
