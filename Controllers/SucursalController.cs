using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Controllers;
using FloresMvc.Extensions;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class SucursalController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public SucursalController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sucursal.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sucursal == null)
            {
                return NotFound();
            }
            var sucursal = await _context.Sucursal
            .FirstOrDefaultAsync(s => s.IdSucursal == id);
            if (sucursal == null)
            {
                return NotFound();
            }
            return View(sucursal);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSucursal,NombreSuc")] Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                sucursal.fecha_Creacion = DateTime.Now;
                sucursal.DateOwner = DateTime.Now;
                sucursal.Owner = User.ObtenerNombreUsuario();
                _context.Add(sucursal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sucursal = await _context.Sucursal.FindAsync(id);
            if (sucursal == null)
            {
                return NotFound();
            }
            return View(sucursal);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSucursal,NombreSuc")] Sucursal sucursal)
        {
            if (id != sucursal.IdSucursal)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var sucursalDb = await _context.Sucursal.FindAsync(id);
                if (sucursalDb == null)
                {
                    return NotFound();
                }

                try
                {
                    sucursalDb.NombreSuc = sucursal.NombreSuc;
                    sucursalDb.LastModify = User.ObtenerNombreUsuario();
                    sucursalDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SucursalExists(sucursal.IdSucursal))
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
            return View(sucursal);
        }
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sucursal = await _context.Sucursal
                .FirstOrDefaultAsync(s => s.IdSucursal == id);

            if (sucursal == null)
            {
                return NotFound();
            }

            return View(sucursal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sucursal = await _context.Sucursal.FindAsync(id);

            if (sucursal == null)
            {
                return NotFound();
            }

            _context.Sucursal.Remove(sucursal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool SucursalExists(int id)
        {
            return _context.Sucursal.Any(s => s.IdSucursal == id);
        }
    }
}
