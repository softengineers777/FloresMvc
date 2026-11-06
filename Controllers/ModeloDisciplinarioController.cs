using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using FloresMvc.Models;
using FloresMvc.Extensions;
using Microsoft.VisualBasic;
using AspNetCoreGeneratedDocument;

namespace FloresMvc.Controllers
{
    public class ModeloDisciplinarioController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FloresDbContext _context;

        public ModeloDisciplinarioController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.ModeloDisciplinario.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ModeloDisciplinario == null)
            {
                return NotFound();
            }
            var ModeloDisciplinario = await _context.ModeloDisciplinario
            .FirstOrDefaultAsync(md => md.IdModelo == id);
            if (ModeloDisciplinario == null)
            {
                return NotFound();
            }
            return View(ModeloDisciplinario);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdModelo,IdArea,fecha_creacion,para,asunto,NomTrabajador,cargo, Proceso, Firma")] ModeloDisciplinario modeloDisciplinario)
        {
            if (ModelState.IsValid)
            {
                modeloDisciplinario.fecha_creacion = DateTime.Now;
                modeloDisciplinario.DateOwner = DateTime.Now;
                modeloDisciplinario.Owner = User.ObtenerNombreUsuario();
                _context.Add(modeloDisciplinario);
                await _context.SaveChangesAsync();
                return View();
            }
            return View(modeloDisciplinario);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var modeloDisciplinario = await _context.ModeloDisciplinario.FindAsync(id);
            if (modeloDisciplinario == null)
            {
                return NotFound();
            }
            return View(modeloDisciplinario);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdModelo,IdArea,fecha_creacion,para,asunto,NomTrabajador,cargo, Proceso, Firma")] ModeloDisciplinario modeloDisciplinario)
        {
            if (id != modeloDisciplinario.IdModelo)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                var modeloDisciplinarioDb = await _context.ModeloDisciplinario.FindAsync(id);
                if (modeloDisciplinarioDb == null)
                {
                    return NotFound();
                }
                try
                {
                    modeloDisciplinarioDb.fecha_creacion = modeloDisciplinario.fecha_creacion;
                    modeloDisciplinarioDb.LastModify = User.ObtenerNombreUsuario();
                    modeloDisciplinarioDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeloExist(modeloDisciplinario.IdModelo))
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
            return View();
        }
        public bool ModeloExist(int id)
        {
            return _context.ModeloDisciplinario.Any(md => md.IdModelo == id);
        }

        [HttpGet,ActionName("Delete")]
        public async Task<IActionResult>Delete(int? id)
        {
            if (id == null)
            {
            {return NotFound();}
                
            }
            var modeloDisciplinario = await _context.ModeloDisciplinario
            .FirstOrDefaultAsync (md=> md.IdModelo == id);
            if (modeloDisciplinario == null)
            {
                return NotFound();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>DeleteConfirmed(int id)
        {
            var modeloDisciplinario = await _context.ModeloDisciplinario.FindAsync(id);
            if(modeloDisciplinario == null)
            {
                return NotFound();
            }
            _context.Remove(modeloDisciplinario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
    }
}