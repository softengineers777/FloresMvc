using System.Data;
using System.ComponentModel.DataAnnotations;
using System.IO;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using FloresMvc.Extensions;
using FloresMvc.Models;
using FloresMvc.Services;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FloresMvc.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;

        public UsuariosController(
            FloresDbContext context,
            IConfiguration configuration,
            PasswordService passwordService)
        {
            _context = context;
            _configuration = configuration;
            _passwordService = passwordService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());

        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var items = await _context.Usuarios.AsNoTracking().OrderBy(u => u.IdUsuarios).ToListAsync();
            var roles = await _context.Roles.AsNoTracking().ToDictionaryAsync(r => r.IdRol, r => r.Nombre);
            string NombreRol(int id) => roles.TryGetValue(id, out var n) ? n : id.ToString();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Usuarios");
            var headers = new[] { "Consecutivo", "Nombre", "Apellido", "Correo", "Rol", "Estado", "Fecha creación", "Creado por", "Fecha registro", "Modificado por", "Última modificación" };
            for (var col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937");
                cell.Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var u in items)
            {
                ws.Cell(row, 1).Value = u.IdUsuarios;
                ws.Cell(row, 2).Value = u.Nombre ?? string.Empty;
                ws.Cell(row, 3).Value = u.Apellido ?? string.Empty;
                ws.Cell(row, 4).Value = u.email ?? string.Empty;
                ws.Cell(row, 5).Value = NombreRol(u.IdRol);
                ws.Cell(row, 6).Value = (u.Activo ?? true) ? "Activo" : "Inactivo";
                ws.Cell(row, 7).Value = u.fecha_creacion;
                ws.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 8).Value = u.Owner ?? string.Empty;
                ws.Cell(row, 9).Value = u.DateOwner;
                ws.Cell(row, 9).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 10).Value = u.LastModify ?? string.Empty;
                if (u.LastDate.HasValue) { ws.Cell(row, 11).Value = u.LastDate.Value; ws.Cell(row, 11).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        //EndPoints de  ususarios
        [HttpGet]
        public async Task<IActionResult> GetUsuariosJson()
        {
            var roles = await _context.Roles.AsNoTracking()
                .ToDictionaryAsync(r => r.IdRol, r => r.Nombre);

            var usuarios = await _context.Usuarios.AsNoTracking()
                .OrderBy(u => u.IdUsuarios)
                .ToListAsync();

            var resultado = usuarios.Select(u => new
            {
                idUsuarios = u.IdUsuarios,
                idRol = u.IdRol,
                rol = roles.TryGetValue(u.IdRol, out var nombreRol) ? nombreRol : $"Rol #{u.IdRol}",
                nombre = u.Nombre,
                apellido = u.Apellido,
                email = u.email,
                activo = u.Activo ?? true,
                fecha_creacion = u.fecha_creacion,
                owner = u.Owner,
                dateOwner = u.DateOwner,
                lastModify = u.LastModify,
                lastDate = u.LastDate
            });

            return Json(resultado);
        }
        [HttpPost]
        public async Task<IActionResult> ActualizarCelda([FromBody] UsuarioUpdateDto usuarioModificado)
        {
            if (usuarioModificado == null || usuarioModificado.IdUsuarios <= 0)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }

            if (string.IsNullOrWhiteSpace(usuarioModificado.Nombre) ||
                string.IsNullOrWhiteSpace(usuarioModificado.Apellido))
            {
                return BadRequest(new { success = false, message = "Nombre y apellido son obligatorios." });
            }

            var email = usuarioModificado.email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { success = false, message = "El correo electrónico es obligatorio." });
            }

            if (!new EmailAddressAttribute().IsValid(email))
            {
                return BadRequest(new { success = false, message = "El correo electrónico no tiene un formato válido." });
            }

            if (usuarioModificado.IdRol <= 0)
            {
                return BadRequest(new { success = false, message = "Debe seleccionar un rol válido." });
            }

            var rol = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRol == usuarioModificado.IdRol);
            if (rol == null)
            {
                return BadRequest(new { success = false, message = "El rol seleccionado no existe." });
            }

            var usuarioDb = await _context.Usuarios.FindAsync(usuarioModificado.IdUsuarios);
            if (usuarioDb == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado." });
            }

            // Evitamos que dos usuarios queden con el mismo correo.
            var emailNormalizado = email.ToLower();
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuarios != usuarioModificado.IdUsuarios &&
                               u.email.ToLower() == emailNormalizado);

            if (emailExiste)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ya existe otro usuario con ese correo electrónico."
                });
            }

            var nuevoActivo = usuarioModificado.Activo ?? usuarioDb.Activo;

            usuarioDb.IdRol = usuarioModificado.IdRol;
            usuarioDb.Nombre = usuarioModificado.Nombre.Trim();
            usuarioDb.Apellido = usuarioModificado.Apellido.Trim();
            usuarioDb.email = email;
            usuarioDb.Activo = nuevoActivo;
            usuarioDb.LastModify = User.ObtenerNombreUsuario();
            usuarioDb.LastDate = DateTime.UtcNow;

            // Sincroniza el acceso de login con el estado activo/inactivo del usuario.
            var credencial = await _context.Login.FirstOrDefaultAsync(l => l.user_id == usuarioDb.IdUsuarios);
            if (credencial != null)
            {
                credencial.activo = nuevoActivo ?? true;
                credencial.LastModify = User.ObtenerNombreUsuario();
                credencial.LastDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            try
            {
                // La entidad ya está siendo rastreada por EF, no hace falta Update().
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var detalle = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo guardar el cambio en la base de datos.",
                    detail = detalle
                });
            }

            return Json(new
            {
                success = true,
                message = "Actualización correcta.",
                idUsuarios = usuarioDb.IdUsuarios,
                idRol = usuarioDb.IdRol,
                rol = rol.Nombre,
                nombre = usuarioDb.Nombre,
                apellido = usuarioDb.Apellido,
                email = usuarioDb.email,
                activo = usuarioDb.Activo ?? true,
                lastModify = usuarioDb.LastModify,
                lastDate = usuarioDb.LastDate
            });
        }
        //EndPoints de  ususarios
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }

            var Usuarios = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuarios == id);

            if (Usuarios == null)
            {
                return NotFound();
            }
            return View(Usuarios);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] UsuarioCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Datos inválidos." });
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Apellido) ||
                string.IsNullOrWhiteSpace(dto.email))
            {
                return BadRequest(new { success = false, message = "Nombre, apellido y correo son obligatorios." });
            }

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                return BadRequest(new { success = false, message = "La contraseña debe tener al menos 6 caracteres." });
            }

            if (!string.IsNullOrWhiteSpace(dto.ConfirmPassword) && dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new { success = false, message = "Las contraseñas no coinciden." });
            }

            if (dto.IdRol <= 0)
            {
                return BadRequest(new { success = false, message = "Debe seleccionar un rol válido." });
            }

            // La tabla Usuarios tiene una llave foránea hacia Roles: verificamos
            // que el rol exista antes de intentar insertar.
            var rolExiste = await _context.Roles.AnyAsync(r => r.IdRol == dto.IdRol);
            if (!rolExiste)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El rol seleccionado no existe. Cree primero un rol e inténtelo de nuevo."
                });
            }

            var emailNormalizado = dto.email.Trim().ToLower();
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.email.ToLower() == emailNormalizado);

            if (emailExiste)
            {
                return BadRequest(new { success = false, message = "Ya existe un usuario con ese correo electrónico." });
            }

            var activo = dto.Activo ?? true;
            var usuario = new Usuarios
            {
                IdRol = dto.IdRol,
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                email = dto.email.Trim(),
                Activo = activo,
                // PostgreSQL (timestamp with time zone) solo acepta fechas en UTC.
                fecha_creacion = dto.fecha_creacion.HasValue
                    ? NormalizarAUtc(dto.fecha_creacion.Value)
                    : DateTime.UtcNow,
                Owner = User.ObtenerNombreUsuario(),
                DateOwner = DateTime.UtcNow,
                LastModify = null,
                LastDate = null
            };

            // Guardamos el usuario y sus credenciales en una sola transacción:
            // si falla el login, no queda un usuario sin acceso.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var credencial = new Login
                {
                    user_id = usuario.IdUsuarios,
                    // Se guarda el hash, nunca la contraseña en texto plano.
                    password_hash = _passwordService.Hash(dto.Password),
                    // La columna es timestamp without time zone: Kind=Unspecified.
                    ultimo_acceso = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    activo = activo,
                    Owner = User.ObtenerNombreUsuario(),
                    DateOwner = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    LastModify = null,
                    LastDate = null
                };

                _context.Login.Add(credencial);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                var detalle = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo guardar el usuario en la base de datos.",
                    detail = detalle
                });
            }

            return Json(new
            {
                success = true,
                message = "Usuario creado con éxito. Ya puede iniciar sesión con su correo y contraseña.",
                idUsuarios = usuario.IdUsuarios
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuarios,IdRol,Nombre,Apellido,fecha_creacion,email")] Usuarios usuarios)
        {
            if (ModelState.IsValid)
            {
                usuarios.Owner = User.ObtenerNombreUsuario();
                usuarios.DateOwner = DateTime.Now;
                _context.Add(usuarios);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuarios);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuarios,IdRol,Nombre,Apellido,fecha_creacion")] Usuarios usuarios)
        {
            if (id != usuarios.IdUsuarios)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var usuarioDb = await _context.Usuarios.FindAsync(id);
                if (usuarioDb == null)
                {
                    return NotFound();
                }

                try
                {
                    // Se conservan los datos de creación y solo se registra
                    // quién hizo la última modificación.
                    usuarioDb.IdRol = usuarios.IdRol;
                    usuarioDb.Nombre = usuarios.Nombre;
                    usuarioDb.Apellido = usuarios.Apellido;
                    usuarioDb.LastModify = User.ObtenerNombreUsuario();
                    usuarioDb.LastDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuariosExixst(usuarios.IdUsuarios))
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
            return View(usuarios);
        }
        public bool UsuariosExixst(int id)
        {
            return _context.Usuarios.Any(u => u.IdUsuarios == id);
        }

        /// <summary>
        /// PostgreSQL guarda las columnas "timestamp with time zone" en UTC;
        /// esta función adapta cualquier DateTime antes de persistirlo.
        /// </summary>
        private static DateTime NormalizarAUtc(DateTime fecha)
        {
            return fecha.Kind switch
            {
                DateTimeKind.Utc => fecha,
                DateTimeKind.Local => fecha.ToUniversalTime(),
                _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            };
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var usuarios = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuarios == id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            _context.Usuarios.Remove(usuarios);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}