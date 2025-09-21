
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class PLPContext : DbContext
{
    public PLPContext(DbContextOptions<PLPContext> options)
    : base(options)
    {
    }

    public DbSet<Zapis> Zapisi { get; set; }
    public DbSet<Linija> Linije { get; set; }
    public DbSet<Pogodba> Pogodbe { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Zapis>().ToTable("Zapisi");
        modelBuilder.Entity<Linija>().ToTable("Linije");
        modelBuilder.Entity<Pogodba>(ent =>
        {
            ent.ToTable("Pogodbe");
            //ent.HasKey(c => new { c.LinijaId, c.PrevoznikId });

            ent.HasOne(c => c.Linija)
                .WithMany(c => c.Pogodbe)
                .HasForeignKey(c => c.LinijaId)
                .OnDelete(DeleteBehavior.NoAction);

            ent.HasOne(c => c.Prevoznik)
                .WithMany(c => c.Pogodbe)
                .HasForeignKey(c => c.PrevoznikId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}