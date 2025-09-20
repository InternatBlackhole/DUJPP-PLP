using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
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
/*
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AuthDbContext>();
*/
builder.Services
    .AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddApiEndpoints();


var app = builder.Build();

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
    }
    
}

app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.Run();
