using MinimalAPIs.Dominio.Enums;

namespace MinimalAPIs.Dominio.ModelViews;

public record AdministradorLogado()
{
    public required string Email { get; init; }
    public required string Perfil { get; init; }
    public required string Token { get; init; }
}