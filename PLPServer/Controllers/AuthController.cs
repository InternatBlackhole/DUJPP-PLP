using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PLPServer.Models;

namespace PLPServer.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{

    private readonly UserManager<BaseUser> userManager;
    private readonly SignInManager<BaseUser> signInManager;
    private readonly PLPContext mainDb;

    public AuthController(UserManager<BaseUser> userManager, PLPContext context, SignInManager<BaseUser> signInManager)
    {
        this.userManager = userManager;
        mainDb = context;
        this.signInManager = signInManager;
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        //var user = await userManager.GetUserAsync(User);
        //if (user == null)
        //    return Unauthorized();

        await signInManager.SignOutAsync();
        return Ok();
    }
}