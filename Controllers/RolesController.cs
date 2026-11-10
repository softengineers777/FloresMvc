using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
    public class RolesController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public RolesController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [Permiso("Roles.Ver")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }

        [HttpGet]
        [Permiso("Roles.Exportar")]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Roles.AsNoTracking().OrderBy(r => r.IdRol).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Roles");
            var headers = new[] { "Consecutivo", "Nombre", "Fecha de Creación", "Creado por", "Última Modificación", "Fecha de Modificación" };
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
                ws.Cell(row, 1).Value = x.IdRol;
                ws.Cell(row, 2).Value = x.Nombre ?? string.Empty;
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
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Roles_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        [Permiso("Roles.Ver")]
        public async Task<IActionResult> GetRolesJson()
        {
            var roles = await _context.Roles.ToListAsync();
            return Json(roles);
        }
        [HttpPost]
        [Permiso("Roles.Editar")]
        public async Task<IActionResult> ActualizarCelda([FromBody] Roles rolModificado)
        {
            if (rolModificado == null)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }
            if (string.IsNullOrWhiteSpace(rolModificado.Nombre))
            {
                return BadRequest(new { success = false, message = "El nombre del rol es obligatorio." });
            }

            var rolDb = await _context.Roles.FindAsync(rolModificado.IdRol);
            if (rolDb == null)
            {
                return NotFound(new { success = false, message = "Rol no encontrado" });
            }

            rolDb.Nombre = rolModificado.Nombre.Trim();
            rolDb.LastModify = User.ObtenerNombreUsuario();
            rolDb.LastDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Actualización correcta",
                lastModify = rolDb.LastModify,
                lastDate = rolDb.LastDate
            });
        }
        [Permiso("Roles.Ver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }
            var Roles = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRol == id);
            if (Roles == null)
            {
                return NotFound();
            }
            return View(Roles);
        }
        [HttpGet]
        [Permiso("Roles.Crear")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Roles.Crear")]
        public async Task<IActionResult> Create([Bind("IdRol,Nombre")] Roles roles)
        {
            if (ModelState.IsValid)
            {
                roles.fecha_Creacion = DateTime.UtcNow;
                roles.Owner = User.ObtenerNombreUsuario();
                roles.DateOwner = DateTime.Now;
                _context.Add(roles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roles);
        }

        [HttpPost]
        [Permiso("Roles.Crear")]
        public async Task<IActionResult> CreateAjax([FromBody] Roles rol)
        {
            if (rol == null)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }
            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                return BadRequest(new { success = false, message = "El nombre del rol es obligatorio." });
            }

            rol.Nombre = rol.Nombre.Trim();
            rol.fecha_Creacion = DateTime.UtcNow;
            rol.Owner = User.ObtenerNombreUsuario();
            rol.DateOwner = DateTime.Now;
            rol.LastModify = null;
            rol.LastDate = null;

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Rol creado con éxito",
                idRol = rol.IdRol
            });
        }

        [HttpGet]
        [Permiso("Roles.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var roles = await _context.Roles.FindAsync(id);
            if (roles == null)
            {
                return NotFound();
            }
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Roles.Editar")]
        public async Task<IActionResult> Edit(int id, [Bind("IdRol,Nombre")] Roles roles)
        {
            if (id != roles.IdRol)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var rolDb = await _context.Roles.FindAsync(id);
                    if (rolDb == null)
                    {
                        return NotFound();
                    }

                    rolDb.Nombre = roles.Nombre;
                    rolDb.LastModify = User.ObtenerNombreUsuario();
                    rolDb.LastDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolesExist(roles.IdRol))
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
            return View(roles);
        }
        public bool RolesExist(int id)
        {
            return _context.Roles.Any(r => r.IdRol == id);
        }

        [HttpGet, ActionName("Delete")]
        [Permiso("Roles.Eliminar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var roles = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRol == id);
            if (roles == null)
            {
                return NotFound();
            }
            return View(roles);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Roles.Eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roles = await _context.Roles.FindAsync(id);
            if (roles == null)
            {
                return NotFound();
            }
            _context.Roles.Remove(roles);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Permiso("Roles.Gestionar")]
        public async Task<IActionResult> Permisos(int id)
        {
            var rol = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.IdRol == id);
            if (rol == null)
            {
                return NotFound();
            }

            var permisos = await _context.Permisos
                .AsNoTracking()
                .OrderBy(p => p.Modulo)
                .ThenBy(p => p.Clave)
                .ToListAsync();

            var asignados = await _context.RolPermisos
                .AsNoTracking()
                .Where(rp => rp.IdRol == id)
                .Select(rp => rp.IdPermiso)
                .ToListAsync();

            ViewBag.Rol = rol;
            ViewBag.Asignados = new HashSet<int>(asignados);

            return View(permisos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Roles.Gestionar")]
        public async Task<IActionResult> Permisos(int id, int[]? permisosSeleccionados)
        {
            var rolExiste = await _context.Roles.AnyAsync(r => r.IdRol == id);
            if (!rolExiste)
            {
                return NotFound();
            }

            await using var tx = await _context.Database.BeginTransactionAsync();

            var actuales = await _context.RolPermisos
                .Where(rp => rp.IdRol == id)
                .ToListAsync();

            if (actuales.Count > 0)
            {
                _context.RolPermisos.RemoveRange(actuales);
            }

            if (permisosSeleccionados != null && permisosSeleccionados.Length > 0)
            {
                var idsValidos = await _context.Permisos
                    .Where(p => permisosSeleccionados.Contains(p.IdPermiso))
                    .Select(p => p.IdPermiso)
                    .ToListAsync();

                foreach (var idPermiso in idsValidos.Distinct())
                {
                    _context.RolPermisos.Add(new RolPermiso
                    {
                        IdRol = id,
                        IdPermiso = idPermiso
                    });
                }
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "Permisos actualizados correctamente. Los usuarios con este rol verán los cambios al iniciar sesión nuevamente.";

            return RedirectToAction(nameof(Permisos), new { id });
        }
    }
}
