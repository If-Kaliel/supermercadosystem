using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Supermercado.Domain.Entities;

namespace Supermercado.Infrastructure.Persistence;

public static class DadosIniciais
{
    public static async Task<int> PopularAsync(SupermercadoContext context, CancellationToken cancellationToken = default)
    {
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            throw new InvalidOperationException("Aplique as migrations com scripts/Iniciar-Desenvolvimento.ps1 antes de iniciar a API.");

        // Nos testes, a carga participa da transação que será revertida pelo programa de integração.
        await using var transaction = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var adicionados = 0;

        var mercearia = await ObterOuAdicionarAsync(new Categoria
        {
            Id = Id(1, 1), Nome = "Mercearia", Descricao = "Arroz, feijão e outros alimentos de despensa."
        }, x => x.Nome == "Mercearia");
        var bebidas = await ObterOuAdicionarAsync(new Categoria
        {
            Id = Id(1, 2), Nome = "Bebidas", Descricao = "Leites, sucos e bebidas em geral."
        }, x => x.Nome == "Bebidas");
        var limpeza = await ObterOuAdicionarAsync(new Categoria
        {
            Id = Id(1, 3), Nome = "Limpeza", Descricao = "Produtos para limpeza da casa."
        }, x => x.Nome == "Limpeza");

        var alimentos = await ObterOuAdicionarAsync(new Fornecedor
        {
            Id = Id(2, 1), RazaoSocial = "Distribuidora Alimentos Exemplo Ltda", NomeFantasia = "Alimentos Exemplo",
            Cnpj = "00000000000001", Email = "contato@alimentos.example.invalid"
        }, x => x.Cnpj == "00000000000001");
        var higiene = await ObterOuAdicionarAsync(new Fornecedor
        {
            Id = Id(2, 2), RazaoSocial = "Distribuidora Limpeza Exemplo Ltda", NomeFantasia = "Limpeza Exemplo",
            Cnpj = "00000000000002", Email = "contato@limpeza.example.invalid"
        }, x => x.Cnpj == "00000000000002");

        var produtos = new[]
        {
            new Produto { Id = Id(3, 1), CategoriaId = mercearia.Id, FornecedorId = alimentos.Id,
                CodigoBarras = "DEMO-ARROZ-001", Nome = "Arroz branco 5 kg", PrecoCusto = 18.00m,
                PrecoVenda = 24.90m, EstoqueAtual = 50, EstoqueMinimo = 10 },
            new Produto { Id = Id(3, 2), CategoriaId = mercearia.Id, FornecedorId = alimentos.Id,
                CodigoBarras = "DEMO-FEIJAO-002", Nome = "Feijão carioca 1 kg", PrecoCusto = 6.00m,
                PrecoVenda = 8.50m, EstoqueAtual = 80, EstoqueMinimo = 15 },
            new Produto { Id = Id(3, 3), CategoriaId = bebidas.Id, FornecedorId = alimentos.Id,
                CodigoBarras = "DEMO-LEITE-003", Nome = "Leite integral 1 L", PrecoCusto = 3.20m,
                PrecoVenda = 4.80m, EstoqueAtual = 100, EstoqueMinimo = 20 },
            new Produto { Id = Id(3, 4), CategoriaId = bebidas.Id, FornecedorId = alimentos.Id,
                CodigoBarras = "DEMO-SUCO-004", Nome = "Suco de uva 1 L", PrecoCusto = 4.50m,
                PrecoVenda = 6.90m, EstoqueAtual = 60, EstoqueMinimo = 10 },
            new Produto { Id = Id(3, 5), CategoriaId = limpeza.Id, FornecedorId = higiene.Id,
                CodigoBarras = "DEMO-DETERGENTE-005", Nome = "Detergente neutro 500 ml", PrecoCusto = 1.30m,
                PrecoVenda = 2.50m, EstoqueAtual = 120, EstoqueMinimo = 20 },
            new Produto { Id = Id(3, 6), CategoriaId = limpeza.Id, FornecedorId = higiene.Id,
                CodigoBarras = "DEMO-SABAO-006", Nome = "Sabão em pó 1 kg", PrecoCusto = 9.00m,
                PrecoVenda = 12.90m, EstoqueAtual = 40, EstoqueMinimo = 10 }
        };
        for (var i = 0; i < produtos.Length; i++)
        {
            var produto = produtos[i];
            produtos[i] = await ObterOuAdicionarAsync(produto, x => x.CodigoBarras == produto.CodigoBarras);
        }

        var ana = await ObterOuAdicionarAsync(new Cliente
        {
            Id = Id(4, 1), Nome = "Ana Exemplo", Cpf = "00000000001",
            Email = "ana@example.invalid", PontosFidelidade = 15
        }, x => x.Cpf == "00000000001");
        await ObterOuAdicionarAsync(new Cliente
        {
            Id = Id(4, 2), Nome = "Bruno Exemplo", Cpf = "00000000002", PontosFidelidade = 5
        }, x => x.Cpf == "00000000002");

        var carlos = await ObterOuAdicionarAsync(new Funcionario
        {
            Id = Id(5, 1), Nome = "Carlos Exemplo", Cpf = "00000000101", Matricula = "DEMO-FUNC-001",
            Cargo = "Operador de caixa", DataAdmissao = new DateTime(2025, 1, 10)
        }, x => x.Matricula == "DEMO-FUNC-001");
        var daniela = await ObterOuAdicionarAsync(new Funcionario
        {
            Id = Id(5, 2), Nome = "Daniela Exemplo", Cpf = "00000000102", Matricula = "DEMO-FUNC-002",
            Cargo = "Operadora de caixa", DataAdmissao = new DateTime(2025, 3, 15)
        }, x => x.Matricula == "DEMO-FUNC-002");

        var caixa1 = await ObterOuAdicionarAsync(new Caixa
        {
            Id = Id(6, 1), NumeroCaixa = 9001, Localizacao = "Frente de loja - exemplo 1"
        }, x => x.NumeroCaixa == 9001);
        var caixa2 = await ObterOuAdicionarAsync(new Caixa
        {
            Id = Id(6, 2), NumeroCaixa = 9002, Localizacao = "Frente de loja - exemplo 2"
        }, x => x.NumeroCaixa == 9002);

        var data = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var venda1 = await ObterOuAdicionarAsync(new Venda
        {
            Id = Id(7, 1), ClienteId = ana.Id, FuncionarioId = carlos.Id, CaixaId = caixa1.Id,
            NumeroCupom = "DEMO-0001", DataHora = data, ValorTotal = 58.30m, Status = "Concluida"
        }, x => x.NumeroCupom == "DEMO-0001");
        var venda2 = await ObterOuAdicionarAsync(new Venda
        {
            Id = Id(7, 2), ClienteId = null, FuncionarioId = daniela.Id, CaixaId = caixa2.Id,
            NumeroCupom = "DEMO-0002", DataHora = data.AddHours(1), ValorTotal = 19.40m, Status = "Concluida"
        }, x => x.NumeroCupom == "DEMO-0002");

        var itens = new[]
        {
            new ItemVenda { Id = Id(8, 1), VendaId = venda1.Id, ProdutoId = produtos[0].Id,
                Quantidade = 2, PrecoUnitario = 24.90m, Subtotal = 49.80m },
            new ItemVenda { Id = Id(8, 2), VendaId = venda1.Id, ProdutoId = produtos[1].Id,
                Quantidade = 1, PrecoUnitario = 8.50m, Subtotal = 8.50m },
            new ItemVenda { Id = Id(8, 3), VendaId = venda2.Id, ProdutoId = produtos[2].Id,
                Quantidade = 3, PrecoUnitario = 4.80m, Subtotal = 14.40m },
            new ItemVenda { Id = Id(8, 4), VendaId = venda2.Id, ProdutoId = produtos[4].Id,
                Quantidade = 2, PrecoUnitario = 2.50m, Subtotal = 5.00m }
        };
        foreach (var item in itens)
            await ObterOuAdicionarAsync(item, x => x.Id == item.Id);

        var pagamentos = new[]
        {
            new Pagamento { Id = Id(9, 1), VendaId = venda1.Id, FormaPagamento = "PIX",
                Valor = 58.30m, DataHora = data, CodigoTransacao = "DEMO-PIX-001" },
            new Pagamento { Id = Id(9, 2), VendaId = venda2.Id, FormaPagamento = "Dinheiro",
                Valor = 10.00m, DataHora = data.AddHours(1) },
            new Pagamento { Id = Id(9, 3), VendaId = venda2.Id, FormaPagamento = "Cartao",
                Valor = 9.40m, DataHora = data.AddHours(1), CodigoTransacao = "DEMO-CARTAO-002" }
        };
        foreach (var pagamento in pagamentos)
            await ObterOuAdicionarAsync(pagamento, x => x.Id == pagamento.Id);

        await context.SaveChangesAsync(cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        return adicionados;

        async Task<T> ObterOuAdicionarAsync<T>(T exemplo, Expression<Func<T, bool>> filtro) where T : class
        {
            // O ID fixo também reconhece um exemplo cujo nome ou código foi editado depois.
            var id = context.Entry(exemplo).Property<Guid>("Id").CurrentValue;
            var existente = await context.Set<T>().FindAsync([id], cancellationToken)
                ?? await context.Set<T>().FirstOrDefaultAsync(filtro, cancellationToken);
            if (existente is not null) return existente;
            context.Add(exemplo);
            adicionados++;
            return exemplo;
        }
    }

    private static Guid Id(int grupo, int numero) => Guid.Parse($"{grupo}0000000-0000-0000-0000-{numero:000000000000}");
}
