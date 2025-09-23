
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Npgsql.TypeMapping;
using PLPServer.Models;

namespace PLPServer.Controllers;

public static class Zapisi
{

    public static IEndpointRouteBuilder MapZapisiEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("", All);
        builder.MapPost("", AddZapis);

        builder.MapGet("optimized", AllOptimized);

        builder.MapGet("{id}", GetById);
        builder.MapPatch("{id}", EditZapis);
        builder.MapDelete("{id}", DeleteZapis);

        return builder;
    }

    [Authorize]
    private static async Task<Results<ForbidHttpResult, NotFound, NoContent>> DeleteZapis(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromRoute] Guid id
    )
    {
        var logger = loggerFact.CreateLogger(nameof(DeleteZapis));

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        switch (user)
        {
            case Prevoznik:
            case Administrator:
                break;
            case Inspektor:
            default:
                return TypedResults.Forbid();
        }

        var zapis = await mainDb.Zapisi.Include(z => z.Pogodba).FirstOrDefaultAsync(z => z.Id == id);

        if (zapis == null)
            return TypedResults.NotFound();

        if (zapis.Pogodba!.PrevoznikId != user.Id)
            return TypedResults.Forbid();

        mainDb.Zapisi.Remove(zapis);
        await mainDb.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    [Authorize]
    private static async Task<Results<ForbidHttpResult, BadRequest<string>, Ok<ChangedZapis>, NotFound>> EditZapis(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromBody] ZapisPut post,
        [FromRoute] Guid id
    )
    {
        var logger = loggerFact.CreateLogger(nameof(EditZapis));

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        switch (user)
        {
            case Prevoznik:
            case Administrator:
                break;
            case Inspektor:
            default:
                return TypedResults.Forbid();
        }

        var current = await mainDb.Zapisi.Where(z => z.Id == id && z.Pogodba!.PrevoznikId == user.Id).SingleOrDefaultAsync();

        if (current == null)
            return TypedResults.NotFound();

        if (post.ZacetekVoznje != null && post.KonecVoznje != null && post.ZacetekVoznje > post.KonecVoznje)
            return TypedResults.BadRequest("Given ZacetekVoznje is later than given KonecVoznje!");

        if (post.ZacetekVoznje != null)
        {
            //if (post.ZacetekVoznje > current.KonecVoznje)
            //    return TypedResults.BadRequest("new ZacetekVoznje is later than current KonecVoznje!");
            current.ZacetekVoznje = (DateTime)post.ZacetekVoznje;
        }

        if (post.KonecVoznje != null)
        {
            /*if (post.KonecVoznje < current.ZacetekVoznje)
                return TypedResults.BadRequest("new KonecVoznje is later than current ZacetekVoznje!");*/
            current.KonecVoznje = (DateTime)post.KonecVoznje;
        }

        if (current.ZacetekVoznje > current.KonecVoznje)
            return TypedResults.BadRequest("new ZacetekVoznje is later than new KonecVoznje!");

        await mainDb.SaveChangesAsync();

        return TypedResults.Ok(new ChangedZapis()
        {
            Id = current.Id,
            ZacetekVoznje = current.ZacetekVoznje,
            KonecVoznje = current.KonecVoznje,
        });
    }

    [Authorize]
    private static async Task<Results<Created<NewZapis>, BadRequest<string>, ForbidHttpResult, UnauthorizedHttpResult>> AddZapis(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromBody] ZapisPost post
    )
    {
        var logger = loggerFact.CreateLogger(nameof(AddZapis));

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        if (user is not Prevoznik)
        {
            return TypedResults.Forbid();
        }

        Prevoznik prevoznik = (Prevoznik)user;

        var isPogodbaCorrect = await mainDb.Pogodbe.AnyAsync(p => p.Id == post.PogodbaId && p.PrevoznikId == prevoznik.Id);
        if (!isPogodbaCorrect)
        {
            //logger.LogInformation("PogodbaId is not applicable to this user!");
            return TypedResults.Forbid();
        }

        if (post.ZacetekVoznje > post.KonecVoznje)
            return TypedResults.BadRequest("ZacetekVoznje is after KonecVoznje!");

        var zapis = new Models.Zapis()
        {
            PogodbaId = post.PogodbaId,
            ZacetekVoznje = post.ZacetekVoznje,
            KonecVoznje = post.KonecVoznje,
        };

        await mainDb.AddAsync(zapis);
        await mainDb.SaveChangesAsync();

        return TypedResults.Created(
            $"zapisi/{zapis.Id}",
            new NewZapis()
            {
                Id = zapis.Id
            });
    }

    [Authorize]
    private static async Task<Results<BadRequest<string>, NotFound, UnauthorizedHttpResult, Ok<Zapis>, ForbidHttpResult>> GetById(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromRoute] Guid id
    )
    {
        var logger = loggerFact.CreateLogger(nameof(GetById));

        /*if (context.User.Identity?.IsAuthenticated != true)
            return TypedResults.Unauthorized();*/

        var dbUser = await userManager.GetUserAsync(context.User);

        if (dbUser == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        if (dbUser is Prevoznik)
        {
            var isDoneByMe = await mainDb.Zapisi.AnyAsync(z => z.Id == id && z.Pogodba!.PrevoznikId == dbUser.Id);
            if (!isDoneByMe)
                return TypedResults.Forbid();
        }
        else if (dbUser is Inspektor || dbUser is Administrator)
        {
        }
        else
        {
            logger.LogError("A user with type {} tried to access an endpoint. Have you forgetten about a new user?", dbUser.GetType());
            return TypedResults.Forbid();
        }

        var zapis = await mainDb.Zapisi
            .Where(z => z.Id == id)
            .Select(z => new Zapis()
            {
                Id = z.Id,
                ZacetekVoznje = z.ZacetekVoznje,
                KonecVoznje = z.KonecVoznje,
                PogodbaId = z.PogodbaId
            })
            .FirstOrDefaultAsync();
        return TypedResults.Ok(zapis);

    }

    [Authorize]
    public static async Task<Results<Ok<List<Zapis>>, BadRequest<string>>> All(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromQuery] int startIndex = 0,
        [FromQuery] int limit = 10
    )
    {
        var logger = loggerFact.CreateLogger(context.Request.Path);

        if (limit < 0)
            return TypedResults.BadRequest("limit os lower than 0!");

        // should not be null if we are authenticated
        var dbUser = await userManager.GetUserAsync(context.User);

        if (dbUser == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        //TODO: simplify, just return the whole Zapis object and make the frontend issue more api requests
        //Or expose this as a new endpoint that makes it easier for frontend, since less data transfer required

        var zapisi = dbUser switch
        {
            Prevoznik p =>
                mainDb.Zapisi.Where(z => z.Pogodba!.PrevoznikId == p.Id),

            // Inšpektor in admin vidita enako
            Inspektor or Administrator => mainDb.Zapisi.AsQueryable(),
            _ => throw new Exception("You forgot to add a user!")
        };
        var ret = zapisi.Select(z => new Zapis()
        {
            Id = z.Id,
            ZacetekVoznje = z.ZacetekVoznje,
            KonecVoznje = z.KonecVoznje,
            PogodbaId = z.PogodbaId
        });
        var res = await BuildResultAsync(ret, startIndex, limit);
        return TypedResults.Ok(res);
    }

    [Authorize]
    public static async Task<Results<Ok<List<MainPageZapis>>, BadRequest<string>, UnauthorizedHttpResult>> AllOptimized(
        HttpContext context,
        [FromServices] UserManager<BaseUser> userManager,
        [FromServices] ILoggerFactory loggerFact,
        [FromServices] PLPContext mainDb,
        [FromQuery] int startIndex = 0,
        [FromQuery] int limit = 10
    )
    {
        var logger = loggerFact.CreateLogger(context.Request.Path);

        if (limit < 0)
            return TypedResults.BadRequest("limit os lower than 0!");

        // should not be null if we are authenticated
        var dbUser = await userManager.GetUserAsync(context.User);

        if (dbUser == null)
        {
            logger.LogError("dbUser is somehow null?");
            throw new NullReferenceException("dbUser is null!");
        }

        //TODO: simplify, just return the whole Zapis object and make the frontend issue more api requests
        //Or expose this as a new endpoint that makes it easier for frontend, since less data transfer required

        var zapisi = dbUser switch
        {
            Prevoznik p =>
                mainDb.Zapisi.Where(z => z.Pogodba!.PrevoznikId == p.Id),

            // Inšpektor in admin vidita enako
            Inspektor or Administrator => mainDb.Zapisi.AsQueryable(),
            _ => throw new Exception("You forgot to add a user!")
        };

        var ret = zapisi.Select(z => new MainPageZapis()
        {
            ZacetekVoznje = z.ZacetekVoznje,
            KonecVoznje = z.KonecVoznje,
            NazivLinije = z.Pogodba!.Linija!.Ime ?? "",
            NazivNarocnika = z.Pogodba.Linija.Narocnik!.Ime ?? "",
            ZnesekPogodbe = z.Pogodba.Znesek,
            LinijaId = z.Pogodba.LinijaId,
            NarocnikId = z.Pogodba.Linija.Narocnik.Id
        });

        var res = await BuildResultAsync(ret, startIndex, limit);
        return TypedResults.Ok(res);
    }

    private static async Task<List<TSort>> BuildResultAsync<TSort>(
        IQueryable<TSort> query,
        int startIndex,
        int limit
    ) where TSort : IHasZacetekVoznje
    {
        var ordered = query.OrderByDescending(e => e.ZacetekVoznje);

        if (startIndex == -1)
            return await ordered.ToListAsync();

        return await ordered.Skip(startIndex).Take(limit).ToListAsync();
    }

    private record NewZapis
    {
        public Guid Id { get; set; }
    }

    public interface IHasZacetekVoznje
    {
        DateTime ZacetekVoznje { get; }
    }

    public record Zapis : IHasZacetekVoznje
    {
        public Guid Id { get; set; }
        public Guid PogodbaId { get; set; }
        public DateTime ZacetekVoznje { get; set; }
        public DateTime KonecVoznje { get; set; }
    }

    public record ChangedZapis
    {
        public Guid Id { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime ZacetekVoznje { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime KonecVoznje { get; set; }
    }

    public record ZapisPut
    {
        [DataType(DataType.DateTime)]
        public DateTime? ZacetekVoznje { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? KonecVoznje { get; set; }
    }

    public record ZapisPost
    {
        [Required]
        public Guid PogodbaId { get; set; }

        [DataType(DataType.DateTime)]
        [Required]
        public DateTime ZacetekVoznje { get; set; }

        [DataType(DataType.DateTime)]
        [Required]
        public DateTime KonecVoznje { get; set; }
    }

    public record MainPageZapis : IHasZacetekVoznje
    {
        public DateTime ZacetekVoznje { get; set; }
        public DateTime KonecVoznje { get; set; }
        public required Guid LinijaId { get; set; }
        public required string NazivLinije { get; set; }
        public required Guid NarocnikId { get; set; }
        public required string NazivNarocnika { get; set; }
        public required double ZnesekPogodbe { get; set; }
    }
}