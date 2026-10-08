using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Supermercado.Application.Repositories;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Persistence;
using Supermercado.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("Connection string 'MySql' não foi encontrada.");

builder.Services.AddDbContext<SupermercadoContext>(options => options.UseMySQL(connectionString));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/categorias", async (IRepository<Categoria> repository, CancellationToken cancellationToken) =>
    Results.Ok(await repository.ListAsync(cancellationToken)));

app.MapGet("/categorias/{id:guid}", async (Guid id, IRepository<Categoria> repository,
    CancellationToken cancellationToken) =>
{
    var categoria = await repository.GetByIdAsync(id, cancellationToken);
    return categoria is null ? Results.NotFound() : Results.Ok(categoria);
});

app.MapPost("/categorias", async (CriarCategoria request, IRepository<Categoria> repository,
    CancellationToken cancellationToken) =>
{
    var nome = request.Nome?.Trim();
    if (string.IsNullOrWhiteSpace(nome) || nome.Length > 100 || request.Descricao?.Length > 500)
    {
        return Results.BadRequest(new { mensagem = "Informe um nome de até 100 caracteres e uma descrição de até 500." });
    }

    var categoria = new Categoria { Nome = nome, Descricao = request.Descricao };
    await repository.AddAsync(categoria, cancellationToken);
    try
    {
        await repository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
    {
        return Results.Conflict(new { mensagem = "Já existe uma categoria com esse nome." });
    }

    return Results.Created($"/categorias/{categoria.Id}", categoria);
});

app.Run();

record CriarCategoria(string? Nome, string? Descricao);
