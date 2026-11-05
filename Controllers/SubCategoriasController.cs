using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Extensions;
using FloresMvc.Models;
using System.Threading.Tasks;


namespace FloresMvc.Controllers
{
    public class SubCategoriasController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public SubCategoriasController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.SubCategorias.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SubCategorias == null)
            {
                return NotFound();
            }
            var subcategorias = await _context.SubCategorias
                .FirstOrDefaultAsync(s => s.IdSubcategoria == id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            return View(subcategorias);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSubcategoria,NomSubcategoria,fecha_Creacion")] SubCategorias subcategorias)
        {
            if (ModelState.IsValid)
            {
                subcategorias.fecha_Creacion = DateTime.Now;
                subcategorias.DateOwner = DateTime.Now;
                subcategorias.Owner = User.ObtenerNombreUsuario();
                _context.Add(subcategorias);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subcategorias);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategorias = await _context.SubCategorias.FindAsync(id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            return View(subcategorias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSubcategoria,NomSubcategoria,fecha_Creacion")] SubCategorias subcategorias)
        {
            if (id != subcategorias.IdSubcategoria)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var subcategoriaDb = await _context.SubCategorias.FindAsync(id);
                if (subcategoriaDb == null)
                {
                    return NotFound();
                }

                try
                {
                    subcategoriaDb.NomSubcategoria = subcategorias.NomSubcategoria;
                    subcategoriaDb.LastModify = User.ObtenerNombreUsuario();
                    subcategoriaDb.LastDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubCategoriaExist(subcategorias.IdSubcategoria))
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
            return View(subcategorias);
        }

        public bool SubCategoriaExist(int id)
        {
            return _context.SubCategorias.Any(s => s.IdSubcategoria == id);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategorias = await _context.SubCategorias
                .FirstOrDefaultAsync(s => s.IdSubcategoria == id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            return View(subcategorias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subcategorias = await _context.SubCategorias.FindAsync(id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            _context.SubCategorias.Remove(subcategorias);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
