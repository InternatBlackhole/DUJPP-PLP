using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using PLPServer.Data;
using PLPServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<PLPContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabase"));
}); //or AddDbContext

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

// In debug create database
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PLPContext>();

    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

using (var scope = app.Services.CreateScope())
{
    var res = await scope.ServiceProvider.CreateRoles();
    if (res != IdentityResult.Success)
    {
        app.Logger.LogError("Couldn't create roles! Error: {0}", res.Errors);
        throw new Exception("Couldn't create roles!");
    }

    if (app.Environment.IsDevelopment())
    {
        await scope.ServiceProvider.SeedTestUsers();
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
