namespace Supermercado.Domain.Entities;

public class ItemVenda
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VendaId { get; set; }
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
}