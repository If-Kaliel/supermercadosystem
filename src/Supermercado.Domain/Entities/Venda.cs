namespace Supermercado.Domain.Entities;

public class Venda
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ClienteId { get; set; } 
    public Guid FuncionarioId { get; set; }
    public Guid CaixaId { get; set; }
    public string NumeroCupom { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public decimal ValorTotal { get; set; }
    public decimal DescontoTotal { get; set; }
    public string Status { get; set; } = "Aberta";
}