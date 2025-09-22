
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class PLPContext : IdentityDbContext<BaseUser, BaseRole, Guid>
{
    public PLPContext(DbContextOptions<PLPContext> options)
    : base(options)
    {
    }

    public DbSet<BaseUser> Uporabniki { get; set; }
    public DbSet<Prevoznik> Prevozniki { get; set; }
    public DbSet<Inspektor> Inspektorji { get; set; }
    public DbSet<Administrator> Administratorji { get; set; }
    public DbSet<Narocnik> Narocniki { get; set; }

    public DbSet<Zapis> Zapisi { get; set; }
    public DbSet<Linija> Linije { get; set; }
    public DbSet<Pogodba> Pogodbe { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //TODO: get rid of this fluent API and use conventions with attributes where possible

        //TPH model
        modelBuilder.Entity<BaseUser>()
            .ToTable("Uporabniki")
            .HasDiscriminator<string>("Tip")
            .HasValue<Prevoznik>("prevoznik")
            .HasValue<Inspektor>("inspektor")
            .HasValue<Administrator>("admin");

        modelBuilder.Entity<Narocnik>()
            .ToTable(nameof(Narocniki));

        modelBuilder.Entity<Zapis>(ent =>
        {
            ent.ToTable("Zapisi");

            ent.HasOne(z => z.Pogodba)
                .WithMany(p => p.Voznje)
                .HasForeignKey(z => z.PogodbaId);
        });

        modelBuilder.Entity<Linija>().ToTable("Linije")
            .HasOne(l => l.Narocnik)
            .WithMany(n => n.Linije)
            .HasForeignKey(l => l.NarocnikId);

        modelBuilder.Entity<Pogodba>(ent =>
        {
            //ent.HasKey(c => new { c.LinijaId, c.PrevoznikId });
            ent.ToTable("Pogodbe");

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