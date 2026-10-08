using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamentos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.FormaPagamento).HasMaxLength(30).IsRequired(true);
        builder.Property(x => x.CodigoTransacao).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired(true);
        builder.Property(x => x.Valor).HasPrecision(18, 2);

        builder.Property(x => x.VendaId).HasColumnType("char(36)");
        builder.HasOne<Venda>().WithMany().HasForeignKey(x => x.VendaId)
            .IsRequired(true).OnDelete(DeleteBehavior.Cascade);
    }
}
