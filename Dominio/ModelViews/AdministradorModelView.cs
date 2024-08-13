using MinimalAPIs.Dominio.Enums;

namespace MinimalAPIs.Dominio.ModelViews;

public record AdministradorModelView()
{
    public required int Id { get; init; }
    public required string Email { get; init; }
    public required string Perfil { get; init; }
}