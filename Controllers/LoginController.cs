using System;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using FloresMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace FloresMvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;
        public LoginController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Autenticar(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Por favor completa todos los campos.";
                return View("Index");
            }

            // Buscamos el usuario por su correo en la tabla Usuarios
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.email == email);
            if (usuario == null)
            {
                ViewBag.Error = "Credenciales incorrectas (Usuario no encontrado).";
                return View("Index");
            }

            // Buscamos el registro de login asociado a este usuario
            var loginRecord = await _context.Login.FirstOrDefaultAsync(l => l.user_id == usuario.IdUsuarios);
            if (loginRecord == null || loginRecord.password_hash != password) // Nota: aquí puedes implementar hashing de contraseña si usas BCrypt o SHA256
            {
                ViewBag.Error = "Contraseña incorrecta.";
                return View("Index");
            }

            if (!loginRecord.activo)
            {
                ViewBag.Error = "Este usuario se encuentra inactivo.";
                return View("Index");
            }

            // Actualizamos último acceso
            loginRecord.ultimo_acceso = DateTime.Now;
            _context.Update(loginRecord);
            await _context.SaveChangesAsync();

            // Redirigimos al Index de Roles o al Dashboard principal
            return RedirectToAction("Index", "Roles");
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




