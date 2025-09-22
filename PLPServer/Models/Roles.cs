using Microsoft.AspNetCore.Identity;

namespace PLPServer.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Inspektor = "Inspektor";
    public const string Prevoznik = "Prevoznik";

    public static string UserToRole(this BaseUser user) => user switch
    {
        Administrator => Admin,
        Models.Inspektor => Inspektor,
        Models.Prevoznik => Prevoznik,
        _ => throw new ArgumentException($"Unknown user type {user.GetType()}")
    };
}

public class BaseRole : IdentityRole<Guid>
{
    public BaseRole(Guid id, string name) : base(name)
    {
        Id = id;
    }
}

//public class t : ISign