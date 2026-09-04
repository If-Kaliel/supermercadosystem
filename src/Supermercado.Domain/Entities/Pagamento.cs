namespace Supermercado.Domain.Entities;

public class Pagamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VendaId { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string? CodigoTransacao { get; set; }
    public string Status { get; set; } = "Aprovado";
}