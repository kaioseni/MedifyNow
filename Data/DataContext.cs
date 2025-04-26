using Microsoft.EntityFrameworkCore;


public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<TipoExame> TipoExames { get; set; } = null!;
    public DbSet<Agendamento> Agendamentos { get; set; } = null!;
    public DbSet<Administrador> Administrador { get; set; } = null!;
     public DbSet<Usuario> Usuarios { get; set; } = null!;
}