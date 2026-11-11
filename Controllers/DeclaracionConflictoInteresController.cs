using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloresMvc.Extensions;
using FloresMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FloresMvc.Controllers
{
    [Authorize]
    public class DeclaracionConflictoInteresController : Controller
    {
        private readonly FloresDbContext _context;

        public DeclaracionConflictoInteresController(FloresDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.DeclaracionConflictoInteres
                .AsNoTracking()
                .Include(d => d.Usuario)
                .Include(d => d.Cargo)
                .Include(d => d.Area)
                .OrderByDescending(d => d.IdDeclaracion)
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var declaracion = await _context.DeclaracionConflictoInteres
                .AsNoTracking()
                .Include(d => d.Usuario)
                .Include(d => d.Cargo)
                .Include(d => d.Area)
                .Include(d => d.Preguntas)
                    .ThenInclude(p => p.Pregunta)
                .FirstOrDefaultAsync(d => d.IdDeclaracion == id);

            if (declaracion == null) return NotFound();

            return View(declaracion);
        }

        [HttpGet]
        public async Task<IActionResult> Documento(int? id)
        {
            if (id == null) return NotFound();

            var declaracion = await _context.DeclaracionConflictoInteres
                .AsNoTracking()
                .Include(d => d.Usuario)
                .Include(d => d.Cargo)
                .Include(d => d.Area)
                .Include(d => d.Preguntas)
                    .ThenInclude(p => p.Pregunta)
                .FirstOrDefaultAsync(d => d.IdDeclaracion == id);

            if (declaracion == null) return NotFound();

            declaracion.Preguntas = declaracion.Preguntas
                .OrderBy(p => p.Pregunta?.IdPregunta ?? p.IdPregunta)
                .ToList();

            return View(declaracion);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DeclaracionConflictoInteresViewModel
            {
                FechaDeclaracion = DateTime.Today,
                TipoDoc = "CC"
            };

            await CargarListasAsync(model);
            await CargarPreguntasAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeclaracionConflictoInteresViewModel model)
        {
            NormalizarModelo(model);
            await ValidarModeloAsync(model, esEdicion: false);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model);
                await CompletarCatalogoPreguntasAsync(model);
                return View(model);
            }

            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var usuarioActual = User.ObtenerNombreUsuario();

            var declaracion = new DeclaracionConflictoInteres
            {
                IdUsuario = model.IdUsuario,
                TipoDoc = string.IsNullOrWhiteSpace(model.TipoDoc) ? "CC" : model.TipoDoc.Trim(),
                NumeroDocumento = model.NumeroDocumento.Trim(),
                IdCargo = model.IdCargo,
                IdArea = model.IdArea,
                FechaDeclaracion = model.FechaDeclaracion.Date,
                NombreFamiliar = Limpiar(model.NombreFamiliar),
                CargoFamiliar = Limpiar(model.CargoFamiliar),
                AreaFamiliar = Limpiar(model.AreaFamiliar),
                Observaciones = Limpiar(model.Observaciones),
                Owner = usuarioActual,
                DateOwner = ahora
            };

            foreach (var item in model.Preguntas)
            {
                declaracion.Preguntas.Add(new DeclaracionConflictoInteresPreguntas
                {
                    IdPregunta = item.IdPregunta,
                    Aplica = item.Aplica
                });
            }

            _context.DeclaracionConflictoInteres.Add(declaracion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Declaración de conflicto de interés creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var declaracion = await _context.DeclaracionConflictoInteres
                .Include(d => d.Preguntas)
                .FirstOrDefaultAsync(d => d.IdDeclaracion == id);

            if (declaracion == null) return NotFound();

            var model = MapearViewModel(declaracion);
            await CargarListasAsync(model);
            await CargarPreguntasAsync(model, declaracion.Preguntas.ToList());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeclaracionConflictoInteresViewModel model)
        {
            if (id != model.IdDeclaracion) return NotFound();

            var declaracionDb = await _context.DeclaracionConflictoInteres
                .Include(d => d.Preguntas)
                .FirstOrDefaultAsync(d => d.IdDeclaracion == id);

            if (declaracionDb == null) return NotFound();

            NormalizarModelo(model);
            await ValidarModeloAsync(model, esEdicion: true);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model);
                await CompletarCatalogoPreguntasAsync(model, declaracionDb.Preguntas.ToList());
                return View(model);
            }

            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var usuarioActual = User.ObtenerNombreUsuario();

            declaracionDb.IdUsuario = model.IdUsuario;
            declaracionDb.TipoDoc = string.IsNullOrWhiteSpace(model.TipoDoc) ? "CC" : model.TipoDoc.Trim();
            declaracionDb.NumeroDocumento = model.NumeroDocumento.Trim();
            declaracionDb.IdCargo = model.IdCargo;
            declaracionDb.IdArea = model.IdArea;
            declaracionDb.FechaDeclaracion = model.FechaDeclaracion.Date;
            declaracionDb.NombreFamiliar = Limpiar(model.NombreFamiliar);
            declaracionDb.CargoFamiliar = Limpiar(model.CargoFamiliar);
            declaracionDb.AreaFamiliar = Limpiar(model.AreaFamiliar);
            declaracionDb.Observaciones = Limpiar(model.Observaciones);
            declaracionDb.LastModify = usuarioActual;
            declaracionDb.LastDate = ahora;

            var existentes = declaracionDb.Preguntas.ToDictionary(p => p.IdPregunta, p => p);
            foreach (var item in model.Preguntas)
            {
                if (existentes.TryGetValue(item.IdPregunta, out var respuesta))
                {
                    respuesta.Aplica = item.Aplica;
                }
                else
                {
                    declaracionDb.Preguntas.Add(new DeclaracionConflictoInteresPreguntas
                    {
                        IdPregunta = item.IdPregunta,
                        Aplica = item.Aplica
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Declaración de conflicto de interés actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var declaracion = await _context.DeclaracionConflictoInteres
                .AsNoTracking()
                .Include(d => d.Usuario)
                .Include(d => d.Cargo)
                .Include(d => d.Area)
                .Include(d => d.Preguntas)
                    .ThenInclude(p => p.Pregunta)
                .FirstOrDefaultAsync(d => d.IdDeclaracion == id);

            if (declaracion == null) return NotFound();

            return View(declaracion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var declaracion = await _context.DeclaracionConflictoInteres
                .Include(d => d.Preguntas)
                .FirstOrDefaultAsync(d => d.IdDeclaracion == id);

            if (declaracion == null) return NotFound();

            _context.DeclaracionConflictoInteresPreguntas.RemoveRange(declaracion.Preguntas);
            _context.DeclaracionConflictoInteres.Remove(declaracion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Declaración de conflicto de interés eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private static DeclaracionConflictoInteresViewModel MapearViewModel(DeclaracionConflictoInteres declaracion)
        {
            return new DeclaracionConflictoInteresViewModel
            {
                IdDeclaracion = declaracion.IdDeclaracion,
                IdUsuario = declaracion.IdUsuario,
                TipoDoc = declaracion.TipoDoc,
                NumeroDocumento = declaracion.NumeroDocumento,
                IdCargo = declaracion.IdCargo,
                IdArea = declaracion.IdArea,
                FechaDeclaracion = declaracion.FechaDeclaracion,
                NombreFamiliar = declaracion.NombreFamiliar,
                CargoFamiliar = declaracion.CargoFamiliar,
                AreaFamiliar = declaracion.AreaFamiliar,
                Observaciones = declaracion.Observaciones,
                Owner = declaracion.Owner,
                DateOwner = declaracion.DateOwner,
                LastModify = declaracion.LastModify,
                LastDate = declaracion.LastDate
            };
        }

        private async Task CargarListasAsync(DeclaracionConflictoInteresViewModel model)
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Nombre).ThenBy(u => u.Apellido)
                .Select(u => new { u.IdUsuarios, Nombre = (u.Nombre + " " + u.Apellido).Trim() })
                .ToListAsync();
            model.Usuarios = usuarios.Select(u => new SelectListItem { Value = u.IdUsuarios.ToString(), Text = u.Nombre, Selected = u.IdUsuarios == model.IdUsuario });

            model.Cargos = await _context.Cargo
                .AsNoTracking()
                .OrderBy(c => c.NomCargo)
                .Select(c => new SelectListItem { Value = c.IdCargo.ToString(), Text = c.NomCargo, Selected = c.IdCargo == model.IdCargo })
                .ToListAsync();

            model.Areas = await _context.Areas
                .AsNoTracking()
                .OrderBy(a => a.NomArea)
                .Select(a => new SelectListItem { Value = a.IdArea.ToString(), Text = a.NomArea, Selected = a.IdArea == model.IdArea })
                .ToListAsync();
        }

        private async Task CargarPreguntasAsync(DeclaracionConflictoInteresViewModel model, List<DeclaracionConflictoInteresPreguntas>? respuestas = null)
        {
            var catalogo = await _context.PreguntasConflictoInteres
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.IdPregunta)
                .ToListAsync();

            var mapa = (respuestas ?? new List<DeclaracionConflictoInteresPreguntas>())
                .ToDictionary(r => r.IdPregunta, r => r);

            model.Preguntas = catalogo.Select(p => new DeclaracionConflictoPreguntaItemViewModel
            {
                IdDeclaracionPregunta = mapa.TryGetValue(p.IdPregunta, out var r) ? r.IdDeclaracionPregunta : 0,
                IdPregunta = p.IdPregunta,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                Aplica = mapa.TryGetValue(p.IdPregunta, out var r2) && r2.Aplica
            }).ToList();
        }

        private async Task CompletarCatalogoPreguntasAsync(DeclaracionConflictoInteresViewModel model, List<DeclaracionConflictoInteresPreguntas>? respuestasExistentes = null)
        {
            var catalogo = await _context.PreguntasConflictoInteres
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.IdPregunta)
                .ToListAsync();

            var mapaPost = (model.Preguntas ?? new List<DeclaracionConflictoPreguntaItemViewModel>())
                .ToDictionary(p => p.IdPregunta, p => p);
            var mapaExistente = (respuestasExistentes ?? new List<DeclaracionConflictoInteresPreguntas>())
                .ToDictionary(p => p.IdPregunta, p => p);

            model.Preguntas = catalogo.Select(p =>
            {
                mapaPost.TryGetValue(p.IdPregunta, out var posted);
                mapaExistente.TryGetValue(p.IdPregunta, out var existente);
                return new DeclaracionConflictoPreguntaItemViewModel
                {
                    IdDeclaracionPregunta = posted?.IdDeclaracionPregunta ?? existente?.IdDeclaracionPregunta ?? 0,
                    IdPregunta = p.IdPregunta,
                    Codigo = p.Codigo,
                    Descripcion = p.Descripcion,
                    Aplica = posted?.Aplica ?? existente?.Aplica ?? false
                };
            }).ToList();
        }

        private async Task ValidarModeloAsync(DeclaracionConflictoInteresViewModel model, bool esEdicion)
        {
            if (model.IdUsuario <= 0 || !await _context.Usuarios.AnyAsync(u => u.IdUsuarios == model.IdUsuario))
                ModelState.AddModelError(nameof(model.IdUsuario), "Seleccione un usuario válido.");

            if (model.IdCargo.HasValue && !await _context.Cargo.AnyAsync(c => c.IdCargo == model.IdCargo.Value))
                ModelState.AddModelError(nameof(model.IdCargo), "Seleccione un cargo válido.");

            if (model.IdArea.HasValue && !await _context.Areas.AnyAsync(a => a.IdArea == model.IdArea.Value))
                ModelState.AddModelError(nameof(model.IdArea), "Seleccione un área válida.");

            if (string.IsNullOrWhiteSpace(model.NumeroDocumento))
                ModelState.AddModelError(nameof(model.NumeroDocumento), "El número de documento es obligatorio.");

            if (model.FechaDeclaracion == default)
                ModelState.AddModelError(nameof(model.FechaDeclaracion), "La fecha de declaración es obligatoria.");
        }

        private static void NormalizarModelo(DeclaracionConflictoInteresViewModel model)
        {
            model.TipoDoc = string.IsNullOrWhiteSpace(model.TipoDoc) ? "CC" : model.TipoDoc.Trim();
            model.NumeroDocumento = model.NumeroDocumento?.Trim() ?? string.Empty;
            model.NombreFamiliar = Limpiar(model.NombreFamiliar);
            model.CargoFamiliar = Limpiar(model.CargoFamiliar);
            model.AreaFamiliar = Limpiar(model.AreaFamiliar);
            model.Observaciones = Limpiar(model.Observaciones);
            model.Preguntas ??= new List<DeclaracionConflictoPreguntaItemViewModel>();
        }

        private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
