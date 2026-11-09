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

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Areas.AsNoTracking().OrderBy(a => a.IdArea).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Áreas");
            var headers = new[] { "Consecutivo", "Nombre del Área", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdArea;
                ws.Cell(row, 2).Value = x.NomArea ?? string.Empty;
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
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Areas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
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

