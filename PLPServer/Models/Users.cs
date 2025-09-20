using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public abstract class User
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [StringLength(100)]
    [Unicode]
    public string? DisplayName { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
}

public class Prevoznik : User
{
    public ICollection<Zapis>? Prevozi { get; set; }

    public ICollection<Pogodba>? Pogodbe { get; set; }
}

public class Inspektor : User
{

}

public class Administrator : User
{
    
}