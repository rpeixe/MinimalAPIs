using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Entidades;
using MinimalAPIs.Dominio.Interfaces;
using MinimalAPIs.Infraestrutura.Dbs;

namespace MinimalAPIs.Dominio.Servicos;

public class VeiculoServico : IVeiculoServico
{
    private readonly DbContexto _contexto;

    public VeiculoServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public void Apagar(Veiculo veiculo)
    {
        _contexto.Veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    public void Atualizar(Veiculo veiculo)
    {
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscaPorId(int id)
    {
        return _contexto.Veiculos.Find(id);
    }

    public void Incluir(Veiculo veiculo)
    {
        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public List<Veiculo> Todos(int pagina = 1, string? nome = null, string? marca = null)
    {
        var query = _contexto.Veiculos.AsQueryable();

        if(!string.IsNullOrEmpty(nome))
        {
            query = query.Where(v => v.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));
        }
        if(!string.IsNullOrEmpty(marca))
        {
            query = query.Where(v => v.Marca.Contains(marca, StringComparison.OrdinalIgnoreCase));
        }

        int itensPorPagina = 10;

        query = query.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);

        return query.ToList();
    }
}