using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.DotNet.Scaffolding.Shared.T4Templating;
using PLPServer.Models;

namespace PLPServer.Controllers;

/// <summary>
/// Login request for user sign in.
/// </summary>
public sealed class LoginRequest
{

    /// <summary>
    /// The user's email address
    /// </summary>
    [Required]
    public required string Email { get; init; }

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required]
    public required string Password { get; init; }
}

public static class Authentication
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {

        builder.MapPost("login", Login);

        builder.MapPost("logout", Logout);

        builder.MapGet("whoami", WhoAmI);

        return builder;
    }

    private static async Task<Results<Ok, BadRequest<string>, UnauthorizedHttpResult>> Login(
        [FromBody] LoginRequest login,
        [FromServices] IServiceProvider sp,
        [FromServices]
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies)
    {
        if (string.IsNullOrEmpty(login.Email))
            return TypedResults.BadRequest("Email is empty");

        if (string.IsNullOrEmpty(login.Password))
            return TypedResults.BadRequest("Password is empty");

        var signInManager = sp.GetRequiredService<SignInManager<BaseUser>>();
        var userManager = sp.GetRequiredService<UserManager<BaseUser>>();

        var useCookiesScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistant = (useCookies == true) && (useSessionCookies != true);

        if (!useCookiesScheme)
            return TypedResults.Unauthorized();

        signInManager.AuthenticationScheme = useCookiesScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var user = await userManager.FindByEmailAsync(login.Email);

        if (user == null)
            return TypedResults.BadRequest("Email or password incorrect!");


        var res = await signInManager.PasswordSignInAsync(user, login.Password, isPersistant, false);

        if (!res.Succeeded)
        {
            return TypedResults.Unauthorized();
        }

        //SignInManager should set the cookie
        return TypedResults.Ok();
    }

    public static async Task<Results<UnauthorizedHttpResult, Ok>> Logout(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] SignInManager<BaseUser> signInManager
    )
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
            return TypedResults.Unauthorized();

        await signInManager.SignOutAsync();
        return TypedResults.Ok();
    }

    public static async Task<Results<Ok<UserInfo>, UnauthorizedHttpResult>> WhoAmI(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager
    )
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new UserInfo()
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
        });
    }

    public record UserInfo
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
    }
}