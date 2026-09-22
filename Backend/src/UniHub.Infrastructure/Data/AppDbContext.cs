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

    // --- MAPEAMENTOS PENDENTES DA SPRINT (Membros 3 e 6) ---
    // Descomente as linhas abaixo quando as classes existirem no projeto.
    public DbSet<RestricaoAlimentar> RestricoesAlimentares { get; set; }
    // public DbSet<HorarioVenda> HorariosVenda { get; set; }
    // public DbSet<LocalVenda> LocaisVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da relação 1:0..1 entre Usuario e Vendedor
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Vendedor)
            .WithOne(v => v.Usuario)
            .HasForeignKey<Vendedor>(v => v.UsuarioId);

        // --- CONFIGURAÇÕES DE INTEGRAÇÃO DA SPRINT (Membro 7) ---
        // Descomente o bloco abaixo assim que os membros 3 e 6 finalizarem o merge.

        /*
        1. Relação N:N (Muitos-para-Muitos) entre Usuario e RestricaoAlimentar
        modelBuilder.Entity<Usuario>()
            .HasMany(u => u.Restricoes) 
            .WithMany(r => r.Usuarios)
            .UsingEntity(j => j.ToTable("UsuarioRestricaoAlimentar"));

        // 2. Relação 1:N (Um-para-Muitos) entre Vendedor e HorarioVenda
        modelBuilder.Entity<Vendedor>()
            .HasMany(v => v.Horarios)
            .WithOne(h => h.Vendedor)
            .HasForeignKey(h => h.VendedorId);

        // 3. Relação 1:N (Um-para-Muitos) entre Vendedor e LocalVenda
        modelBuilder.Entity<Vendedor>()
            .HasMany(v => v.Locais)
            .WithOne(l => l.Vendedor)
            .HasForeignKey(l => l.VendedorId);
        */
    }
}