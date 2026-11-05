using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class InventarioController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public InventarioController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Inventario
        public async Task<IActionResult> Index()
        {
            return View(await _context.Inventario.ToListAsync());
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Inventario == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.IdInventario == id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // GET: Inventario/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdInventario,Nombre,Descripcion,Cantidad,Precio")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                inventario.fecha_Creacion = DateTime.Now;
                inventario.Owner = User.ObtenerNombreUsuario();
                inventario.DateOwner = DateTime.Now;

                _context.Add(inventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(inventario);
        }

        // GET: Inventario/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario.FindAsync(id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("IdInventario,Nombre,Descripcion,Cantidad,Precio")] Inventario inventario)
        {
            if (id != inventario.IdInventario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var inventarioDb = await _context.Inventario.FindAsync(id);
                if (inventarioDb == null)
                {
                    return NotFound();
                }

                try
                {
                    // Solo se actualizan los campos editables: la auditoría de
                    // creación (fecha_Creacion, Owner, DateOwner) se conserva.
                    inventarioDb.Nombre = inventario.Nombre;
                    inventarioDb.Descripcion = inventario.Descripcion;
                    inventarioDb.Cantidad = inventario.Cantidad;
                    inventarioDb.Precio = inventario.Precio;
                    inventarioDb.LastModify = User.ObtenerNombreUsuario();
                    inventarioDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventarioExists(inventario.IdInventario))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(inventario);
        }

        // GET: Inventario/Delete/5
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.IdInventario == id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // POST: Inventario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventario = await _context.Inventario.FindAsync(id);

            if (inventario == null)
            {
                return NotFound();
            }

            _context.Inventario.Remove(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool InventarioExists(int id)
        {
            return _context.Inventario.Any(i => i.IdInventario == id);
        }
    }
}
