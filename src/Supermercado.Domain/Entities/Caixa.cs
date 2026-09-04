namespace Supermercado.Domain.Entities;

public class Caixa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int NumeroCaixa { get; set; }
    public string Localizacao { get; set; } = string.Empty;
    public string Status { get; set; } = "Aberto";
}