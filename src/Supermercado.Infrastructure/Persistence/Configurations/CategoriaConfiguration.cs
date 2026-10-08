using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(100).IsRequired(true);
        builder.Property(x => x.Descricao).HasMaxLength(500).IsRequired(false);
        builder.HasIndex(x => x.Nome).IsUnique();
    }
}
