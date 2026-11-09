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
    public class SucursalController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public SucursalController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sucursal.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Sucursal.AsNoTracking().OrderBy(s => s.IdSucursal).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sucursales");
            var headers = new[] { "Consecutivo", "Nombre de la Sucursal", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdSucursal;
                ws.Cell(row, 2).Value = x.NombreSuc ?? string.Empty;
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
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Sucursales_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sucursal == null)
            {
                return NotFound();
            }
            var sucursal = await _context.Sucursal
            .FirstOrDefaultAsync(s => s.IdSucursal == id);
            if (sucursal == null)
            {
                return NotFound();
            }
            return View(sucursal);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSucursal,NombreSuc")] Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                sucursal.fecha_Creacion = DateTime.Now;
                sucursal.DateOwner = DateTime.Now;
                sucursal.Owner = User.ObtenerNombreUsuario();
                _context.Add(sucursal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sucursal = await _context.Sucursal.FindAsync(id);
            if (sucursal == null)
            {
                return NotFound();
            }
            return View(sucursal);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSucursal,NombreSuc")] Sucursal sucursal)
        {
            if (id != sucursal.IdSucursal)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var sucursalDb = await _context.Sucursal.FindAsync(id);
                if (sucursalDb == null)
                {
                    return NotFound();
                }

                try
                {
                    sucursalDb.NombreSuc = sucursal.NombreSuc;
                    sucursalDb.LastModify = User.ObtenerNombreUsuario();
                    sucursalDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SucursalExists(sucursal.IdSucursal))
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
            return View(sucursal);
        }
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sucursal = await _context.Sucursal
                .FirstOrDefaultAsync(s => s.IdSucursal == id);

            if (sucursal == null)
            {
                return NotFound();
            }

            return View(sucursal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sucursal = await _context.Sucursal.FindAsync(id);

            if (sucursal == null)
            {
                return NotFound();
            }

            _context.Sucursal.Remove(sucursal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool SucursalExists(int id)
        {
            return _context.Sucursal.Any(s => s.IdSucursal == id);
        }
    }
}
