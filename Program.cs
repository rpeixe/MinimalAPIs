using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Entidades;
using MinimalAPIs.Dominio.Enums;
using MinimalAPIs.Dominio.Interfaces;
using MinimalAPIs.Dominio.ModelViews;
using MinimalAPIs.Dominio.Servicos;
using MinimalAPIs.Infraestrutura.Dbs;

#region Builder
var builder = WebApplication.CreateBuilder(args);

string? key = builder.Configuration.GetSection("Jwt")["Key"];
if (string.IsNullOrEmpty(key))
{
    key = "123456";
}

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters {
        ValidateLifetime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira seu token JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
string gerarTokenJwt(Administrador administrador)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials,
        claims: claims
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    var administrador = administradorServico.Login(loginDTO);
    if(administrador != null)
    {
        string token = gerarTokenJwt(administrador);
        return Results.Ok(new AdministradorLogado {
            Email = administrador.Email,
            Perfil = administrador.Perfil,
            Token = token
        });
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

    return Results.Created($"administradores/{administrador.Id}", new AdministradorModelView {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => {
    var administradores = administradorServico.Todos(pagina);
    var admViews = administradores.Select(adm => new AdministradorModelView {
        Id = adm.Id,
        Email = adm.Email,
        Perfil = adm.Perfil
    });

    return Results.Ok(admViews);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

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
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");
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
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veículos");

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
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null)
    {
        return Results.NotFound();
    }

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veículos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
