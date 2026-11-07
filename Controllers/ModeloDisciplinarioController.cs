using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Models;

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
            var items = await _context.ModeloDisciplinario
                .AsNoTracking()
                .OrderByDescending(m => m.IdModelo)
                .ToListAsync();

            ViewBag.Areas = await _context.Areas.AsNoTracking()
                .ToDictionaryAsync(a => a.IdArea, a => a.NomArea);
            ViewBag.Cargos = await _context.Cargo.AsNoTracking()
                .ToDictionaryAsync(c => c.IdCargo, c => c.NomCargo);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelo = await _context.ModeloDisciplinario
                .AsNoTracking()
                .FirstOrDefaultAsync(md => md.IdModelo == id);
            if (modelo == null)
            {
                return NotFound();
            }

            await CargarNombresAsync(modelo.IdArea, modelo.cargo);
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Documento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelo = await _context.ModeloDisciplinario
                .AsNoTracking()
                .FirstOrDefaultAsync(md => md.IdModelo == id);
            if (modelo == null)
            {
                return NotFound();
            }

            await CargarNombresAsync(modelo.IdArea, modelo.cargo);
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListasAsync();
            return View(new ModeloDisciplinario
            {
                para = string.Empty,
                asunto = string.Empty,
                NomTrabajador = string.Empty,
                Proceso = string.Empty,
                Situacion = string.Empty,
                Firma = string.Empty,
                fecha_creacion = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdArea,para,asunto,NomTrabajador,cargo,Proceso,Situacion,Firma")] ModeloDisciplinario modeloDisciplinario)
        {
            ModelState.Remove(nameof(ModeloDisciplinario.fecha_creacion));
            ModelState.Remove(nameof(ModeloDisciplinario.DateOwner));

            if (ModelState.IsValid)
            {
                var ahora = DateTime.Now;
                modeloDisciplinario.fecha_creacion = ahora;
                modeloDisciplinario.DateOwner = ahora;
                modeloDisciplinario.Owner = User.ObtenerNombreUsuario();
                modeloDisciplinario.LastModify = null;
                modeloDisciplinario.LastDate = null;

                _context.Add(modeloDisciplinario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Modelo disciplinario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListasAsync(modeloDisciplinario.IdArea, modeloDisciplinario.cargo);
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

            await CargarListasAsync(modeloDisciplinario.IdArea, modeloDisciplinario.cargo);
            return View(modeloDisciplinario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdModelo,IdArea,para,asunto,NomTrabajador,cargo,Proceso,Situacion,Firma")] ModeloDisciplinario modeloDisciplinario)
        {
            if (id != modeloDisciplinario.IdModelo)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(ModeloDisciplinario.fecha_creacion));
            ModelState.Remove(nameof(ModeloDisciplinario.DateOwner));

            if (ModelState.IsValid)
            {
                var modeloDisciplinarioDb = await _context.ModeloDisciplinario.FindAsync(id);
                if (modeloDisciplinarioDb == null)
                {
                    return NotFound();
                }

                try
                {
                    modeloDisciplinarioDb.IdArea = modeloDisciplinario.IdArea;
                    modeloDisciplinarioDb.para = modeloDisciplinario.para;
                    modeloDisciplinarioDb.asunto = modeloDisciplinario.asunto;
                    modeloDisciplinarioDb.NomTrabajador = modeloDisciplinario.NomTrabajador;
                    modeloDisciplinarioDb.cargo = modeloDisciplinario.cargo;
                    modeloDisciplinarioDb.Proceso = modeloDisciplinario.Proceso;
                    modeloDisciplinarioDb.Situacion = modeloDisciplinario.Situacion;
                    modeloDisciplinarioDb.Firma = modeloDisciplinario.Firma;
                    modeloDisciplinarioDb.LastModify = User.ObtenerNombreUsuario();
                    modeloDisciplinarioDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Modelo disciplinario actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeloExist(modeloDisciplinario.IdModelo))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarListasAsync(modeloDisciplinario.IdArea, modeloDisciplinario.cargo);
            return View(modeloDisciplinario);
        }

        public bool ModeloExist(int id)
        {
            return _context.ModeloDisciplinario.Any(md => md.IdModelo == id);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modeloDisciplinario = await _context.ModeloDisciplinario
                .AsNoTracking()
                .FirstOrDefaultAsync(md => md.IdModelo == id);
            if (modeloDisciplinario == null)
            {
                return NotFound();
            }

            await CargarNombresAsync(modeloDisciplinario.IdArea, modeloDisciplinario.cargo);
            return View(modeloDisciplinario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modeloDisciplinario = await _context.ModeloDisciplinario.FindAsync(id);
            if (modeloDisciplinario == null)
            {
                return NotFound();
            }

            _context.ModeloDisciplinario.Remove(modeloDisciplinario);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Modelo disciplinario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasAsync(int? idArea = null, int? idCargo = null)
        {
            ViewBag.Areas = new SelectList(
                await _context.Areas.AsNoTracking().OrderBy(a => a.NomArea).ToListAsync(),
                "IdArea", "NomArea", idArea);

            ViewBag.Cargos = new SelectList(
                await _context.Cargo.AsNoTracking().OrderBy(c => c.NomCargo).ToListAsync(),
                "IdCargo", "NomCargo", idCargo);
        }

        private async Task CargarNombresAsync(int idArea, int idCargo)
        {
            ViewBag.NombreArea = await _context.Areas.AsNoTracking()
                .Where(a => a.IdArea == idArea)
                .Select(a => a.NomArea)
                .FirstOrDefaultAsync();
            ViewBag.NombreCargo = await _context.Cargo.AsNoTracking()
                .Where(c => c.IdCargo == idCargo)
                .Select(c => c.NomCargo)
                .FirstOrDefaultAsync();
        }
    }
}