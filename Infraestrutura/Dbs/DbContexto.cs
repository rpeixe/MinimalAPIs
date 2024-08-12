using Microsoft.EntityFrameworkCore;
using MinimalAPIs.Dominio.Entidades;

namespace MinimalAPIs.Infraestrutura.Dbs;

public class DbContexto : DbContext
{
    private readonly IConfiguration _configuracaoAppSettings;

    public DbContexto(IConfiguration configuracaoAppSettings)
    {
        _configuracaoAppSettings = configuracaoAppSettings;
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) {
            var stringConexao = _configuracaoAppSettings.GetConnectionString("sqlserver")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseSqlServer(stringConexao);
            }
        }
    }
}