using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FloresMvc.Extensions;
using FloresMvc.Filters;
using FloresMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FloresMvc.Controllers
{
    [Authorize]
    public class ConsultaPepsController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public ConsultaPepsController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Permiso("ConsultaPeps.Ver")]
        public async Task<IActionResult> Index()
        {
            var items = await _context.ConsultaPeps
                .AsNoTracking()
                .OrderByDescending(c => c.IdConsulta)
                .ToListAsync();

            await CargarNombresViewBagAsync(items);
            return View(items);
        }

        [HttpGet]
        [Permiso("ConsultaPeps.Exportar")]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.ConsultaPeps
                .AsNoTracking()
                .OrderByDescending(c => c.IdConsulta)
                .ToListAsync();

            var usuarios = await ObtenerUsuariosDictAsync(items.Select(i => i.IdUsuario));
            var cargos = await ObtenerCargosDictAsync(items.Select(i => i.IdCargo));
            var areas = await ObtenerAreasDictAsync(items.Select(i => i.IdArea));

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Consultas PEPS");
            var headers = new[] { "Consecutivo", "Usuario", "Cédula", "Cargo", "Área", "Es PEP (usted o familiar)", "Creado por", "Fecha registro", "Modificado por", "Última modificación" };
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
                ws.Cell(row, 1).Value = x.IdConsulta;
                ws.Cell(row, 2).Value = usuarios.TryGetValue(x.IdUsuario, out var nu) ? nu : x.IdUsuario.ToString();
                ws.Cell(row, 3).Value = x.Cedula ?? string.Empty;
                ws.Cell(row, 4).Value = cargos.TryGetValue(x.IdCargo, out var nc) ? nc : (x.IdCargo > 0 ? x.IdCargo.ToString() : string.Empty);
                ws.Cell(row, 5).Value = areas.TryGetValue(x.IdArea, out var na) ? na : (x.IdArea > 0 ? x.IdArea.ToString() : string.Empty);
                ws.Cell(row, 6).Value = x.EsPep == true ? "Sí" : x.EsPep == false ? "No" : string.Empty;
                ws.Cell(row, 7).Value = x.Owner ?? string.Empty;
                if (x.DateOwner.HasValue) { ws.Cell(row, 8).Value = x.DateOwner.Value; ws.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                ws.Cell(row, 9).Value = x.LastModify ?? string.Empty;
                if (x.LastDate.HasValue) { ws.Cell(row, 10).Value = x.LastDate.Value; ws.Cell(row, 10).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ConsultasPeps_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [Permiso("ConsultaPeps.Ver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ConsultaPeps.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdConsulta == id);
            if (item == null) return NotFound();

            await CargarNombresItemAsync(item.IdUsuario, item.IdCargo, item.IdArea);
            return View(item);
        }

        [HttpGet]
        [Permiso("ConsultaPeps.Documento")]
        public async Task<IActionResult> Documento(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ConsultaPeps.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdConsulta == id);
            if (item == null) return NotFound();

            await CargarNombresItemAsync(item.IdUsuario, item.IdCargo, item.IdArea);
            return View(item);
        }

        [HttpGet]
        [Permiso("ConsultaPeps.Crear")]
        public async Task<IActionResult> Create()
        {
            await CargarListasAsync();
            return View(new ConsultaPeps());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("ConsultaPeps.Crear")]
        public async Task<IActionResult> Create([Bind("IdConsulta,IdUsuario,Cedula,IdCargo,IdArea,EsPep,Firma")] ConsultaPeps consulta)
        {
            ValidarConsulta(consulta);

            if (ModelState.IsValid)
            {
                var ahora = DateTime.Now;
                consulta.Firma = consulta.Firma?.Trim();
                consulta.Owner = User.ObtenerNombreUsuario();
                consulta.DateOwner = ahora;
                consulta.LastModify = null;
                consulta.LastDate = null;

                _context.ConsultaPeps.Add(consulta);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Consulta PEPS creada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListasAsync(consulta.IdUsuario, consulta.IdCargo, consulta.IdArea);
            return View(consulta);
        }

        [HttpGet]
        [Permiso("ConsultaPeps.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var consulta = await _context.ConsultaPeps.FindAsync(id);
            if (consulta == null) return NotFound();

            await CargarListasAsync(consulta.IdUsuario, consulta.IdCargo, consulta.IdArea);
            return View(consulta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("ConsultaPeps.Editar")]
        public async Task<IActionResult> Edit(int id, [Bind("IdConsulta,IdUsuario,Cedula,IdCargo,IdArea,EsPep,Firma")] ConsultaPeps consulta)
        {
            if (id != consulta.IdConsulta) return NotFound();

            ValidarConsulta(consulta);

            if (ModelState.IsValid)
            {
                var consultaDb = await _context.ConsultaPeps.FindAsync(id);
                if (consultaDb == null) return NotFound();

                try
                {
                    consultaDb.IdUsuario = consulta.IdUsuario;
                    consultaDb.Cedula = consulta.Cedula?.Trim();
                    consultaDb.IdCargo = consulta.IdCargo;
                    consultaDb.IdArea = consulta.IdArea;
                    consultaDb.EsPep = consulta.EsPep;
                    consultaDb.Firma = consulta.Firma?.Trim();
                    consultaDb.LastModify = User.ObtenerNombreUsuario();
                    consultaDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Consulta PEPS actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultaExists(consulta.IdConsulta)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarListasAsync(consulta.IdUsuario, consulta.IdCargo, consulta.IdArea);
            return View(consulta);
        }

        [HttpGet, ActionName("Delete")]
        [Permiso("ConsultaPeps.Eliminar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ConsultaPeps.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdConsulta == id);
            if (item == null) return NotFound();

            await CargarNombresItemAsync(item.IdUsuario, item.IdCargo, item.IdArea);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("ConsultaPeps.Eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ConsultaPeps.FindAsync(id);
            if (item == null) return NotFound();

            _context.ConsultaPeps.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Consulta PEPS eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public bool ConsultaExists(int id) => _context.ConsultaPeps.Any(c => c.IdConsulta == id);

        private void ValidarConsulta(ConsultaPeps consulta)
        {
            if (consulta.IdUsuario <= 0)
                ModelState.AddModelError(nameof(ConsultaPeps.IdUsuario), "Seleccione un usuario.");
            if (string.IsNullOrWhiteSpace(consulta.Cedula))
                ModelState.AddModelError(nameof(ConsultaPeps.Cedula), "La cédula es obligatoria.");
            if (string.IsNullOrWhiteSpace(consulta.Firma))
                ModelState.AddModelError(nameof(ConsultaPeps.Firma), "La firma es obligatoria.");
        }

        private async Task CargarListasAsync(int? idUsuario = null, int? idCargo = null, int? idArea = null)
        {
            var usuarios = await _context.Usuarios.AsNoTracking()
                .OrderBy(u => u.Nombre).ThenBy(u => u.Apellido)
                .Select(u => new { u.IdUsuarios, Nombre = (u.Nombre + " " + u.Apellido).Trim() })
                .ToListAsync();
            ViewBag.Usuarios = new SelectList(usuarios, "IdUsuarios", "Nombre", idUsuario);

            var cargos = await _context.Cargo.AsNoTracking().OrderBy(c => c.NomCargo).ToListAsync();
            ViewBag.Cargos = new SelectList(cargos, "IdCargo", "NomCargo", idCargo);

            var areas = await _context.Areas.AsNoTracking().OrderBy(a => a.NomArea).ToListAsync();
            ViewBag.Areas = new SelectList(areas, "IdArea", "NomArea", idArea);
        }

        private async Task CargarNombresViewBagAsync(List<ConsultaPeps> items)
        {
            ViewBag.Usuarios = await ObtenerUsuariosDictAsync(items.Select(i => i.IdUsuario));
            ViewBag.Cargos = await ObtenerCargosDictAsync(items.Select(i => i.IdCargo));
            ViewBag.Areas = await ObtenerAreasDictAsync(items.Select(i => i.IdArea));
        }

        private async Task CargarNombresItemAsync(int idUsuario, int idCargo, int idArea)
        {
            var usuarios = await ObtenerUsuariosDictAsync(new[] { idUsuario });
            var cargos = await ObtenerCargosDictAsync(new[] { idCargo });
            var areas = await ObtenerAreasDictAsync(new[] { idArea });
            ViewBag.NombreUsuario = usuarios.TryGetValue(idUsuario, out var nu) ? nu : idUsuario.ToString();
            ViewBag.NombreCargo = cargos.TryGetValue(idCargo, out var nc) ? nc : (idCargo > 0 ? idCargo.ToString() : "—");
            ViewBag.NombreArea = areas.TryGetValue(idArea, out var na) ? na : (idArea > 0 ? idArea.ToString() : "—");
        }

        private async Task<Dictionary<int, string>> ObtenerUsuariosDictAsync(IEnumerable<int> ids)
        {
            var set = ids.Where(i => i > 0).Distinct().ToList();
            if (set.Count == 0) return new Dictionary<int, string>();
            return await _context.Usuarios.AsNoTracking()
                .Where(u => set.Contains(u.IdUsuarios))
                .Select(u => new { u.IdUsuarios, Nombre = (u.Nombre + " " + u.Apellido).Trim() })
                .ToDictionaryAsync(x => x.IdUsuarios, x => x.Nombre);
        }

        private async Task<Dictionary<int, string>> ObtenerCargosDictAsync(IEnumerable<int> ids)
        {
            var set = ids.Where(i => i > 0).Distinct().ToList();
            if (set.Count == 0) return new Dictionary<int, string>();
            return await _context.Cargo.AsNoTracking()
                .Where(c => set.Contains(c.IdCargo))
                .ToDictionaryAsync(c => c.IdCargo, c => c.NomCargo);
        }

        private async Task<Dictionary<int, string>> ObtenerAreasDictAsync(IEnumerable<int> ids)
        {
            var set = ids.Where(i => i > 0).Distinct().ToList();
            if (set.Count == 0) return new Dictionary<int, string>();
            return await _context.Areas.AsNoTracking()
                .Where(a => set.Contains(a.IdArea))
                .ToDictionaryAsync(a => a.IdArea, a => a.NomArea);
        }
    }
}
