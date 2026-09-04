namespace Supermercado.Domain.Entities;

public class Produto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoriaId { get; set; }
    public Guid FornecedorId { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public decimal PrecoCusto { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }
}