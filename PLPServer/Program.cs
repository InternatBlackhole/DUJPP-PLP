using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

builder.Services
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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PLPContext>();

    // In debug create database
    if (app.Environment.IsDevelopment())
    {
        var dotnetWatchVar = Environment.GetEnvironmentVariable("DOTNET_WATCH_ITERATION");
        var dotnetWatchCounter = string.IsNullOrEmpty(dotnetWatchVar) ? 0 : int.Parse(dotnetWatchVar);

        var isInDebugger = Environment.GetEnvironmentVariable("INDEBUGGER") == "true";

        if (dotnetWatchCounter == 1 || isInDebugger)
            await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
    else if (app.Environment.IsProduction())
    {
        await context.Database.MigrateAsync();
    }
}

using (var scope = app.Services.CreateScope())
{
    var res = await InitData.CreateRoles(scope.ServiceProvider);
    if (res != IdentityResult.Success)
    {
        app.Logger.LogError("Couldn't create roles! Error: {0}", res.Errors);
        throw new Exception("Couldn't create roles!");
    }

    if (app.Environment.IsDevelopment())
    {
        await InitData.SeedTestData(scope.ServiceProvider);
    }
}

// After DB init 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
