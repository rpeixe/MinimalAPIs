using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Entidades;

namespace MinimalAPIs.Dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);
    void Incluir(Administrador administrador);
    Administrador? BuscaPorId(int id);
    List<Administrador> Todos(int? pagina);
    void Atualizar(Administrador administrador);
    void Apagar(Administrador administrador);
}