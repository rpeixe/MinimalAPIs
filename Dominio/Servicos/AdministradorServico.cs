using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Entidades;
using MinimalAPIs.Dominio.Interfaces;
using MinimalAPIs.Infraestrutura.Dbs;

namespace MinimalAPIs.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;

    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public Administrador? Login(LoginDTO loginDTO)
    {
        Administrador? adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return adm;
    }
}