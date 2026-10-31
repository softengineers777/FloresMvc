using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using FloresMvc.Models;
using System.Threading.Tasks;


namespace FloresMvc.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public CategoriasController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categorias == null)
            {
                return NotFound();
            }
            var Categorias = await _context.Categorias
            .FirstOrDefaultAsync(c => c.IdCategoria == id);
            if (Categorias == null)
            {
                return NotFound();
            }
            return View(Categorias);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult>Create([Bind("IdCategoria,Nomcategoria,fecha_Creacion")]Categorias categorias)
        {
            if(ModelState.IsValid)
            {
                _context.Add(categorias);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categorias);
        }
        [HttpGet]
        public async Task <IActionResult>Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var categorias = await _context.Categorias.FindAsync(id);
            if(categorias == null)
            {
                return NotFound();
            }
            return View (categorias);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult>Edit(int id,[Bind("IdCategoria,Nomcategoria,fecha_Creacion")]Categorias categorias)
        {
            if(id != categorias.IdCategoria)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(categorias);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (CategoriaExist(categorias.IdCategoria))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                RedirectToAction(nameof(Index));
            }
            return View(categorias);
        }
        public bool CategoriaExist (int id)
        {
            return _context.Categorias.Any(c=> c.IdCategoria == id);
        }
        [HttpGet, ActionName("Delete")]
        public async Task <IActionResult>Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var categorias = await _context.Categorias
            .FirstOrDefaultAsync(c=> c.IdCategoria == id);
            if (categorias == null)
            {
                return NotFound();
            }
            return View(categorias);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult>DeleteConfirmed(int id)
        {
            var categorias = await _context.Categorias.FindAsync(id);
            if (categorias == null)
            {
                return NotFound();
            }
            _context.Categorias.Remove(categorias);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }


}