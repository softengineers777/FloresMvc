using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Controllers;
using FloresMvc.Extensions;
using FloresMvc.Filters;
using FloresMvc.Models;


namespace FloresMvc.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public CategoriasController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [Permiso("Categoria.Ver")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        [HttpGet]
        [Permiso("Categoria.Exportar")]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Categorias.AsNoTracking().OrderBy(c => c.IdCategoria).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Categorías");
            var headers = new[] { "Consecutivo", "Nombre de la Categoría", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdCategoria;
                ws.Cell(row, 2).Value = x.Nomcategoria ?? string.Empty;
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
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Categorias_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        [Permiso("Categoria.Ver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categorias == null)
            {
                return NotFound();
            }
            var Categorias = await _context.Categorias
            .FirstOrDefaultAsync(c => c.IdCategoria == id);
            if (Categorias == null)
            {
                return NotFound();
            }
            return View(Categorias);
        }
        [HttpGet]
        [Permiso("Categoria.Crear")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Categoria.Crear")]
        public async Task<IActionResult> Create([Bind("IdCategoria,Nomcategoria,fecha_Creacion")] Categorias categorias)
        {
            if(ModelState.IsValid)
            {
                categorias.fecha_Creacion = DateTime.Now;
                categorias.DateOwner = DateTime.Now;
                categorias.Owner = User.ObtenerNombreUsuario();
                _context.Add(categorias);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categorias);
        }
        [HttpGet]
        [Permiso("Categoria.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var categorias = await _context.Categorias.FindAsync(id);
            if(categorias == null)
            {
                return NotFound();
            }
            return View (categorias);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Categoria.Editar")]
        public async Task<IActionResult> Edit(int id, [Bind("IdCategoria,Nomcategoria,fecha_Creacion")] Categorias categorias)
        {
            if(id != categorias.IdCategoria)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                var categoriaDb = await _context.Categorias.FindAsync(id);
                if (categoriaDb == null)
                {
                    return NotFound();
                }

                try
                {
                    categoriaDb.Nomcategoria = categorias.Nomcategoria;
                    categoriaDb.LastModify = User.ObtenerNombreUsuario();
                    categoriaDb.LastDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExist(categorias.IdCategoria))
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
            return View(categorias);
        }
        public bool CategoriaExist (int id)
        {
            return _context.Categorias.Any(c=> c.IdCategoria == id);
        }
        [HttpGet, ActionName("Delete")]
        [Permiso("Categoria.Eliminar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var categorias = await _context.Categorias
            .FirstOrDefaultAsync(c=> c.IdCategoria == id);
            if (categorias == null)
            {
                return NotFound();
            }
            return View(categorias);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Categoria.Eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categorias = await _context.Categorias.FindAsync(id);
            if (categorias == null)
            {
                return NotFound();
            }
            _context.Categorias.Remove(categorias);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }


}