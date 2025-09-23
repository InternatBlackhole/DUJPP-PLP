
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using PLPServer.Models;

namespace PLPServer.Controllers;

[ApiController]
[Route("/zapisi")]
public class ZapisiController : ControllerBase
{

    private readonly UserManager<BaseUser> userManager;
    private readonly PLPContext mainDb;

    private readonly ILogger<ZapisiController> logger;

    public ZapisiController(UserManager<BaseUser> userManager, PLPContext context, ILogger<ZapisiController> logger)
    {
        this.userManager = userManager;
        this.mainDb = context;
        this.logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MainPageZapisek>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> All(
        [FromQuery] int startIndex = 0,
        [FromQuery] int limit = 10
    )
    {
        if (User.Identity?.IsAuthenticated != true)
            return Unauthorized();

        // should not be null if we are authenticated
        var dbUser = await userManager.GetUserAsync(User);

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
                mainDb.Zapisi.Where(z => z.Pogodba.PrevoznikId == p.Id),

            // Inšpektor in admin vidita enako
            Inspektor or Administrator => mainDb.Zapisi.AsQueryable()
        };
        var res = await zapisi
        .Select(z => new MainPageZapisek()
        {
            ZacetekVoznje = z.ZacetekVoznje,
            KonecVoznje = z.KonecVoznje,
            NazivLinije = z.Pogodba.Linija.Ime,
            NazivNarocnika = z.Pogodba.Linija.Narocnik.Ime,
            ZnesekPogodbe = z.Pogodba.Znesek,
            LinijaId = z.Pogodba.LinijaId,
            NarocnikId = z.Pogodba.Linija.Narocnik.Id
        })
        .OrderByDescending(e => e.ZacetekVoznje)
        .Skip(startIndex)
        .Take(limit)
        .ToListAsync();
        return Ok(res);
    }
}

public record MainPageZapisek
{
    public DateTime ZacetekVoznje { get; set; }
    public DateTime KonecVoznje { get; set; }
    public Guid LinijaId { get; set; }
    public string NazivLinije { get; set; }
    public Guid NarocnikId { get; set; }
    public string NazivNarocnika { get; set; }
    public double ZnesekPogodbe { get; set; }
}