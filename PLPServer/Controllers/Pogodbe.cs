
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PLPServer.Models;

namespace PLPServer.Controllers;

public static class Pogodbe
{
    public static IEndpointRouteBuilder MapPogodbeEndpoints(this IEndpointRouteBuilder builder)
    {

        builder.MapGet("", GetPogodbe);

        return builder;
    }

    [Authorize]
    private static async Task<Results<Ok<List<PogodbaBasic>>, BadRequest<string>>> GetPogodbe(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromQuery] int startIndex = 0,
        [FromQuery] int limit = 10
    )
    {
        var logger = loggerFact.CreateLogger(nameof(GetPogodbe));

        if (limit < 0)
            return TypedResults.BadRequest("limit os lower than 0!");

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        if (limit < 1)
            return TypedResults.BadRequest("Limit can't be less than one!");

        var pogodbe = user switch
        {
            Prevoznik p =>
                mainDb.Pogodbe.Where(po => po.PrevoznikId == p.Id),

            // They can see everything
            Inspektor or Administrator => mainDb.Pogodbe.AsQueryable(),
            _ => throw new Exception("You forgot to add a user!")
        };

        var ret = pogodbe.Select(p => new PogodbaBasic()
        {
            Id = p.Id,
            LinijaId = p.LinijaId,
            PrevoznikId = p.PrevoznikId,
            Znesek = p.Znesek
        });

        if (startIndex == -1)
            return TypedResults.Ok(await ret.ToListAsync());

        var res = await ret.Skip(startIndex).Take(limit).ToListAsync();
        return TypedResults.Ok(res);
    }

    public record PogodbaBasic
    {
        public Guid Id { get; set; }
        public Guid LinijaId { get; set; }
        public Guid PrevoznikId { get; set; }
        public double Znesek { get; set; }
    }
}