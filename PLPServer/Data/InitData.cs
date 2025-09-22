using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Npgsql.TypeMapping;
using PLPServer.Models;

namespace PLPServer.Data;

public static partial class InitData
{
    private static TUser UserInit<TUser>(string guid, string email, Action<TUser>? action = null) where TUser : BaseUser, new()
    {
        var match = MyRegex().Match(email);
        var name = match.Groups["name"].Value;

        var usr = new TUser()
        {
            Id = Guid.Parse(guid),
            Email = email,
            UserName = name
        };

        action?.Invoke(usr);
        return usr;
    }

    private static Dictionary<BaseUser, string> users = new()
    {
    {
            UserInit<Administrator>("76f5518a-44fa-41c5-9c12-99d58dc9a72f", "admin@test.com"),
            "0adminTest;"
    },

    {
            UserInit<Prevoznik>("32f116f4-769d-47fc-b168-6280828fea1b", "prevoznik1@test.com"),
            "Prevoznik1!"
    },
    {
            UserInit<Prevoznik>("543e5a11-b07d-4894-aa82-d0109e07d002", "prevoznik2@test.com"),
            "Prevoznik1!"
    },
    {
            UserInit<Prevoznik>("24d3befa-2523-4df2-9053-0af2ae56d3c9", "prevoznik3@test.com"),
            "Prevoznik1!"
    },
    {
            UserInit<Prevoznik>("3021a6ed-4b26-42b3-8c24-14f527685c6e", "prevoznik34@test.com"),
            "Prevoznik1!"
    },

    {
            UserInit<Inspektor>("95398ce2-6acb-44b5-8979-cdd42ed5edfd", "inspektor1@test.com"),
            "Inspekt0r!"
    },
    {
            UserInit<Inspektor>("71040f00-295c-46f1-bc18-60ab7af910d9", "inspektor2@test.com"),
            "Inspekt0r!"
    },
    };

    private static List<Narocnik> narocniki = [
        new() { Id = Guid.Parse("b20effbd-8dd2-4fc0-9400-8b56b3b29a46"), Ime = "OŠ1"},
        new() { Id = Guid.Parse("b329eb79-1679-428a-84cd-06b32765f90e"), Ime = "Občina"}
    ];

    private static readonly List<Pogodba> pogodbe = [
        new()
        {
            Id = Guid.Parse("c5d471f8-f48d-4cb4-ba74-43568b1cbebc"),
            LinijaId = Guid.Parse("1befeb95-080a-4239-adf3-62fac9343e5c"),
            PrevoznikId = Guid.Parse("543e5a11-b07d-4894-aa82-d0109e07d002"),
            Znesek = 1000000.0
        },
        new()
        {
            Id = Guid.Parse("bdc83d3a-c6df-449f-bffb-fff686107faa"),
            LinijaId = Guid.Parse("1befeb95-080a-4239-adf3-62fac9343e5c"),
            PrevoznikId = Guid.Parse("24d3befa-2523-4df2-9053-0af2ae56d3c9"),
            Znesek = 999999.99
        },
        new()
        {
            Id = Guid.Parse("3764fe54-dd3d-482e-ad8b-752ca5fe3154"),
            LinijaId = Guid.Parse("26a858a3-3f37-4ffd-b671-03d5a9648d3d"),
            PrevoznikId = Guid.Parse("24d3befa-2523-4df2-9053-0af2ae56d3c9"),
            Znesek = 1.0
        },
        new()
        {
            Id = Guid.Parse("09ab5ed4-8221-441d-8faf-f9b5b6466287"),
            LinijaId = Guid.Parse("1befeb95-080a-4239-adf3-62fac9343e5c"),
            PrevoznikId = Guid.Parse("32f116f4-769d-47fc-b168-6280828fea1b"),
            Znesek = 888.9,
        }
    ];

