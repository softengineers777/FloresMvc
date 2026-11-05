using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Controllers;
using FloresMvc.Extensions;
using FloresMvc.Models;



namespace FloresMvc.Controllers
{
    public class AreasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FloresDbContext _context;
        public AreasController (FloresDbContext context, IConfiguration configuration)
        {
            _configuration =  configuration;
            _context = context;
        }
        public async Task<IActionResult> Index ()
        {
            return View (await _context.Areas.ToListAsync());
        }
        public async Task<IActionResult>Details(int? id)
        {
            if(id == null || _context.Areas == null)
            {
                return NotFound();
            }
            var Areas = await _context.Areas
            .FirstOrDefaultAsync(a => a.IdArea == id);
            if (Areas == null)
            {
                 return NotFound();
            }
            return View(Areas);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult>Create([Bind("IdArea,NomArea")]Areas areas)
        {
            if (ModelState.IsValid)
            {
                areas.fecha_Creacion = DateTime.Now;
                areas.DateOwner = DateTime.Now;
                areas.Owner = User.ObtenerNombreUsuario();
                _context.Add(areas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(areas);
        }
        [HttpGet]
        public async Task<IActionResult>Edit (int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var areas = await _context.Areas.FindAsync(id);
            if(areas == null)
            {
                return NotFound();
            }
            return View (areas);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult>Edit (int id,[Bind("IdArea,NomArea")]Areas areas)
        {
            if (id != areas.IdArea)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var areaDb = await _context.Areas.FindAsync(id);
                if (areaDb == null)
                {
                    return NotFound();
                }

                try
                {
                    areaDb.NomArea = areas.NomArea;
                    areaDb.LastModify = User.ObtenerNombreUsuario();
                    areaDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AreasExists(areas.IdArea))
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
            return View(areas);
        }
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var areas = await _context.Areas
                .FirstOrDefaultAsync(a => a.IdArea == id);

            if (areas == null)
            {
                return NotFound();
            }

            return View(areas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var areas = await _context.Areas.FindAsync(id);

            if (areas == null)
            {
                return NotFound();
            }

            _context.Areas.Remove(areas);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool AreasExists(int id)
        {
            return _context.Areas.Any(a => a.IdArea == id);
        }
       
        
    }    
}

