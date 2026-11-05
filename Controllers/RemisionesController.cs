using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Extensions;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
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
        public IActionResult Index()
        {
            return View();
        }

        // GET: Remisiones/GetRemisionesJson
        // Datos para Tabulator. Se proyecta solo lo que necesita el index
        // para evitar cargar el detalle completo de cada remisión.
        [HttpGet]
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

        // GET: Remisiones/Create
        [HttpGet]
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
