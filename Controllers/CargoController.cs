using System;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Controllers;
using FloresMvc.Extensions;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class CargoController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public CargoController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cargo.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Cargo.AsNoTracking().OrderBy(c => c.IdCargo).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Cargos");
            var headers = new[] { "Consecutivo", "Nombre del Cargo", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdCargo;
                ws.Cell(row, 2).Value = x.NomCargo ?? string.Empty;
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
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Cargos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cargo == null)
            {
                return NotFound();
            }
            var cargo = await _context.Cargo
            .FirstOrDefaultAsync(c => c.IdCargo == id);
            if (cargo == null)
            {
                return NotFound();
            }
            return View(cargo);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCargo,NomCargo")] Cargo cargo)
        {
            if (ModelState.IsValid)
            {
                cargo.fecha_Creacion = DateTime.Now;
                cargo.DateOwner = DateTime.Now;
                cargo.Owner = User.ObtenerNombreUsuario();
                _context.Add(cargo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cargo);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var cargo = await _context.Cargo.FindAsync(id);
            if (cargo == null)
            {
                return NotFound();
            }
            return View(cargo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCargo,NomCargo")] Cargo cargo)
        {
            if (id != cargo.IdCargo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var cargoDb = await _context.Cargo.FindAsync(id);
                if (cargoDb == null)
                {
                    return NotFound();
                }

                try
                {
                    cargoDb.NomCargo = cargo.NomCargo;
                    cargoDb.LastModify = User.ObtenerNombreUsuario();
                    cargoDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CargoExists(cargo.IdCargo))
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
            return View(cargo);
        }
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cargo = await _context.Cargo
                .FirstOrDefaultAsync(c => c.IdCargo == id);

            if (cargo == null)
            {
                return NotFound();
            }

            return View(cargo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cargo = await _context.Cargo.FindAsync(id);

            if (cargo == null)
            {
                return NotFound();
            }

            _context.Cargo.Remove(cargo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool CargoExists(int id)
        {
            return _context.Cargo.Any(c => c.IdCargo == id);
        }
    }
}
