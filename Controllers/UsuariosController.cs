using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using System.Data;
using System.ComponentModel.DataAnnotations;
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

        //EndPoints de  ususarios
        [HttpGet]
        public async Task<IActionResult> GetUsuariosJson()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Json(usuarios);
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

            usuarioDb.Nombre = usuarioModificado.Nombre.Trim();
            usuarioDb.Apellido = usuarioModificado.Apellido.Trim();
            usuarioDb.email = email;

            try
            {
                // La entidad ya está siendo rastreada por EF, no hace falta Update().
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo guardar el cambio en la base de datos."
                });
            }

            return Json(new
            {
                success = true,
                message = "Actualización correcta.",
                idUsuarios = usuarioDb.IdUsuarios,
                nombre = usuarioDb.Nombre,
                apellido = usuarioDb.Apellido,
                email = usuarioDb.email
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

            var usuario = new Usuarios
            {
                IdRol = dto.IdRol,
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                email = dto.email.Trim(),
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
                    activo = true,
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
                    usuarioDb.LastDate = DateTime.Now;

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