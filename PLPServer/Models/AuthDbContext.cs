using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class AuthDbContext : IdentityDbContext<BaseUser, BaseRole, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<BaseUser> Uporabniki { get; set; }
    public DbSet<Prevoznik> Prevozniki { get; set; }
    public DbSet<Inspektor> Inspektorji { get; set; }
    public DbSet<Administrator> Administratorji { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //TPH model
        builder.Entity<BaseUser>()
            .ToTable("Uporabniki")
            .HasDiscriminator<string>("Tip")
            .HasValue<Prevoznik>("prevoznik")
            .HasValue<Inspektor>("inspektor")
            .HasValue<Administrator>("admin");
    }
}