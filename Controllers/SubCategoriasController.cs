using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Filters;
using FloresMvc.Models;


namespace FloresMvc.Controllers
{
    [Authorize]
    public class SubCategoriasController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public SubCategoriasController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Permiso("SubCategorias.Ver")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubCategorias.ToListAsync());
        }

        [HttpGet]
        [Permiso("SubCategorias.Exportar")]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.SubCategorias.AsNoTracking().OrderBy(s => s.IdSubcategoria).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Subcategorías");
            var headers = new[] { "Consecutivo", "Nombre de la Subcategoría", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdSubcategoria;
                ws.Cell(row, 2).Value = x.NomSubcategoria ?? string.Empty;
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
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SubCategorias_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [Permiso("SubCategorias.Ver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SubCategorias == null)
            {
                return NotFound();
            }
            var subcategorias = await _context.SubCategorias
                .FirstOrDefaultAsync(s => s.IdSubcategoria == id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            return View(subcategorias);
        }

        [HttpGet]
        [Permiso("SubCategorias.Crear")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("SubCategorias.Crear")]
        public async Task<IActionResult> Create([Bind("IdSubcategoria,NomSubcategoria,fecha_Creacion")] SubCategorias subcategorias)
        {
            if (ModelState.IsValid)
            {
                subcategorias.fecha_Creacion = DateTime.Now;
                subcategorias.DateOwner = DateTime.Now;
                subcategorias.Owner = User.ObtenerNombreUsuario();
                _context.Add(subcategorias);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subcategorias);
        }

        [HttpGet]
        [Permiso("SubCategorias.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategorias = await _context.SubCategorias.FindAsync(id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            return View(subcategorias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("SubCategorias.Editar")]
        public async Task<IActionResult> Edit(int id, [Bind("IdSubcategoria,NomSubcategoria,fecha_Creacion")] SubCategorias subcategorias)
        {
            if (id != subcategorias.IdSubcategoria)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var subcategoriaDb = await _context.SubCategorias.FindAsync(id);
                if (subcategoriaDb == null)
                {
                    return NotFound();
                }

                try
                {
                    subcategoriaDb.NomSubcategoria = subcategorias.NomSubcategoria;
                    subcategoriaDb.LastModify = User.ObtenerNombreUsuario();
                    subcategoriaDb.LastDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubCategoriaExist(subcategorias.IdSubcategoria))
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
            return View(subcategorias);
        }

        public bool SubCategoriaExist(int id)
        {
            return _context.SubCategorias.Any(s => s.IdSubcategoria == id);
        }

        [HttpGet, ActionName("Delete")]
        [Permiso("SubCategorias.Eliminar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategorias = await _context.SubCategorias
                .FirstOrDefaultAsync(s => s.IdSubcategoria == id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            return View(subcategorias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("SubCategorias.Eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subcategorias = await _context.SubCategorias.FindAsync(id);
            if (subcategorias == null)
            {
                return NotFound();
            }
            _context.SubCategorias.Remove(subcategorias);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
