using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using FloresMvc.Extensions;
using FloresMvc.Models;


namespace FloresMvc.Controllers

{
    public class FincasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FloresDbContext _context;

        public FincasController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Fincas.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Fincas == null)
            {
                return NotFound();
            }
            var fincas = await _context.Fincas
            .FirstOrDefaultAsync(f => f.IdFinca == id);
            if (fincas == null)
            {
                return NotFound();
            }
            return View(fincas);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFinca,NomFinca")] Fincas fincas)
        {
            if (ModelState.IsValid)
            {
                fincas.fecha_Creacion = DateTime.Now;
                fincas.Owner = User.ObtenerNombreUsuario();
                fincas.DateOwner = DateTime.Now;

                _context.Add(fincas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fincas);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var fincas = await _context.Fincas.FindAsync(id);

            if (fincas == null)
            {
                return NotFound();
            }
            return View(fincas);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFinca,NomFinca")] Fincas fincas)
        {
            if (id != fincas.IdFinca)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var fincaDb = await _context.Fincas.FindAsync(id);
                if (fincaDb == null)
                {
                    return NotFound();
                }

                try
                {
                    fincaDb.NomFinca = fincas.NomFinca;
                    fincaDb.LastModify = User.ObtenerNombreUsuario();
                    fincaDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FincasExist(fincas.IdFinca))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));

            }
            return View(fincas);

        }
        public bool FincasExist(int id)
        {
            return _context.Fincas.Any(f => f.IdFinca == id);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var fincas = await _context.Fincas
            .FirstOrDefaultAsync(f => f.IdFinca == id);

            if (fincas == null)
            {
                return NotFound();
            }
            return View(fincas);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fincas = await _context.Fincas.FindAsync(id);

            if (fincas == null)
            {
                return NotFound();
            }
            _context.Remove(fincas);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}

