using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using System.Data;
using System.ComponentModel.DataAnnotations;
using FloresMvc.Models;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FloresMvc.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());

        }

        //EndPoints de  ususarios
        [HttpGet]
        public async Task<IActionResult> GetUsuariosJson()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Json(usuarios);
        }
        [HttpPost]
        public async Task<IActionResult> ActualizarCelda([FromBody] Usuarios usuarioModificado)
        {
            if (usuarioModificado == null)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }
            var usuarioDb = await _context.Usuarios.FindAsync(usuarioModificado.IdUsuarios);
            if (usuarioDb == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado" });
            }

            usuarioDb.Nombre = usuarioModificado.Nombre;
            usuarioDb.Apellido = usuarioModificado.Apellido;
            usuarioDb.email = usuarioModificado.email;

            _context.Update(usuarioDb);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "actualizacion correcta" });
        }
        //EndPoints de  ususarios
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }

            var Usuarios = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuarios == id);

            if (Usuarios == null)
            {
                return NotFound();
            }
            return View(Usuarios);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuarios,IdRol,Nombre,Apellido,fecha_creacion,email")] Usuarios usuarios)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuarios);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuarios);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuarios,IdRol,Nombre,Apellido,fecha_creacion")] Usuarios usuarios)
        {
            if (id != usuarios.IdUsuarios)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarios);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuariosExixst(usuarios.IdUsuarios))
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
            return View(usuarios);
        }
        public bool UsuariosExixst(int id)
        {
            return _context.Usuarios.Any(u => u.IdUsuarios == id);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var usuarios = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuarios == id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            _context.Usuarios.Remove(usuarios);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}