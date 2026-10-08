using Microsoft.EntityFrameworkCore;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence;

public class SupermercadoContext(DbContextOptions<SupermercadoContext> options) : DbContext(options)
{
    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupermercadoContext).Assembly);
    }
}
