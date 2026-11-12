using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Filters;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    [Authorize]
    public class RemisionesController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public RemisionesController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Remisiones
        [Permiso("Remisiones.Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Permiso("Remisiones.Exportar")]
        public async Task<IActionResult> ExportarExcel()
        {
            var remisiones = await _context.Remisiones
                .AsNoTracking()
                .Include(r => r.Area)
                .Include(r => r.Finca)
                .Include(r => r.Detalles)
                .OrderByDescending(r => r.IdRemision)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Remisiones");
            var headers = new[]
            {
                "Consecutivo", "Nº Remisión", "Solicitante", "Dirección", "Área", "Finca", "Fecha",
                "Placa vehículo", "Despachado por", "Transportado por", "Recibido por",
                "Peso total", "Insumos (cant.)", "Observaciones", "Creado por", "Fecha registro"
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
            foreach (var r in remisiones)
            {
                ws.Cell(row, 1).Value = r.IdRemision;
                ws.Cell(row, 2).Value = r.NumeroRemision ?? string.Empty;
                ws.Cell(row, 3).Value = $"{r.Nombre} {r.Apellido}".Trim();
                ws.Cell(row, 4).Value = r.Direccion ?? string.Empty;
                ws.Cell(row, 5).Value = r.Area?.NomArea ?? string.Empty;
                ws.Cell(row, 6).Value = r.Finca?.NomFinca ?? string.Empty;
                ws.Cell(row, 7).Value = r.fecha_Creacion;
                ws.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 8).Value = r.PlacaVehiculo ?? string.Empty;
                ws.Cell(row, 9).Value = r.DespachadoPor ?? string.Empty;
                ws.Cell(row, 10).Value = r.TransportadoPor ?? string.Empty;
                ws.Cell(row, 11).Value = r.RecibidoPor ?? string.Empty;
                ws.Cell(row, 12).Value = r.PesoTotal;
                ws.Cell(row, 13).Value = r.Detalles?.Count ?? 0;
                ws.Cell(row, 14).Value = r.Observaciones ?? string.Empty;
                ws.Cell(row, 15).Value = r.Owner ?? string.Empty;
                ws.Cell(row, 16).Value = r.DateOwner;
                ws.Cell(row, 16).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Remisiones_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // GET: Remisiones/GetRemisionesJson
        // Datos para Tabulator. Se proyecta solo lo que necesita el index
        // para evitar cargar el detalle completo de cada remisión.
        [HttpGet]
        [Permiso("Remisiones.Ver")]
        public async Task<IActionResult> GetRemisionesJson()
        {
            var remisiones = await _context.Remisiones
                .AsNoTracking()
                .OrderByDescending(r => r.IdRemision)
                .Select(r => new
                {
                    idRemision = r.IdRemision,
                    numeroRemision = r.NumeroRemision,
                    solicitante = (r.Nombre + " " + r.Apellido).Trim(),
                    area = r.Area != null ? r.Area.NomArea : "",
                    finca = r.Finca != null ? r.Finca.NomFinca : "",
                    fecha = r.fecha_Creacion,
                    placa = r.PlacaVehiculo,
                    insumos = r.Detalles.Count()
                })
                .ToListAsync();

            return Json(remisiones);
        }

        // GET: Remisiones/Details/5
        [Permiso("Remisiones.Ver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var remision = await _context.Remisiones
                .Include(r => r.Area)
                .Include(r => r.Finca)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRemision == id);

            if (remision == null)
            {
                return NotFound();
            }

            return View(remision);
        }

        // GET: Remisiones/Documento/5
        [Permiso("Remisiones.Ver")]
        public async Task<IActionResult> Documento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var remision = await _context.Remisiones
                .AsNoTracking()
                .Include(r => r.Area)
                .Include(r => r.Finca)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRemision == id);

            if (remision == null)
            {
                return NotFound();
            }

            ViewBag.nombreArea = remision.Area?.NomArea ?? remision.IdArea.ToString();
            ViewBag.nombreFinca = remision.Finca?.NomFinca ?? remision.IdFinca.ToString();

            return View(remision);
        }

        // GET: Remisiones/Create
        [HttpGet]
        [Permiso("Remisiones.Crear")]
        public async Task<IActionResult> Create()
        {
            var model = new RemisionViewModel();
            await CargarListasAsync(model);
            model.Detalles.Add(new RemisionDetalleViewModel());
            return View(model);
        }

        // POST: Remisiones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Remisiones.Crear")]
        public async Task<IActionResult> Create(RemisionViewModel model)
        {
            var detalles = ValidarDetalles(model.Detalles);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model);
                if (model.Detalles == null || model.Detalles.Count == 0)
                {
                    model.Detalles = new List<RemisionDetalleViewModel> { new RemisionDetalleViewModel() };
                }
                return View(model);
            }

            var usuario = User.ObtenerNombreUsuario();
            var ahora = DateTime.Now;

            var remision = new Remisiones
            {
                NumeroRemision = model.NumeroRemision.Trim(),
                Nombre = model.Nombre.Trim(),
                Apellido = model.Apellido.Trim(),
                Direccion = model.Direccion.Trim(),
                IdArea = model.IdArea,
                IdFinca = model.IdFinca,
                fecha_Creacion = ahora,
                PlacaVehiculo = model.PlacaVehiculo.Trim(),
                DespachadoPor = LimpiarOpcional(model.DespachadoPor),
                TransportadoPor = LimpiarOpcional(model.TransportadoPor),
                RecibidoPor = LimpiarOpcional(model.RecibidoPor),
                PesoTotal = model.PesoTotal,
                Observaciones = LimpiarOpcional(model.Observaciones),
                Owner = usuario,
                DateOwner = ahora,
                LastModify = null,
                LastDate = null
            };

            foreach (var detalle in detalles)
            {
                remision.Detalles.Add(new RemisionesDTO
                {
                    referencia = detalle.Referencia!.Trim(),
                    Cantidad = detalle.Cantidad,
                    Descripcion = detalle.Descripcion!.Trim(),
                    Owner = usuario,
                    DateOwner = ahora
                });
            }

            _context.Remisiones.Add(remision);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Remisión creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Remisiones/Edit/5
        [HttpGet]
        [Permiso("Remisiones.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var remision = await _context.Remisiones
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRemision == id);

            if (remision == null)
            {
                return NotFound();
            }

            var model = MapearViewModel(remision);
            if (model.Detalles.Count == 0)
            {
                model.Detalles.Add(new RemisionDetalleViewModel());
            }

            await CargarListasAsync(model);
            return View(model);
        }

        // POST: Remisiones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Remisiones.Editar")]
        public async Task<IActionResult> Edit(int id, RemisionViewModel model)
        {
            if (id != model.IdRemision)
            {
                return NotFound();
            }

            var remisionDb = await _context.Remisiones
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRemision == id);

            if (remisionDb == null)
            {
                return NotFound();
            }

            var detalles = ValidarDetalles(model.Detalles);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model);
                if (model.Detalles == null || model.Detalles.Count == 0)
                {
                    model.Detalles = new List<RemisionDetalleViewModel> { new RemisionDetalleViewModel() };
                }
                return View(model);
            }

            var usuario = User.ObtenerNombreUsuario();
            var ahora = DateTime.Now;

            remisionDb.NumeroRemision = model.NumeroRemision.Trim();
            remisionDb.Nombre = model.Nombre.Trim();
            remisionDb.Apellido = model.Apellido.Trim();
            remisionDb.Direccion = model.Direccion.Trim();
            remisionDb.IdArea = model.IdArea;
            remisionDb.IdFinca = model.IdFinca;
            remisionDb.PlacaVehiculo = model.PlacaVehiculo.Trim();
            remisionDb.DespachadoPor = LimpiarOpcional(model.DespachadoPor);
            remisionDb.TransportadoPor = LimpiarOpcional(model.TransportadoPor);
            remisionDb.RecibidoPor = LimpiarOpcional(model.RecibidoPor);
            remisionDb.PesoTotal = model.PesoTotal;
            remisionDb.Observaciones = LimpiarOpcional(model.Observaciones);
            remisionDb.LastModify = usuario;
            remisionDb.LastDate = ahora;

            // Sincroniza el detalle: actualiza los insumos existentes,
            // agrega los nuevos y elimina los que se quitaron del formulario.
            var publicados = detalles
                .Where(d => d.IdDetalle > 0)
                .GroupBy(d => d.IdDetalle)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var existente in remisionDb.Detalles.ToList())
            {
                if (publicados.TryGetValue(existente.IdDetalle, out var editado))
                {
                    existente.referencia = editado.Referencia!.Trim();
                    existente.Cantidad = editado.Cantidad;
                    existente.Descripcion = editado.Descripcion!.Trim();
                    existente.LastModify = usuario;
                    existente.LastDate = ahora;
                }
                else
                {
                    _context.RemisionesDTO.Remove(existente);
                }
            }

            foreach (var nuevo in detalles.Where(d => d.IdDetalle == 0))
            {
                remisionDb.Detalles.Add(new RemisionesDTO
                {
                    referencia = nuevo.Referencia!.Trim(),
                    Cantidad = nuevo.Cantidad,
                    Descripcion = nuevo.Descripcion!.Trim(),
                    Owner = usuario,
                    DateOwner = ahora
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Remisión actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Remisiones/Delete/5
        [HttpGet, ActionName("Delete")]
        [Permiso("Remisiones.Eliminar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var remision = await _context.Remisiones
                .Include(r => r.Area)
                .Include(r => r.Finca)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRemision == id);

            if (remision == null)
            {
                return NotFound();
            }

            return View(remision);
        }

        // POST: Remisiones/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permiso("Remisiones.Eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var remision = await _context.Remisiones
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRemision == id);

            if (remision == null)
            {
                return NotFound();
            }

            _context.RemisionesDTO.RemoveRange(remision.Detalles);
            _context.Remisiones.Remove(remision);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Remisión eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public bool RemisionesExists(int id)
        {
            return _context.Remisiones.Any(r => r.IdRemision == id);
        }

        /// <summary>
        /// Carga las listas de áreas y fincas para los formularios.
        /// </summary>
        private async Task CargarListasAsync(RemisionViewModel model)
        {
            model.Areas = await _context.Areas
                .OrderBy(a => a.NomArea)
                .Select(a => new SelectListItem
                {
                    Value = a.IdArea.ToString(),
                    Text = a.NomArea
                })
                .ToListAsync();

            model.Fincas = await _context.Fincas
                .OrderBy(f => f.NomFinca)
                .Select(f => new SelectListItem
                {
                    Value = f.IdFinca.ToString(),
                    Text = f.NomFinca
                })
                .ToListAsync();
        }

        private static RemisionViewModel MapearViewModel(Remisiones remision)
        {
            return new RemisionViewModel
            {
                IdRemision = remision.IdRemision,
                NumeroRemision = remision.NumeroRemision,
                Nombre = remision.Nombre,
                Apellido = remision.Apellido,
                Direccion = remision.Direccion,
                IdArea = remision.IdArea,
                IdFinca = remision.IdFinca,
                PlacaVehiculo = remision.PlacaVehiculo,
                DespachadoPor = remision.DespachadoPor,
                TransportadoPor = remision.TransportadoPor,
                RecibidoPor = remision.RecibidoPor,
                PesoTotal = remision.PesoTotal,
                Observaciones = remision.Observaciones,
                fecha_Creacion = remision.fecha_Creacion,
                Owner = remision.Owner,
                DateOwner = remision.DateOwner,
                LastModify = remision.LastModify,
                LastDate = remision.LastDate,
                Detalles = remision.Detalles
                    .OrderBy(d => d.IdDetalle)
                    .Select(d => new RemisionDetalleViewModel
                    {
                        IdDetalle = d.IdDetalle,
                        Referencia = d.referencia,
                        Cantidad = d.Cantidad,
                        Descripcion = d.Descripcion
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Valida las filas del detalle y devuelve solo las que tienen datos.
        /// Las filas completamente vacías se ignoran.
        /// </summary>
        private List<RemisionDetalleViewModel> ValidarDetalles(List<RemisionDetalleViewModel>? detalles)
        {
            var lista = detalles ?? new List<RemisionDetalleViewModel>();
            var validos = new List<RemisionDetalleViewModel>();

            for (var i = 0; i < lista.Count; i++)
            {
                var detalle = lista[i];
                var vacio = string.IsNullOrWhiteSpace(detalle.Referencia)
                            && string.IsNullOrWhiteSpace(detalle.Descripcion)
                            && detalle.Cantidad == 0;

                if (vacio)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(detalle.Referencia))
                {
                    ModelState.AddModelError($"Detalles[{i}].Referencia", "La referencia es obligatoria.");
                }

                if (detalle.Cantidad <= 0)
                {
                    ModelState.AddModelError($"Detalles[{i}].Cantidad", "La cantidad debe ser mayor que cero.");
                }

                if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                {
                    ModelState.AddModelError($"Detalles[{i}].Descripcion", "La descripción es obligatoria.");
                }

                validos.Add(detalle);
            }

            if (validos.Count == 0)
            {
                ModelState.AddModelError("Detalles", "Agrega al menos un insumo a la remisión.");
            }

            return validos;
        }

        private static string? LimpiarOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
