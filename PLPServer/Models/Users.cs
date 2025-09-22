using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class BaseUser : IdentityUser<Guid>
{
    //public virtual IdentityRole Role { get; private set; }
}

public class Prevoznik : BaseUser
{
    //virtual for lazy loading
    public ICollection<Pogodba>? Pogodbe { get; set; }
}

public class Inspektor : BaseUser
{

}

public class Administrator : BaseUser
{

}

public class Narocnik
{
    public Guid Id { get; set; }

    public required string Ime { get; set; }

    public ICollection<Linija>? Linije { get; set; }
}