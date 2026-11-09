using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
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

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.ModeloDisciplinario
                .AsNoTracking()
                .OrderByDescending(m => m.IdModelo)
                .ToListAsync();

            var areas = await _context.Areas.AsNoTracking().ToDictionaryAsync(a => a.IdArea, a => a.NomArea);
            var cargos = await _context.Cargo.AsNoTracking().ToDictionaryAsync(c => c.IdCargo, c => c.NomCargo);
            string NombreArea(int id) => areas.TryGetValue(id, out var n) ? n : id.ToString();
            string NombreCargo(int id) => cargos.TryGetValue(id, out var n) ? n : id.ToString();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Manejo Disciplinario");
            var headers = new[]
            {
                "Consecutivo", "Fecha creación", "Para", "Asunto", "Trabajador",
                "Área", "Cargo", "Proceso", "Situación", "Firma",
                "Creado por", "Fecha registro", "Modificado por", "Última modificación"
            };
            for (var col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937");
                cell.Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var m in items)
            {
                ws.Cell(row, 1).Value = m.IdModelo;
                ws.Cell(row, 2).Value = m.fecha_creacion;
                ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 3).Value = m.para ?? string.Empty;
                ws.Cell(row, 4).Value = m.asunto ?? string.Empty;
                ws.Cell(row, 5).Value = m.NomTrabajador ?? string.Empty;
                ws.Cell(row, 6).Value = NombreArea(m.IdArea);
                ws.Cell(row, 7).Value = NombreCargo(m.cargo);
                ws.Cell(row, 8).Value = m.Proceso ?? string.Empty;
                ws.Cell(row, 9).Value = m.Situacion ?? string.Empty;
                ws.Cell(row, 10).Value = m.Firma ?? string.Empty;
                ws.Cell(row, 11).Value = m.Owner ?? string.Empty;
                ws.Cell(row, 12).Value = m.DateOwner;
                ws.Cell(row, 12).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 13).Value = m.LastModify ?? string.Empty;
                if (m.LastDate.HasValue) { ws.Cell(row, 14).Value = m.LastDate.Value; ws.Cell(row, 14).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ModeloDisciplinario_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
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