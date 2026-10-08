using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("ItensVenda");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.PrecoUnitario).HasPrecision(10, 2);
        builder.Property(x => x.Subtotal).HasPrecision(10, 2);
        builder.Property(x => x.Desconto).HasPrecision(10, 2);

        builder.Property(x => x.VendaId).HasColumnType("char(36)");
        builder.HasOne<Venda>().WithMany().HasForeignKey(x => x.VendaId)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ProdutoId).HasColumnType("char(36)");
        builder.HasOne<Produto>().WithMany().HasForeignKey(x => x.ProdutoId)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
