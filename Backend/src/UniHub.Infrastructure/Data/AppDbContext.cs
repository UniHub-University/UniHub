using Microsoft.EntityFrameworkCore;
using UniHub.Domain.Entities;

namespace UniHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Mapeamento das classes para as tabelas do banco de dados
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<ItemBazar> ItensBazar { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da relação 1:0..1 entre Usuario e Vendedor
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Vendedor)
            .WithOne(v => v.Usuario)
            .HasForeignKey<Vendedor>(v => v.UsuarioId);
    }
}