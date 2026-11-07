using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class CitacionDescargasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FloresDbContext _context;

        public CitacionDescargasController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.CitacionDescargos
                .AsNoTracking()
                .OrderByDescending(c => c.IdCitacion)
                .ToListAsync();

            ViewBag.Cargos = await _context.Cargo.AsNoTracking()
                .ToDictionaryAsync(c => c.IdCargo, c => c.NomCargo);
            ViewBag.Areas = await _context.Areas.AsNoTracking()
                .ToDictionaryAsync(a => a.IdArea, a => a.NomArea);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.CitacionDescargos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCitacion == id);
            if (item == null)
            {
                return NotFound();
            }

            await CargarNombresAsync(item.IdCargo, item.IdArea);
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListasAsync();
            return View(new CitacionDescargos
            {
                fecha_Creacion = DateTime.Now,
                fecha_Diligencia = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ciudad,NomTrabajador,IdCargo,IdArea,Proceso,fecha_Diligencia,FechaHechos,CcRecibido")] CitacionDescargos citacion, string? HoraDiligenciaHora)
        {
            ModelState.Remove(nameof(CitacionDescargos.fecha_Creacion));
            ModelState.Remove(nameof(CitacionDescargos.DateOwner));
            ModelState.Remove(nameof(CitacionDescargos.HoraDiligencia));

            citacion.HoraDiligencia = ParseHora(HoraDiligenciaHora, citacion.fecha_Diligencia);

            if (ModelState.IsValid)
            {
                var ahora = DateTime.Now;
                citacion.fecha_Creacion = ahora;
                citacion.DateOwner = ahora;
                citacion.Owner = User.ObtenerNombreUsuario();
                citacion.LastModify = null;
                citacion.LastDate = null;

                _context.Add(citacion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Citación a descargos creada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListasAsync(citacion.IdCargo, citacion.IdArea);
            return View(citacion);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var citacion = await _context.CitacionDescargos.FindAsync(id);
            if (citacion == null)
            {
                return NotFound();
            }

            await CargarListasAsync(citacion.IdCargo, citacion.IdArea);
            return View(citacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCitacion,Ciudad,NomTrabajador,IdCargo,IdArea,Proceso,fecha_Diligencia,FechaHechos,CcRecibido")] CitacionDescargos citacion, string? HoraDiligenciaHora)
        {
            if (id != citacion.IdCitacion)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CitacionDescargos.fecha_Creacion));
            ModelState.Remove(nameof(CitacionDescargos.DateOwner));
            ModelState.Remove(nameof(CitacionDescargos.HoraDiligencia));

            citacion.HoraDiligencia = ParseHora(HoraDiligenciaHora, citacion.fecha_Diligencia);

            if (ModelState.IsValid)
            {
                var citacionDb = await _context.CitacionDescargos.FindAsync(id);
                if (citacionDb == null)
                {
                    return NotFound();
                }

                try
                {
                    citacionDb.Ciudad = citacion.Ciudad;
                    citacionDb.NomTrabajador = citacion.NomTrabajador;
                    citacionDb.IdCargo = citacion.IdCargo;
                    citacionDb.IdArea = citacion.IdArea;
                    citacionDb.Proceso = citacion.Proceso;
                    citacionDb.fecha_Diligencia = citacion.fecha_Diligencia;
                    citacionDb.HoraDiligencia = citacion.HoraDiligencia;
                    citacionDb.FechaHechos = citacion.FechaHechos;
                    citacionDb.CcRecibido = citacion.CcRecibido;
                    citacionDb.LastModify = User.ObtenerNombreUsuario();
                    citacionDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Citación a descargos actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitacionExists(citacion.IdCitacion))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarListasAsync(citacion.IdCargo, citacion.IdArea);
            return View(citacion);
        }

        public bool CitacionExists(int id)
        {
            return _context.CitacionDescargos.Any(c => c.IdCitacion == id);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var citacion = await _context.CitacionDescargos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCitacion == id);
            if (citacion == null)
            {
                return NotFound();
            }

            await CargarNombresAsync(citacion.IdCargo, citacion.IdArea);
            return View(citacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var citacion = await _context.CitacionDescargos.FindAsync(id);
            if (citacion == null)
            {
                return NotFound();
            }

            _context.CitacionDescargos.Remove(citacion);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Citación a descargos eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasAsync(int? idCargo = null, int? idArea = null)
        {
            ViewBag.Cargos = new SelectList(
                await _context.Cargo.AsNoTracking().OrderBy(c => c.NomCargo).ToListAsync(),
                "IdCargo", "NomCargo", idCargo);
            ViewBag.Areas = new SelectList(
                await _context.Areas.AsNoTracking().OrderBy(a => a.NomArea).ToListAsync(),
                "IdArea", "NomArea", idArea);
        }

        private async Task CargarNombresAsync(int idCargo, int idArea)
        {
            ViewBag.NombreCargo = await _context.Cargo.AsNoTracking()
                .Where(c => c.IdCargo == idCargo)
                .Select(c => c.NomCargo)
                .FirstOrDefaultAsync();
            ViewBag.NombreArea = await _context.Areas.AsNoTracking()
                .Where(a => a.IdArea == idArea)
                .Select(a => a.NomArea)
                .FirstOrDefaultAsync();
        }

        private static DateTime? ParseHora(string? hora, DateTime? fechaBase)
        {
            if (string.IsNullOrWhiteSpace(hora))
            {
                return null;
            }

            if (TimeSpan.TryParse(hora, CultureInfo.InvariantCulture, out var ts) ||
                TimeSpan.TryParse(hora, CultureInfo.CurrentCulture, out ts))
            {
                var baseDate = (fechaBase?.Date) ?? DateTime.Today;
                return baseDate.Add(ts);
            }

            return null;
        }
    }
}
