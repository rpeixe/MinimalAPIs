using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPIs.Dominio.DTOs;
using MinimalAPIs.Dominio.Interfaces;
using MinimalAPIs.Dominio.Servicos;
using MinimalAPIs.Infraestrutura.Dbs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if(administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
