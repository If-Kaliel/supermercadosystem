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
    var categoriaAnterior = await context.Categorias.FirstOrDefaultAsync(x => x.Nome == "Mercearia");
    if (categoriaAnterior is null)
    {
        categoriaAnterior = new Categoria { Nome = "Mercearia", Descricao = "Cadastro anterior à carga", Ativo = false };
        context.Categorias.Add(categoriaAnterior);
        await context.SaveChangesAsync();
    }
    var descricaoAnterior = categoriaAnterior.Descricao;
    var ativoAnterior = categoriaAnterior.Ativo;
    await DadosIniciais.PopularAsync(context);
    // Simula a edição de uma chave do exemplo; reiniciar não deve recriar nem sobrescrever o registro.
    var produtoEditado = await context.Produtos.FindAsync(Guid.Parse("30000000-0000-0000-0000-000000000001"));
    if (produtoEditado is not null)
    {
        produtoEditado.CodigoBarras = "TESTE-CODIGO-EDITADO";
        await context.SaveChangesAsync();
    }
    var primeiraCarga = await ContarRegistrosAsync();
    Check(primeiraCarga.All(total => total > 0), "carga de exemplos nas nove entidades");
    context.ChangeTracker.Clear();
    var segundaCarga = await DadosIniciais.PopularAsync(context);
    var contagemSegundaCarga = await ContarRegistrosAsync();
    var chavePreservada = produtoEditado is null || await context.Produtos.AnyAsync(x =>
        x.Id == produtoEditado.Id && x.CodigoBarras == "TESTE-CODIGO-EDITADO");
    Check(segundaCarga == 0 && chavePreservada && primeiraCarga.SequenceEqual(contagemSegundaCarga),
        "carga inicial sem duplicação");
    var categoriaPreservada = await context.Categorias.SingleAsync(x => x.Nome == "Mercearia");
    Check(categoriaPreservada.Id == categoriaAnterior.Id && categoriaPreservada.Descricao == descricaoAnterior
        && categoriaPreservada.Ativo == ativoAnterior, "carga preserva cadastro anterior");
    var vendasExemplo = await context.Vendas.Where(x => x.NumeroCupom == "DEMO-0001" || x.NumeroCupom == "DEMO-0002").ToListAsync();
    var totaisCoerentes = true;
    foreach (var exemplo in vendasExemplo)
    {
        var subtotal = await context.ItensVenda.Where(x => x.VendaId == exemplo.Id).SumAsync(x => x.Subtotal - x.Desconto);
        var pago = await context.Pagamentos.Where(x => x.VendaId == exemplo.Id).SumAsync(x => x.Valor);
        totaisCoerentes &= exemplo.ValorTotal == subtotal - exemplo.DescontoTotal && exemplo.ValorTotal == pago;
    }
    Check(vendasExemplo.Count == 2 && totaisCoerentes, "totais dos itens, vendas e pagamentos de exemplo");
    context.ChangeTracker.Clear();

    var suffix = Guid.NewGuid().ToString("N")[..24];
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
        Matricula = suffix, Cargo = "Caixa", DataAdmissao = DateTime.UtcNow };
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

    await ExpectDatabaseError(() => {
        context.Categorias.Add(new Categoria { Nome = new string('A', 81) });
    }, 1406, "nome da categoria limitado a 80 caracteres");
    await ExpectDatabaseError(() => {
        context.Categorias.Add(new Categoria { Nome = $"Descricao-{suffix}", Descricao = new string('A', 256) });
    }, 1406, "descrição da categoria limitada a 255 caracteres");
    await ExpectDatabaseError(() => {
        context.Vendas.Add(new Venda { ClienteId = Guid.NewGuid(), FuncionarioId = funcionario.Id,
            CaixaId = caixa.Id, NumeroCupom = $"FK-{suffix}" });
    }, 1452, "cliente opcional precisa existir quando informado");
    await ExpectDatabaseError(() => {
        produto.PrecoVenda = 100000000.00m;
        context.Produtos.Update(produto);
    }, 1264, "precisão monetária decimal(10,2)");

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

async Task<int[]> ContarRegistrosAsync() =>
[
    await context.Categorias.CountAsync(), await context.Fornecedores.CountAsync(),
    await context.Produtos.CountAsync(), await context.Clientes.CountAsync(),
    await context.Funcionarios.CountAsync(), await context.Caixas.CountAsync(),
    await context.Vendas.CountAsync(), await context.ItensVenda.CountAsync(), await context.Pagamentos.CountAsync()
];
