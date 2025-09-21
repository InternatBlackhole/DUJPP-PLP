using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using PLPServer.Data;
using PLPServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<PLPContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabase"))); //or AddDbContext

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ASP.NET Core Identity - cookies and supported, for JWT do something different
builder.Services.AddDbContext<AuthDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("AuthDatabase")));
builder.Services.AddAuthentication();

builder.Services
    .AddIdentityCore<BaseUser>()
    .AddRoles<BaseRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddApiEndpoints();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var res = await scope.ServiceProvider.CreateRoles();
    if (res != IdentityResult.Success)
    {
        app.Logger.LogError("Couldn't create roles! Error: {0}", res.Errors);
        throw new Exception("Couldn't create roles!");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<PLPContext>();
        context.Database.EnsureCreated();

        var context2 = services.GetRequiredService<AuthDbContext>();
        context2.Database.EnsureCreated();

        await services.SeedTestUsers();
    }

}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGroup("/api/auth").MapIdentityApi<BaseUser>();

app.Run();
