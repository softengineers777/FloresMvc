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
    public class ActaDescargosController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FloresDbContext _context;

        public ActaDescargosController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.ActaDescargos
                .AsNoTracking()
                .OrderByDescending(a => a.IdActa)
                .ToListAsync();

            ViewBag.Cargos = await _context.Cargo.AsNoTracking()
                .ToDictionaryAsync(c => c.IdCargo, c => c.NomCargo);
            ViewBag.Areas = await _context.Areas.AsNoTracking()
                .ToDictionaryAsync(a => a.IdArea, a => a.NomArea);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var acta = await _context.ActaDescargos
                .AsNoTracking()
                .Include(a => a.Preguntas)
                .FirstOrDefaultAsync(a => a.IdActa == id);
            if (acta == null) return NotFound();

            await CargarNombresAsync(acta.IdCargo, acta.IdArea);
            return View(acta);
        }

        [HttpGet]
        public async Task<IActionResult> Documento(int? id)
        {
            if (id == null) return NotFound();

            var acta = await _context.ActaDescargos
                .AsNoTracking()
                .Include(a => a.Preguntas)
                .FirstOrDefaultAsync(a => a.IdActa == id);
            if (acta == null) return NotFound();

            await CargarNombresAsync(acta.IdCargo, acta.IdArea);
            return View(acta);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ActaDescargosViewModel
            {
                fecha_Creacion = DateTime.Now,
                FechaDiligencia = DateTime.Now
            };
            await CargarListasAsync(model);
            model.Preguntas.Add(new ActaDescargosPreguntaViewModel { Orden = 1 });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActaDescargosViewModel model)
        {
            ModelState.Remove(nameof(ActaDescargosViewModel.fecha_Creacion));
            ModelState.Remove(nameof(ActaDescargosViewModel.DateOwner));
            ModelState.Remove(nameof(ActaDescargosViewModel.Owner));

            var preguntas = ValidarPreguntas(model.Preguntas);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model);
                if (model.Preguntas == null || model.Preguntas.Count == 0)
                    model.Preguntas = new List<ActaDescargosPreguntaViewModel> { new ActaDescargosPreguntaViewModel { Orden = 1 } };
                return View(model);
            }

            var usuario = User.ObtenerNombreUsuario();
            var ahora = DateTime.Now;

            var acta = new ActaDescargos
            {
                Ciudad = Limpiar(model.Ciudad),
                fecha_Creacion = ahora,
                FechaInformeDisciplinario = model.FechaInformeDisciplinario,
                NombreTrabajador = model.NombreTrabajador.Trim(),
                CcTrabajador = model.CcTrabajador.Trim(),
                DireccionResidencia = Limpiar(model.DireccionResidencia),
                Telefono = Limpiar(model.Telefono),
                IdCargo = model.IdCargo,
                IdArea = model.IdArea,
                Proceso = model.Proceso.Trim(),
                FechaIngreso = model.FechaIngreso,
                AsistidoTestigos = model.AsistidoTestigos,
                NombreTestigo = Limpiar(model.NombreTestigo),
                HoraInicio = Limpiar(model.HoraInicio),
                HoraFin = Limpiar(model.HoraFin),
                FechaDiligencia = model.FechaDiligencia,
                ConceptoGestionHumana = Limpiar(model.ConceptoGestionHumana),
                DiasSancion = model.DiasSancion,
                FechaInicioSancion = model.FechaInicioSancion,
                Owner = usuario,
                DateOwner = ahora,
                LastModify = null,
                LastDate = null
            };

            foreach (var p in preguntas)
            {
                acta.Preguntas.Add(new ActaDescargosPreguntas
                {
                    Orden = p.Orden,
                    Pregunta = p.Pregunta!.Trim(),
                    Respuesta = p.Respuesta!.Trim(),
                    Owner = usuario,
                    DateOwner = ahora
                });
            }

            _context.ActaDescargos.Add(acta);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Acta de descargos creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var acta = await _context.ActaDescargos
                .Include(a => a.Preguntas)
                .FirstOrDefaultAsync(a => a.IdActa == id);
            if (acta == null) return NotFound();

            var model = MapearViewModel(acta);
            if (model.Preguntas.Count == 0)
                model.Preguntas.Add(new ActaDescargosPreguntaViewModel { Orden = 1 });

            await CargarListasAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActaDescargosViewModel model)
        {
            if (id != model.IdActa) return NotFound();

            var actaDb = await _context.ActaDescargos
                .Include(a => a.Preguntas)
                .FirstOrDefaultAsync(a => a.IdActa == id);
            if (actaDb == null) return NotFound();

            ModelState.Remove(nameof(ActaDescargosViewModel.fecha_Creacion));
            ModelState.Remove(nameof(ActaDescargosViewModel.DateOwner));
            ModelState.Remove(nameof(ActaDescargosViewModel.Owner));

            var preguntas = ValidarPreguntas(model.Preguntas);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model);
                if (model.Preguntas == null || model.Preguntas.Count == 0)
                    model.Preguntas = new List<ActaDescargosPreguntaViewModel> { new ActaDescargosPreguntaViewModel { Orden = 1 } };
                return View(model);
            }

            var usuario = User.ObtenerNombreUsuario();
            var ahora = DateTime.Now;

            actaDb.Ciudad = Limpiar(model.Ciudad);
            actaDb.FechaInformeDisciplinario = model.FechaInformeDisciplinario;
            actaDb.NombreTrabajador = model.NombreTrabajador.Trim();
            actaDb.CcTrabajador = model.CcTrabajador.Trim();
            actaDb.DireccionResidencia = Limpiar(model.DireccionResidencia);
            actaDb.Telefono = Limpiar(model.Telefono);
            actaDb.IdCargo = model.IdCargo;
            actaDb.IdArea = model.IdArea;
            actaDb.Proceso = model.Proceso.Trim();
            actaDb.FechaIngreso = model.FechaIngreso;
            actaDb.AsistidoTestigos = model.AsistidoTestigos;
            actaDb.NombreTestigo = Limpiar(model.NombreTestigo);
            actaDb.HoraInicio = Limpiar(model.HoraInicio);
            actaDb.HoraFin = Limpiar(model.HoraFin);
            actaDb.FechaDiligencia = model.FechaDiligencia;
            actaDb.ConceptoGestionHumana = Limpiar(model.ConceptoGestionHumana);
            actaDb.DiasSancion = model.DiasSancion;
            actaDb.FechaInicioSancion = model.FechaInicioSancion;
            actaDb.LastModify = usuario;
            actaDb.LastDate = ahora;

            var publicadas = preguntas
                .Where(p => p.IdPregunta > 0)
                .GroupBy(p => p.IdPregunta)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var existente in actaDb.Preguntas.ToList())
            {
                if (publicadas.TryGetValue(existente.IdPregunta, out var editada))
                {
                    existente.Orden = editada.Orden;
                    existente.Pregunta = editada.Pregunta!.Trim();
                    existente.Respuesta = editada.Respuesta!.Trim();
                    existente.LastModify = usuario;
                    existente.LastDate = ahora;
                }
                else
                {
                    _context.ActaDescargosPreguntas.Remove(existente);
                }
            }

            foreach (var nueva in preguntas.Where(p => p.IdPregunta == 0))
            {
                actaDb.Preguntas.Add(new ActaDescargosPreguntas
                {
                    Orden = nueva.Orden,
                    Pregunta = nueva.Pregunta!.Trim(),
                    Respuesta = nueva.Respuesta!.Trim(),
                    Owner = usuario,
                    DateOwner = ahora
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Acta de descargos actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var acta = await _context.ActaDescargos
                .AsNoTracking()
                .Include(a => a.Preguntas)
                .FirstOrDefaultAsync(a => a.IdActa == id);
            if (acta == null) return NotFound();

            await CargarNombresAsync(acta.IdCargo, acta.IdArea);
            return View(acta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acta = await _context.ActaDescargos
                .Include(a => a.Preguntas)
                .FirstOrDefaultAsync(a => a.IdActa == id);
            if (acta == null) return NotFound();

            _context.ActaDescargosPreguntas.RemoveRange(acta.Preguntas);
            _context.ActaDescargos.Remove(acta);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Acta de descargos eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public bool ActaExists(int id) => _context.ActaDescargos.Any(a => a.IdActa == id);

        private async Task CargarListasAsync(ActaDescargosViewModel model)
        {
            model.Cargos = await _context.Cargo.AsNoTracking().OrderBy(c => c.NomCargo)
                .Select(c => new SelectListItem { Value = c.IdCargo.ToString(), Text = c.NomCargo }).ToListAsync();
            model.Areas = await _context.Areas.AsNoTracking().OrderBy(a => a.NomArea)
                .Select(a => new SelectListItem { Value = a.IdArea.ToString(), Text = a.NomArea }).ToListAsync();
        }

        private async Task CargarNombresAsync(int idCargo, int idArea)
        {
            ViewBag.NombreCargo = await _context.Cargo.AsNoTracking().Where(c => c.IdCargo == idCargo).Select(c => c.NomCargo).FirstOrDefaultAsync();
            ViewBag.NombreArea = await _context.Areas.AsNoTracking().Where(a => a.IdArea == idArea).Select(a => a.NomArea).FirstOrDefaultAsync();
        }

        private static ActaDescargosViewModel MapearViewModel(ActaDescargos acta)
        {
            return new ActaDescargosViewModel
            {
                IdActa = acta.IdActa,
                Ciudad = acta.Ciudad,
                fecha_Creacion = acta.fecha_Creacion,
                FechaInformeDisciplinario = acta.FechaInformeDisciplinario,
                NombreTrabajador = acta.NombreTrabajador,
                CcTrabajador = acta.CcTrabajador,
                DireccionResidencia = acta.DireccionResidencia,
                Telefono = acta.Telefono,
                IdCargo = acta.IdCargo,
                IdArea = acta.IdArea,
                Proceso = acta.Proceso,
                FechaIngreso = acta.FechaIngreso,
                AsistidoTestigos = acta.AsistidoTestigos,
                NombreTestigo = acta.NombreTestigo,
                HoraInicio = acta.HoraInicio,
                HoraFin = acta.HoraFin,
                FechaDiligencia = acta.FechaDiligencia,
                ConceptoGestionHumana = acta.ConceptoGestionHumana,
                DiasSancion = acta.DiasSancion,
                FechaInicioSancion = acta.FechaInicioSancion,
                Owner = acta.Owner,
                DateOwner = acta.DateOwner,
                LastModify = acta.LastModify,
                LastDate = acta.LastDate,
                Preguntas = acta.Preguntas.OrderBy(p => p.Orden).ThenBy(p => p.IdPregunta).Select(p => new ActaDescargosPreguntaViewModel
                {
                    IdPregunta = p.IdPregunta,
                    Orden = p.Orden,
                    Pregunta = p.Pregunta,
                    Respuesta = p.Respuesta
                }).ToList()
            };
        }

        private List<ActaDescargosPreguntaViewModel> ValidarPreguntas(List<ActaDescargosPreguntaViewModel>? preguntas)
        {
            var lista = preguntas ?? new List<ActaDescargosPreguntaViewModel>();
            var validas = new List<ActaDescargosPreguntaViewModel>();

            for (var i = 0; i < lista.Count; i++)
            {
                var p = lista[i];
                var vacia = string.IsNullOrWhiteSpace(p.Pregunta) && string.IsNullOrWhiteSpace(p.Respuesta);
                if (vacia) continue;

                if (p.Orden <= 0) p.Orden = validas.Count + 1;

                if (string.IsNullOrWhiteSpace(p.Pregunta))
                    ModelState.AddModelError($"Preguntas[{i}].Pregunta", "La pregunta es obligatoria.");
                if (string.IsNullOrWhiteSpace(p.Respuesta))
                    ModelState.AddModelError($"Preguntas[{i}].Respuesta", "La respuesta es obligatoria.");

                validas.Add(p);
            }

            if (validas.Count == 0)
                ModelState.AddModelError("Preguntas", "Agrega al menos una pregunta con su respuesta.");

            return validas;
        }

        private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
