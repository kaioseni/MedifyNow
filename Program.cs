using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;   // <<< adicione este using
using MedifyNow.Controllers;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:Development"));


builder.Services.AddScoped<IPasswordHasher<Administrador>, PasswordHasher<Administrador>>();
builder.Services.AddScoped<IPasswordHasher<Usuario>,      PasswordHasher<Usuario>>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
