using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using FloresMvc.Models;
using FloresMvc.Services;
using Microsoft.EntityFrameworkCore;

namespace FloresMvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;

        public LoginController(
            FloresDbContext context,
            IConfiguration configuration,
            PasswordService passwordService)
        {
            _context = context;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            // Si ya inició sesión, no tiene sentido mostrarle el login otra vez.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Roles");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Autenticar(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Por favor completa todos los campos.";
                return View("Index");
            }

            // Buscamos el usuario por su correo en la tabla Usuarios
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.email.ToLower() == email.Trim().ToLower());

            // Un mensaje genérico evita que un atacante pueda averiguar qué
            // correos están registrados en el sistema.
            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View("Index");
            }

            // Buscamos el registro de login asociado a este usuario
            var loginRecord = await _context.Login.FirstOrDefaultAsync(l => l.user_id == usuario.IdUsuarios);
            if (loginRecord == null || !_passwordService.Verify(loginRecord, password, out var requiereRehash))
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View("Index");
            }

            if (!loginRecord.activo)
            {
                ViewBag.Error = "Este usuario se encuentra inactivo.";
                return View("Index");
            }

            // Si la contraseña estaba en texto plano o con un formato obsoleto,
            // se reemplaza por un hash seguro en este mismo inicio de sesión.
            if (requiereRehash)
            {
                loginRecord.password_hash = _passwordService.Hash(password);
            }

            // La columna "ultimo_acceso" es timestamp without time zone:
            // PostgreSQL/Npgsql espera un DateTime con Kind=Unspecified.
            loginRecord.ultimo_acceso = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Update(loginRecord);
            await _context.SaveChangesAsync();

            // Creamos la sesión del usuario con sus datos básicos
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuarios.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}".Trim()),
                new Claim(ClaimTypes.Email, usuario.email),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
                });

            // Redirigimos al Index de Roles o al Dashboard principal
            return RedirectToAction("Index", "Roles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CambiarPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(
            string passwordActual,
            string passwordNueva,
            string confirmarPassword)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var idUsuario))
            {
                return RedirectToAction(nameof(Index));
            }

            var loginRecord = await _context.Login.FirstOrDefaultAsync(l => l.user_id == idUsuario);
            if (loginRecord == null)
            {
                ViewBag.Error = "No se encontró la credencial del usuario.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(passwordActual) ||
                string.IsNullOrWhiteSpace(passwordNueva) ||
                string.IsNullOrWhiteSpace(confirmarPassword))
            {
                ViewBag.Error = "Por favor completa todos los campos.";
                return View();
            }

            if (!_passwordService.Verify(loginRecord, passwordActual, out _))
            {
                ViewBag.Error = "La contraseña actual no es correcta.";
                return View();
            }

            if (passwordNueva.Length < 6)
            {
                ViewBag.Error = "La nueva contraseña debe tener al menos 6 caracteres.";
                return View();
            }

            if (passwordNueva != confirmarPassword)
            {
                ViewBag.Error = "La nueva contraseña y su confirmación no coinciden.";
                return View();
            }

            if (passwordNueva == passwordActual)
            {
                ViewBag.Error = "La nueva contraseña debe ser diferente a la actual.";
                return View();
            }

            loginRecord.password_hash = _passwordService.Hash(passwordNueva);
            loginRecord.LastModify = User.Identity?.Name;
            loginRecord.LastDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Update(loginRecord);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Contraseña actualizada correctamente.";
            return RedirectToAction(nameof(CambiarPassword));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var Login = await _context.Login
            .FirstOrDefaultAsync(l => l.IdLogin == id);

            if (Login == null)
            {
                return NotFound();
            }
            return View(Login);
        }
                [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLogin,user_id,password_hash,ultimo_acceso,activo")] Login login)
        {
            if (ModelState.IsValid)
            {
                _context.Add(login);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(login);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var login = await _context.Login.FindAsync(id);
            if (login == null)
            {
                return NotFound();
            }
            return View(login);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLogin,user_id,password_hash,ultimo_acceso,activo")] Login login)
        {
            if (id != login.IdLogin)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(login);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoginExists(login.IdLogin))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(login);
        }
        public bool LoginExists(int id)
        {
            return _context.Login.Any(l => l.IdLogin == id);
        }
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var login = await _context.Login
            .FirstOrDefaultAsync(l => l.IdLogin == id);
            if (login == null)
            {
                return NotFound();
            }
            return View(login);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var login = await _context.Login.FindAsync(id);
            if (login == null)
            {
                return NotFound();
            }
            _context.Login.Remove(login);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}




