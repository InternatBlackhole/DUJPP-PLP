

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PLPServer.Models;

namespace PLPServer.Controllers;

public static class Linije
{
    public static IEndpointRouteBuilder MapLinijeEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("", GetAllLinije);

        return builder;
    }

    [Authorize]
    private static async Task<Results<Ok<List<LinijaBasic>>, ForbidHttpResult, BadRequest<string>>> GetAllLinije(
        HttpContext context,
        [FromServices] PLPContext mainDb,
        [FromServices] ILoggerFactory loggerFactory,
        [FromServices] UserManager<BaseUser> userManager,
        [FromQuery] int startIndex = 0,
        [FromQuery] int limit = 10
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(GetAllLinije));

        /*var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }*/

        if (limit < 1)
            return TypedResults.BadRequest("Limit can't be less than one!");

        var q = mainDb.Linije.Select(l => new LinijaBasic()
        {
            Id = l.Id,
            Ime = l.Ime,
            NarocnikId = l.NarocnikId
        });

        if (startIndex == -1)
            return TypedResults.Ok(await q.ToListAsync());

        return TypedResults.Ok(await q.Skip(startIndex).Take(limit).ToListAsync());
    }

    public record LinijaBasic
    {
        public Guid Id { get; set; }
        public Guid NarocnikId { get; set; }
        public required string Ime { get; set; }
    }
}