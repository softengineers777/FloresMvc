using System;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FloresMvc.Models;
using FloresMvc.Services;

namespace FloresMvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;
        private readonly ILogger<LoginController> _logger;
        private readonly IWebHostEnvironment _environment;

        public LoginController(
            FloresDbContext context,
            IConfiguration configuration,
            PasswordService passwordService,
            ILogger<LoginController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _passwordService = passwordService;
            _logger = logger;
            _environment = environment;
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

            // Creamos la sesión del usuario con sus datos básicos y permisos del rol
            var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRol == usuario.IdRol);

            var permisos = await _context.RolPermisos
                .AsNoTracking()
                .Where(rp => rp.IdRol == usuario.IdRol)
                .Select(rp => rp.Permiso!.Clave)
                .Distinct()
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuarios.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}".Trim()),
                new Claim(ClaimTypes.Email, usuario.email ?? string.Empty),
                new Claim(ClaimTypes.Role, rol?.Nombre ?? "SinRol")
            };

            foreach (var clave in permisos)
            {
                if (!string.IsNullOrWhiteSpace(clave))
                {
                    claims.Add(new Claim("permiso", clave));
                }
            }

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
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Respuesta genérica para no revelar si el correo existe.
            const string mensajeGenerico = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.";

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Ingresa tu correo electrónico.";
                return View();
            }

            var correo = email.Trim().ToLower();
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.email.ToLower() == correo);

            if (usuario != null)
            {
                var loginRecord = await _context.Login
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.user_id == usuario.IdUsuarios && l.activo);

                if (loginRecord != null)
                {
                    var rawToken = GenerarTokenSeguro();
                    var tokenHash = HashToken(rawToken);
                    var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                    // Invalida tokens anteriores no usados del mismo usuario.
                    var anteriores = await _context.PasswordResetTokens
                        .Where(t => t.UserId == usuario.IdUsuarios && t.UsedAt == null)
                        .ToListAsync();
                    foreach (var t in anteriores)
                        t.UsedAt = ahora;

                    _context.PasswordResetTokens.Add(new PasswordResetToken
                    {
                        UserId = usuario.IdUsuarios,
                        TokenHash = tokenHash,
                        CreatedAt = ahora,
                        ExpiresAt = ahora.AddHours(1),
                        UsedAt = null
                    });
                    await _context.SaveChangesAsync();

                    var resetUrl = Url.Action("ResetPassword", "Login", new { token = rawToken }, Request.Scheme);
                    _logger.LogInformation("Enlace de restablecimiento de contraseña para {Email}: {Url}", correo, resetUrl);

                    // TODO: integrar envío real de correo (SMTP/SendGrid/etc.) usando la URL generada.
                    if (_environment.IsDevelopment() && !string.IsNullOrEmpty(resetUrl))
                        ViewBag.ResetLink = resetUrl;
                }
            }

            ViewBag.Success = mensajeGenerico;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "El enlace de restablecimiento no es válido.";
                return View(model: null);
            }

            var tokenValido = await ObtenerTokenValidoAsync(token);
            if (tokenValido == null)
            {
                ViewBag.Error = "El enlace de restablecimiento no es válido o ya expiró. Solicita uno nuevo.";
                return View(model: null);
            }

            ViewBag.Token = token;
            return View(model: null);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string passwordNueva, string confirmarPassword)
        {
            ViewBag.Token = token;

            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "El enlace de restablecimiento no es válido.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(passwordNueva) || string.IsNullOrWhiteSpace(confirmarPassword))
            {
                ViewBag.Error = "Por favor completa todos los campos.";
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

            var tokenRecord = await ObtenerTokenValidoAsync(token);
            if (tokenRecord == null)
            {
                ViewBag.Error = "El enlace de restablecimiento no es válido o ya expiró. Solicita uno nuevo.";
                ViewBag.Token = null;
                return View();
            }

            var loginRecord = await _context.Login.FirstOrDefaultAsync(l => l.user_id == tokenRecord.UserId);
            if (loginRecord == null)
            {
                ViewBag.Error = "No se encontró la credencial del usuario. Contacta al administrador.";
                ViewBag.Token = null;
                return View();
            }

            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            loginRecord.password_hash = _passwordService.Hash(passwordNueva);
            loginRecord.LastModify = "ResetToken";
            loginRecord.LastDate = ahora;

            tokenRecord.UsedAt = ahora;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Tu contraseña fue restablecida correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<PasswordResetToken?> ObtenerTokenValidoAsync(string rawToken)
        {
            var tokenHash = HashToken(rawToken);
            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > ahora);
        }

        private static string GenerarTokenSeguro()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
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




