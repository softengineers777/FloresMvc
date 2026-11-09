using System;
using System.Data;
using System.IO;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
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

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Fincas.AsNoTracking().OrderBy(f => f.IdFinca).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Fincas");
            var headers = new[] { "Consecutivo", "Nombre de la Finca", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
            for (var col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937");
                cell.Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var x in items)
            {
                ws.Cell(row, 1).Value = x.IdFinca;
                ws.Cell(row, 2).Value = x.NomFinca ?? string.Empty;
                ws.Cell(row, 3).Value = x.fecha_Creacion;
                ws.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 4).Value = x.Owner ?? string.Empty;
                ws.Cell(row, 5).Value = x.LastModify ?? string.Empty;
                if (x.LastDate.HasValue) { ws.Cell(row, 6).Value = x.LastDate.Value; ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Fincas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
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

