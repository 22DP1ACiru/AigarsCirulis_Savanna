using Microsoft.EntityFrameworkCore;
using Savanna.Data;
using Savanna.Data.Interfaces;
using Savanna.Data.Entities;
using Savanna.Core.Interfaces;
using Savanna.Core.Services;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Factories;
using Savanna.Web.Hubs;
using Savanna.Web.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- Database & Identity ---
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- Application Services ---
builder.Services.AddSignalR();  

builder.Services.AddRazorPages();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IGameSaveService, GameSaveService>();

builder.Services.AddSingleton<IAnimalFactory, AnimalFactory>();

builder.Services.AddSingleton<IGameSessionService, GameSessionService>();

builder.Services.AddHostedService<GameLoopService>();

// --- Authentication Configuration ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.Events.OnSignedIn = context =>
    {
        context.Response.Redirect("/Game");
        return Task.CompletedTask;
    };
});

// --- Build App ---
var app = builder.Build();

// --- Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapHub<GameHub>("/gameHub");

app.MapRazorPages();

// --- Run App ---
app.Run();
