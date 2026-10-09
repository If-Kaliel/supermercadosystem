using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Supermercado.Application.Repositories;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Persistence;
using Supermercado.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(args);
}

// Uma variável vazia não deve apagar uma conexão válida dos arquivos locais.
var connectionSetting = ((IConfigurationRoot)builder.Configuration).Providers.Reverse()
    .Select(provider => new
    {
        Provider = provider,
        Value = provider.TryGet("ConnectionStrings:MySql", out var value) ? value : null
    })
    .FirstOrDefault(setting => !string.IsNullOrWhiteSpace(setting.Value));
var connectionString = connectionSetting?.Value;
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings:MySql. Em desenvolvimento, execute scripts/Iniciar-Desenvolvimento.ps1 " +
        "e use o perfil http ou https (Development).");
}
var mysqlSettings = new MySqlConnectionStringBuilder(connectionString);

builder.Services.AddDbContext<SupermercadoContext>(options => options.UseMySQL(connectionString));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();
var connectionSource = connectionSetting!.Provider is FileConfigurationProvider fileProvider
    ? fileProvider.Source.Path
    : connectionSetting.Provider.GetType().Name;
app.Logger.LogInformation("MySQL: origem {Source}; destino {Server}:{Port}/{Database}; ambiente {Environment}",
    connectionSource, mysqlSettings.Server, mysqlSettings.Port, mysqlSettings.Database, app.Environment.EnvironmentName);

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
    if (string.IsNullOrWhiteSpace(nome) || nome.Length > 80 || request.Descricao?.Length > 255)
    {
        return Results.BadRequest(new { mensagem = "Informe um nome de até 80 caracteres e uma descrição de até 255." });
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
