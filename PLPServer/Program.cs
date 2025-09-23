using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PLPServer.Controllers;
using PLPServer.Data;
using PLPServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(opts => { });
builder.Services.AddDbContextFactory<PLPContext>(opt =>
    opt//.UseLazyLoadingProxies()
        .UseNpgsql(builder.Configuration.GetConnectionString("MainDatabase"))

); //or AddDbContext

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ASP.NET Core Identity - cookies and supported, for JWT do something different
builder.Services.AddAuthentication(opt =>
{ });

/*builder.Services.AddAuthorization(opt =>
{
})*/

builder.Services
    // I will keep the role added since Identity doesn't like it when it's not specified :(
    .AddIdentity<BaseUser, BaseRole>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.SignIn.RequireConfirmedEmail = false;
        opt.SignIn.RequireConfirmedAccount = false;
    })
    //.AddRoles<BaseRole>()
    .AddEntityFrameworkStores<PLPContext>() //TODO: possibly separate Auth and everything else
                                            // Adds SignInManager and emailer stuff. Since I will make my own API endpoints i have to add it myself
                                            //.AddApiEndpoints();
    .AddSignInManager()
    // For prop tokens
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/api/login";
    opts.LogoutPath = "/api/logout";
});

var app = builder.Build();

var dotnetWatchVar = Environment.GetEnvironmentVariable("DOTNET_WATCH_ITERATION");
var dotnetWatchCounter = string.IsNullOrEmpty(dotnetWatchVar) ? 0 : int.Parse(dotnetWatchVar);

var isInDebugger = Environment.GetEnvironmentVariable("INDEBUGGER") == "true";
var dbCleanSlate = dotnetWatchCounter == 1 || isInDebugger;

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PLPContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    /*var res = await InitData.CreateRoles(scope.ServiceProvider);
    if (res != IdentityResult.Success)
    {
        app.Logger.LogError("Couldn't create roles! Error: {0}", res.Errors);
        throw new Exception("Couldn't create roles!");
    }*/

    // In debug create database
    if (app.Environment.IsDevelopment())
    {
        if (dbCleanSlate)
        {
            logger.LogInformation("dotnetWatchCounter: {}, isInDebugger: {}", dotnetWatchVar, isInDebugger);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Reinit test data
            await InitData.SeedTestData(scope.ServiceProvider);
        }
    }
    else if (app.Environment.IsProduction())
    {
        await context.Database.MigrateAsync();
    }
}

using (var scope = app.Services.CreateScope())
{
    
}

// After DB init 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(); //remove if stuff is wrong
}

//app.UseHttpsRedirection();

//app.UseRouting(); // maybe remove?

//app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

//ENDPOINTS GO HERE!!

//var apiPrefix = app.MapGroup("/api");

app.MapGroup("/auth").MapAuthEndpoints();
app.MapGroup("/zapisi").MapZapisiEndpoints();

app.MapControllers();

app.Run();
