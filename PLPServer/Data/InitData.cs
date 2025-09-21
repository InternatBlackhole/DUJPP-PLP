using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.VisualBasic;
using PLPServer.Models;

namespace PLPServer.Data;

public static class InitData
{
    public static async Task<IdentityResult> CreateRoles(this IServiceProvider provider)
    {
        var roleManager = provider.GetService<RoleManager<IdentityRole>>() ?? throw new Exception("no roleManager!");

        foreach (var role in new[] { Roles.Prevoznik, Roles.Inspektor, Roles.Admin })
        {
            IdentityResult r;
            if (!await roleManager.RoleExistsAsync(role.ToString()))
            {
                r = await roleManager.CreateAsync(new IdentityRole(role.ToString()));
                if (r != IdentityResult.Success)
                    return r;
            }
        }
        return IdentityResult.Success;
    }

    public static async Task SeedTestUsers(this IServiceProvider provider)
    {
        var userManager = provider.GetRequiredService<UserManager<BaseUser>>();

        BaseUser UserInit(string email)
        {
            return new BaseUser()
            {
                Email = email
            };
        }

        var admins = new Dictionary<BaseUser, string>
        {
            { UserInit("admin@test.com"), "adminTest" },
        };

        var prevozniki = new Dictionary<BaseUser, string>
        {
            { UserInit("prevoznik1@test.com"), "prevoznik1"},
            { UserInit("prevoznik2@test.com"), "prevoznik1"},
            { UserInit("prevoznik3@test.com"), "prevoznik1"},
        };

        var inspektorji = new Dictionary<BaseUser, string>
        {
            { UserInit("inspektor1@test.com"), "inspektor"},
            { UserInit("inspektor2@test.com"), "inspektor"},
        };

        var users = new Dictionary<string, Dictionary<BaseUser, string>>
        {
            {Roles.Admin, admins},
            {Roles.Prevoznik, prevozniki },
            {Roles.Inspektor, inspektorji},
        };

        List<Task<IdentityResult>> tasks = new List<Task<IdentityResult>>();

        foreach (var (role, roleUsers) in users)
        {
            foreach (var (user, pwd) in roleUsers)
            {
                tasks.Add(UserBuilder.Builder(user).AddRole(role).Create(userManager, pwd));
            }
        }

        await Task.WhenAll(tasks);
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