    private static readonly List<Zapis> voznje = [
        new()
        {
            Id = Guid.Parse("e7f034fc-6eb3-4275-92f0-981650ade64f"),
            PogodbaId = Guid.Parse("09ab5ed4-8221-441d-8faf-f9b5b6466287"),
            ZacetekVoznje = ISOString("2024-01-02 09:09"),
            KonecVoznje = ISOString("2024-01-02 10:00")
        },
        new()
        {
            Id = Guid.Parse("74ea94c2-827e-4938-9ddb-0e01953a0b8f"),
            PogodbaId = Guid.Parse("c5d471f8-f48d-4cb4-ba74-43568b1cbebc"),
            ZacetekVoznje = ISOString("2024-07-07 08:05"),
            KonecVoznje = ISOString("2024-07-07 10:01")
        },
        new()
        {
            Id = Guid.Parse("a93c74fa-d102-4eb8-bcde-e7e31b4961d7"),
            PogodbaId = Guid.Parse("3764fe54-dd3d-482e-ad8b-752ca5fe3154"),
            ZacetekVoznje = ISOString("2025-04-29 13:09"),
            KonecVoznje = ISOString("2024-04-29 14:01")
        },
        new()
        {
            Id = Guid.Parse("e20ab10a-3350-4687-8fa9-a532c8f4fdbf"),
            PogodbaId = Guid.Parse("3764fe54-dd3d-482e-ad8b-752ca5fe3154"),
            ZacetekVoznje = ISOString("2023-06-02 20:00"),
            KonecVoznje = ISOString("2024-06-03 10:00")
        },
        new()
        {
            Id = Guid.Parse("d9a4c332-8f58-428c-8f6d-f43c6ee0ded2"),
            PogodbaId = Guid.Parse("09ab5ed4-8221-441d-8faf-f9b5b6466287"),
            ZacetekVoznje = ISOString("2024-12-14 14:08"),
            KonecVoznje = ISOString("2024-12-14 16:45")
        },
        new()
        {
            Id = Guid.Parse("54a04932-4ecb-4def-9ea4-a9cc536f8a53"),
            PogodbaId = Guid.Parse("09ab5ed4-8221-441d-8faf-f9b5b6466287"),
            ZacetekVoznje = ISOString("2026-10-31 23:55"),
            KonecVoznje = ISOString("2024-11-10 10:00")
        },
    ];

    private static readonly List<Linija> linije = [
        new()
        {
            Id = Guid.Parse("1befeb95-080a-4239-adf3-62fac9343e5c"),
            NarocnikId = Guid.Parse("b20effbd-8dd2-4fc0-9400-8b56b3b29a46"),
            Ime = "Linija1"
        },
        new()
        {
            Id = Guid.Parse("26a858a3-3f37-4ffd-b671-03d5a9648d3d"),
            NarocnikId = Guid.Parse("b329eb79-1679-428a-84cd-06b32765f90e"),
            Ime = "Linija2"
        },
    ];

    private static DateTime ISOString(string date) =>
        DateTime.ParseExact(date,
            "yyyy-MM-dd HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal).ToUniversalTime();

    public static async Task SeedTestData(IServiceProvider provider)
    {
        var mainDb = provider.GetRequiredService<PLPContext>();
        var userManager = provider.GetRequiredService<UserManager<BaseUser>>();

        foreach (var (user, pwd) in users)
        {
            var role = user.UserToRole();
            await UserBuilder.Builder(user).AddRole(role).Create(userManager, pwd);
        }

        // insert linje
        await mainDb.Linije.AddRangeAsync(linije);
        await mainDb.Narocniki.AddRangeAsync(narocniki);
        await mainDb.Pogodbe.AddRangeAsync(pogodbe);
        await mainDb.Zapisi.AddRangeAsync(voznje);

        await mainDb.SaveChangesAsync();
    }

    public static async Task<IdentityResult> CreateRoles(IServiceProvider provider)
    {
        var roleManager = provider.GetRequiredService<RoleManager<BaseRole>>();

        foreach (var role in new[] { Roles.Prevoznik, Roles.Inspektor, Roles.Admin })
        {
            IdentityResult r;
            if (!await roleManager.RoleExistsAsync(role.ToString()))
            {
                r = await roleManager.CreateAsync(new BaseRole(Guid.NewGuid(), role.ToString()));
                if (r != IdentityResult.Success)
                    return r;
            }
        }
        return IdentityResult.Success;
    }

    public class UserBuilder
    {

        private readonly List<string> roles = new();

        private readonly BaseUser _base;

        private UserBuilder(BaseUser @base)
        {
            this._base = @base;
        }

        public BaseUser Base { get => _base; }

        public UserBuilder AddRole(string role)
        {
            roles.Add(role);
            return this;
        }

        public async Task<IdentityResult> Create(UserManager<BaseUser> manager, string pwd)
        {
            var foundUser = await manager.FindByEmailAsync(_base.Email!);

            if (foundUser == null)
            {
                var res = await manager.CreateAsync(_base, pwd);
                if (res.Succeeded)
                    return await manager.AddToRolesAsync(_base, roles);
                return res;
            }
            return IdentityResult.Success;
        }

        public static UserBuilder Builder(BaseUser baseUser) => new(baseUser);
    }

    [GeneratedRegex("^(?<name>\\w+)@.*")]
    private static partial Regex MyRegex();
}