namespace BSERVE_API.Models;

public class Usuario
{
    public Guid Id { get; set; }               // UUID agora
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public bool Ativo { get; set; } = true;
}