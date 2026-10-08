using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence.Configurations;

public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("Vendas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)").ValueGeneratedNever();
        builder.Property(x => x.NumeroCupom).HasMaxLength(50).IsRequired(true);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired(true);
        builder.Property(x => x.ValorTotal).HasPrecision(18, 2);
        builder.Property(x => x.DescontoTotal).HasPrecision(18, 2);
        builder.HasIndex(x => x.NumeroCupom).IsUnique();

        builder.Property(x => x.ClienteId).HasColumnType("char(36)");
        builder.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId)
            .IsRequired(false).OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.FuncionarioId).HasColumnType("char(36)");
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(x => x.FuncionarioId)
            .IsRequired(true).OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CaixaId).HasColumnType("char(36)");
        builder.HasOne<Caixa>().WithMany().HasForeignKey(x => x.CaixaId)
            .IsRequired(true).OnDelete(DeleteBehavior.Restrict);
    }
}
