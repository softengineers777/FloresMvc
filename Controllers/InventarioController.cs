using System;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class InventarioController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public InventarioController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Inventario
        public async Task<IActionResult> Index()
        {
            return View(await _context.Inventario.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Inventario.AsNoTracking().OrderBy(i => i.IdInventario).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario");
            var headers = new[] { "Consecutivo", "Nombre", "Descripción", "Cantidad", "Precio", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdInventario;
                ws.Cell(row, 2).Value = x.Nombre ?? string.Empty;
                ws.Cell(row, 3).Value = x.Descripcion ?? string.Empty;
                ws.Cell(row, 4).Value = x.Cantidad;
                ws.Cell(row, 5).Value = x.Precio;
                ws.Cell(row, 6).Value = x.fecha_Creacion;
                ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 7).Value = x.Owner ?? string.Empty;
                ws.Cell(row, 8).Value = x.LastModify ?? string.Empty;
                if (x.LastDate.HasValue) { ws.Cell(row, 9).Value = x.LastDate.Value; ws.Cell(row, 9).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Inventario_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Inventario == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.IdInventario == id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // GET: Inventario/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdInventario,Nombre,Descripcion,Cantidad,Precio")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                inventario.fecha_Creacion = DateTime.Now;
                inventario.Owner = User.ObtenerNombreUsuario();
                inventario.DateOwner = DateTime.Now;

                _context.Add(inventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(inventario);
        }

        // GET: Inventario/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario.FindAsync(id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("IdInventario,Nombre,Descripcion,Cantidad,Precio")] Inventario inventario)
        {
            if (id != inventario.IdInventario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var inventarioDb = await _context.Inventario.FindAsync(id);
                if (inventarioDb == null)
                {
                    return NotFound();
                }

                try
                {
                    // Solo se actualizan los campos editables: la auditoría de
                    // creación (fecha_Creacion, Owner, DateOwner) se conserva.
                    inventarioDb.Nombre = inventario.Nombre;
                    inventarioDb.Descripcion = inventario.Descripcion;
                    inventarioDb.Cantidad = inventario.Cantidad;
                    inventarioDb.Precio = inventario.Precio;
                    inventarioDb.LastModify = User.ObtenerNombreUsuario();
                    inventarioDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventarioExists(inventario.IdInventario))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(inventario);
        }

        // GET: Inventario/Delete/5
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.IdInventario == id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // POST: Inventario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventario = await _context.Inventario.FindAsync(id);

            if (inventario == null)
            {
                return NotFound();
            }

            _context.Inventario.Remove(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool InventarioExists(int id)
        {
            return _context.Inventario.Any(i => i.IdInventario == id);
        }
    }
}
