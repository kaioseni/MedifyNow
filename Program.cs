using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MedifyNow.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

#if DEBUG
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:Development"));
#else
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:Production"));
#endif

builder.Services.AddScoped<IPasswordHasher<Administrador>, PasswordHasher<Administrador>>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
app.UseAPIKey();

app.UseAuthorization();

app.MapControllers();

app.Run();
