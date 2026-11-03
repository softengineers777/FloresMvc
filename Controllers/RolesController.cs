using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Controllers;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class RolesController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public RolesController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
                public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }
         public async Task<IActionResult> GetRolesJson()
        {
            var roles = await _context.Roles.ToListAsync();
            return Json(roles);
        }
        [HttpPost]
        public async Task<IActionResult> ActualizarCelda([FromBody] Roles rolModificado)
        {
            if (rolModificado == null)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }
            if (string.IsNullOrWhiteSpace(rolModificado.Nombre))
            {
                return BadRequest(new { success = false, message = "El nombre del rol es obligatorio." });
            }

            var rolDb = await _context.Roles.FindAsync(rolModificado.IdRol);
            if (rolDb == null)
            {
                return NotFound(new { success = false, message = "Rol no encontrado" });
            }

            rolDb.Nombre = rolModificado.Nombre.Trim();
            rolDb.LastModify = "Admin";
            rolDb.LastDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Actualización correcta",
                lastModify = rolDb.LastModify,
                lastDate = rolDb.LastDate
            });
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }
            var Roles = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRol == id);
            if (Roles == null)
            {
                return NotFound();
            }
            return View(Roles);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRol,Nombre")] Roles roles)
        {
            if (ModelState.IsValid)
            {
                roles.fecha_Creacion = DateTime.UtcNow;
                roles.Owner = "Admin";
                roles.DateOwner = DateTime.Now;
                _context.Add(roles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Roles rol)
        {
            if (rol == null)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }
            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                return BadRequest(new { success = false, message = "El nombre del rol es obligatorio." });
            }

            rol.Nombre = rol.Nombre.Trim();
            rol.fecha_Creacion = DateTime.UtcNow;
            rol.Owner = "Admin";
            rol.DateOwner = DateTime.Now;
            rol.LastModify = null;
            rol.LastDate = null;

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Rol creado con éxito",
                idRol = rol.IdRol
            });
        }

                [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var roles = await _context.Roles.FindAsync(id);
            if (roles == null)
            {
                return NotFound();
            }
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRol,Nombre")] Roles roles)
        {
            if (id != roles.IdRol)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var rolDb = await _context.Roles.FindAsync(id);
                    if (rolDb == null)
                    {
                        return NotFound();
                    }

                    rolDb.Nombre = roles.Nombre;
                    rolDb.LastModify = "Admin";
                    rolDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolesExist(roles.IdRol))
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
            return View(roles);
        }
        public bool RolesExist(int id)
        {
            return _context.Roles.Any(r => r.IdRol == id);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var roles = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRol == id);
            if (roles == null)
            {
                return NotFound();
            }
            return View(roles);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roles = await _context.Roles.FindAsync(id);
            if (roles == null)
            {
                return NotFound();
            }
            _context.Roles.Remove(roles);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
