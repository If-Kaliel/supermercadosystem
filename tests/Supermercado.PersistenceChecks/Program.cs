using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Persistence;
using Supermercado.Infrastructure.Repositories;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MySql")
    ?? throw new InvalidOperationException("Defina ConnectionStrings__MySql com um banco MySQL local migrado.");
var options = new DbContextOptionsBuilder<SupermercadoContext>().UseMySQL(connectionString).Options;
await using var context = new SupermercadoContext(options);
if ((await context.Database.GetPendingMigrationsAsync()).Any())
    throw new InvalidOperationException("Aplique a migration antes da verificação.");

await using var transaction = await context.Database.BeginTransactionAsync();
try
{
    var suffix = Guid.NewGuid().ToString("N");
    var documento = Random.Shared.NextInt64(10000000000000, 99999999999999).ToString();
    var repository = new Repository<Categoria>(context);
    var categoria = new Categoria { Nome = $"Teste-{suffix}" };
    await repository.AddAsync(categoria);
    await repository.SaveChangesAsync();
    context.ChangeTracker.Clear();
    var read = await repository.GetByIdAsync(categoria.Id);
    Check(read is not null, "inclusão e busca por ID");
    read!.Descricao = "Atualizada";
    repository.Update(read);
    await repository.SaveChangesAsync();
    context.ChangeTracker.Clear();
    Check((await repository.ListAsync()).Any(x => x.Id == categoria.Id && x.Descricao == "Atualizada"),
        "atualização e listagem");

    var fornecedor = new Fornecedor { RazaoSocial = "Fornecedor de teste", NomeFantasia = "Teste",
        Cnpj = documento, Email = $"teste-{suffix}@example.invalid" };
    var funcionario = new Funcionario { Nome = "Operador de teste", Cpf = documento[..11],
        Matricula = suffix[..30], Cargo = "Caixa", DataAdmissao = DateTime.UtcNow };
    var caixa = new Caixa { NumeroCaixa = Random.Shared.Next(100000, int.MaxValue), Localizacao = "Teste" };
    var cliente = new Cliente { Nome = "Cliente de teste", Cpf = documento[1..12] };
    var outroCliente = new Cliente { Nome = "Segundo cliente", Cpf = documento[2..13] };
    var produto = new Produto { CategoriaId = categoria.Id, FornecedorId = fornecedor.Id,
        CodigoBarras = suffix, Nome = "Produto de teste", PrecoCusto = 5.12m, PrecoVenda = 10.25m };
    var venda = new Venda { ClienteId = cliente.Id, FuncionarioId = funcionario.Id,
        CaixaId = caixa.Id, NumeroCupom = suffix, ValorTotal = 20.50m };
    var vendaAnonima = new Venda { FuncionarioId = funcionario.Id, CaixaId = caixa.Id,
        NumeroCupom = $"ANON-{suffix}" };
    var item = new ItemVenda { VendaId = venda.Id, ProdutoId = produto.Id,
        Quantidade = 2, PrecoUnitario = 10.25m, Subtotal = 20.50m };
    var pagamento = new Pagamento { VendaId = venda.Id, FormaPagamento = "PIX", Valor = 10.00m };
    var outroPagamento = new Pagamento { VendaId = venda.Id, FormaPagamento = "Dinheiro", Valor = 10.50m };
    context.AddRange(fornecedor, funcionario, caixa, cliente, outroCliente, produto, venda, vendaAnonima,
        item, pagamento, outroPagamento);
    await context.SaveChangesAsync();
    context.ChangeTracker.Clear();

    Check(await context.Vendas.AnyAsync(x => x.Id == vendaAnonima.Id && x.ClienteId == null),
        "venda sem cliente");
    Check(await context.Clientes.CountAsync(x => (x.Id == cliente.Id || x.Id == outroCliente.Id) && x.Email == null) == 2,
        "múltiplos e-mails nulos");
    Check(await context.ItensVenda.AnyAsync(x => x.Id == item.Id && x.Subtotal == 20.50m),
        "persistência monetária");
    Check(await context.Pagamentos.CountAsync(x => x.VendaId == venda.Id) == 2, "múltiplos pagamentos");

    await ExpectDatabaseError(() => {
        context.Categorias.Add(new Categoria { Nome = categoria.Nome });
    }, 1062, "índice único");
    await ExpectDatabaseError(() => {
        context.Produtos.Add(new Produto { CategoriaId = Guid.NewGuid(), FornecedorId = fornecedor.Id,
            Nome = "FK inválida", CodigoBarras = $"FK-{suffix}" });
    }, 1452, "chave estrangeira obrigatória");
    await ExpectDatabaseError(() => context.Produtos.Remove(produto), 1451, "exclusão restrita de produto vendido");

    context.Clientes.Remove(cliente);
    await context.SaveChangesAsync();
    context.ChangeTracker.Clear();
    Check(await context.Vendas.AnyAsync(x => x.Id == venda.Id && x.ClienteId == null), "SET NULL do cliente");

    context.Vendas.Remove(venda);
    await context.SaveChangesAsync();
    context.ChangeTracker.Clear();
    Check(!await context.ItensVenda.AnyAsync(x => x.VendaId == venda.Id)
        && !await context.Pagamentos.AnyAsync(x => x.VendaId == venda.Id), "cascata de itens e pagamentos");

    var descartavel = new Categoria { Nome = $"Remover-{suffix}" };
    await repository.AddAsync(descartavel);
    await repository.SaveChangesAsync();
    repository.Remove(descartavel);
    await repository.SaveChangesAsync();
    Check(await repository.GetByIdAsync(descartavel.Id) is null, "remoção pelo repositório");
    Console.WriteLine("Todas as verificações passaram. Os dados de teste serão revertidos.");
}
finally
{
    await transaction.RollbackAsync();
}

void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException($"Falhou: {name}");
    Console.WriteLine($"OK: {name}");
}

async Task ExpectDatabaseError(Action arrange, int errorNumber, string name)
{
    context.ChangeTracker.Clear();
    arrange();
    try
    {
        await context.SaveChangesAsync();
    }
    catch (DbUpdateException exception) when (exception.InnerException is MySqlException mysql && mysql.Number == errorNumber)
    {
        context.ChangeTracker.Clear();
        Console.WriteLine($"OK: {name}");
        return;
    }
    throw new InvalidOperationException($"O banco deveria rejeitar: {name}");
}

