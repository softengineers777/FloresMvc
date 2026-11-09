using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FloresMvc.Models;
using FloresMvc.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FloresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Hash de contraseñas: el PasswordHasher de ASP.NET Core usa PBKDF2 con sal
// aleatoria, así no se almacenan contraseñas en texto plano.
builder.Services.AddScoped<IPasswordHasher<Login>, PasswordHasher<Login>>();
builder.Services.AddScoped<PasswordService>();

// Autenticación por cookie: el usuario debe iniciar sesión antes de
// poder acceder a cualquier pantalla de la aplicación.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login";
        options.Cookie.Name = "FloresMvc.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                // Revalidación periódica: detecta usuarios inactivados sin consultar la BD en cada request.
                var interval = TimeSpan.FromMinutes(2);
                var issuedUtc = context.Properties.IssuedUtc;
                if (issuedUtc.HasValue && DateTimeOffset.UtcNow - issuedUtc.Value < interval)
                    return;

                var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdValue, out var userId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<FloresDbContext>();
                var activo = await db.Usuarios
                    .AsNoTracking()
                    .Where(u => u.IdUsuarios == userId)
                    .Select(u => u.Activo)
                    .FirstOrDefaultAsync();

                var loginActivo = await db.Login
                    .AsNoTracking()
                    .Where(l => l.user_id == userId)
                    .Select(l => (bool?)l.activo)
                    .FirstOrDefaultAsync();

                // Sin registro o inactivo => se invalida la sesión.
                if (activo != true || loginActivo != true)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                // Marca la última validación para no consultar la BD en cada request.
                context.ShouldRenew = true;
                context.Properties.IssuedUtc = DateTimeOffset.UtcNow;
            }
        };
    });

// Toda la aplicación queda protegida por defecto. Las acciones de login
// se marcan con [AllowAnonymous] para permitir el acceso.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
