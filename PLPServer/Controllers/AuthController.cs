using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using PLPServer.Models;

namespace PLPServer.Controllers;

/// <summary>
/// Login request for user sign in.
/// </summary>
public sealed class LoginRequest {

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

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{

    private readonly UserManager<BaseUser> userManager;
    private readonly SignInManager<BaseUser> signInManager;
    private readonly PLPContext mainDb;
    private readonly ILogger<AuthController> logger;

    public AuthController(UserManager<BaseUser> userManager, PLPContext context, SignInManager<BaseUser> signInManager, ILogger<AuthController> logger)
    {
        this.userManager = userManager;
        mainDb = context;
        this.signInManager = signInManager;
        this.logger = logger;
    }



    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequireHttps]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest login,
        [FromServices] IServiceProvider sp,
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies)
    {
        if (string.IsNullOrEmpty(login.Email))
            return BadRequest("Email is empty");

        if (string.IsNullOrEmpty(login.Password))
            return BadRequest("Password is empty");

        var signInManager = sp.GetRequiredService<SignInManager<BaseUser>>();
        var userManager = sp.GetRequiredService<UserManager<BaseUser>>();

        var useCookiesScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistant = (useCookies == true) && (useSessionCookies != true);

        if (!useCookiesScheme)
            return Unauthorized("token authentication not possible yet");

        signInManager.AuthenticationScheme = useCookiesScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var user = await userManager.FindByEmailAsync(login.Email);

        if (user == null)
            return BadRequest("Email or password incorrect!");


        var res = await signInManager.PasswordSignInAsync(user, login.Password, isPersistant, false);

        if (!res.Succeeded)
        {
            return Unauthorized(res);
        }

        //SignInManager should set the cookie
        return Ok();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        await signInManager.SignOutAsync();
        return Ok();
    }
}