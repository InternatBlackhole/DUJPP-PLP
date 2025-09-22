
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

    public DbSet<Zapis> Zapisi { get; set; }
    public DbSet<Linija> Linije { get; set; }
    public DbSet<Pogodba> Pogodbe { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //TPH model
        modelBuilder.Entity<BaseUser>()
            .ToTable("Uporabniki")
            .HasDiscriminator<string>("Tip")
            .HasValue<Prevoznik>("prevoznik")
            .HasValue<Inspektor>("inspektor")
            .HasValue<Administrator>("admin");

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