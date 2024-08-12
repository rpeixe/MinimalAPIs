using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Entidades;

namespace MinimalAPIs.Dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);
}