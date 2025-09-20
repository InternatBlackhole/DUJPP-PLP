
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class PLPContext : DbContext
{
    public PLPContext(DbContextOptions<PLPContext> options)
    : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
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
        modelBuilder.Entity<User>()
            .ToTable("Uporabniki")
            .HasDiscriminator<string>("Vloga")
            .HasValue<Prevoznik>("prevoznik")
            .HasValue<Inspektor>("inspektor")
            .HasValue<Administrator>("admin");

        modelBuilder.Entity<Zapis>().ToTable("Zapisi");
        modelBuilder.Entity<Linija>().ToTable("Linije");
        modelBuilder.Entity<Pogodba>().ToTable("Pogodbe");
    }
}