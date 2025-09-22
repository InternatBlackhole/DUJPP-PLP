using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.VisualBasic;
using PLPServer.Models;

namespace PLPServer.Data;

public static class InitData
{
    public static async Task<IdentityResult> CreateRoles(this IServiceProvider provider)
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

    public static async Task SeedTestUsers(this IServiceProvider provider)
    {
        var userManager = provider.GetRequiredService<UserManager<BaseUser>>();

        TUser UserInit<TUser>(string email, Action<TUser>? action = null) where TUser : BaseUser, new()
        {
            var match = Regex.Match(email, "^(?<name>\\w+)@.*");
            var name = match.Groups["name"].Value;

            var usr = new TUser()
            {
                Email = email,
                UserName = name
            };

            action?.Invoke(usr);
            return usr;
        }

        var admins = new Dictionary<BaseUser, string>
        {
            { UserInit<Administrator>("admin@test.com"), "0adminTest;" },
        };

        var prevozniki = new Dictionary<BaseUser, string>
        {
            { UserInit<Prevoznik>("prevoznik1@test.com"), "Prevoznik1!"},
            { UserInit<Prevoznik>("prevoznik2@test.com"), "Prevoznik1!"},
            { UserInit<Prevoznik>("prevoznik3@test.com"), "Prevoznik1!"},
        };

        var inspektorji = new Dictionary<BaseUser, string>
        {
            { UserInit<Inspektor>("inspektor1@test.com"), "Inspekt0r!"},
            { UserInit<Inspektor>("inspektor2@test.com"), "Inspekt0r!"},
        };

        var users = new Dictionary<string, Dictionary<BaseUser, string>>
        {
            {Roles.Admin, admins},
            {Roles.Prevoznik, prevozniki },
            {Roles.Inspektor, inspektorji},
        };

        foreach (var (role, roleUsers) in users)
        {
            foreach (var (user, pwd) in roleUsers)
            {
                var res = await UserBuilder.Builder(user).AddRole(role).Create(userManager, pwd);
            }
        }
    }

    public class UserBuilder
    {

        private readonly List<string> roles = new List<string>();

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

        public static UserBuilder Builder(BaseUser baseUser) => new UserBuilder(baseUser);
    }
}