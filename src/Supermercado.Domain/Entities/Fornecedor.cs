namespace Supermercado.Domain.Entities;

public class Fornecedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public bool Ativo { get; set; } = true;
}