namespace Supermercado.Domain.Entities;

public class Funcionario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime DataAdmissao { get; set; }
    public bool Ativo { get; set; } = true;
}