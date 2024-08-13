using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Entidades;
using MinimalAPIs.Dominio.Enums;
using MinimalAPIs.Dominio.Interfaces;
using MinimalAPIs.Dominio.ModelViews;
using MinimalAPIs.Dominio.Servicos;
using MinimalAPIs.Infraestrutura.Dbs;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if(administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
    var validacao = new ErrosDeValidacao {
        Mensagens = new List<string>()
    };
    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.Mensagens.Add("O email não pode ser vazio");
    }
    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("A senha não pode ser vazio");
    }
    if (administradorDTO.Perfil == null)
    {
        validacao.Mensagens.Add("O perfil não pode ser vazio");
    }

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };
    administradorServico.Incluir(administrador);

    return Results.Created($"administradores/{administrador.Id}", administrador);
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => {
    var administradores = administradorServico.Todos(pagina);
    var admViews = administradores.Select(adm => new AdministradorModelView {
        Id = adm.Id,
        Email = adm.Email,
        Perfil = adm.Perfil
    });

    return Results.Ok(admViews);
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
    var administrador = administradorServico.BuscaPorId(id);

    if(administrador == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new AdministradorModelView {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).WithTags("Administradores");
#endregion

#region Veículos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O nome não pode ser vazio");
    }
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca não pode ficar em branco");
    }
    if (veiculoDTO.Ano < 1900)
    {
        validacao.Mensagens.Add("O ano deve ser maior ou igual a 1900");
    }

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
    var validacao = validaDTO(veiculoDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);

    return Results.Created($"veiculos/{veiculo.Id}", veiculo);
}).WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(veiculo);
}).WithTags("Veículos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null)
    {
        return Results.NotFound();
    }

    var validacao = validaDTO(veiculoDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null)
    {
        return Results.NotFound();
    }

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veículos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
