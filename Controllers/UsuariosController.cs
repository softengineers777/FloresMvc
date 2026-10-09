using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FloresMvc.Controllers;
using System.Data;
using System.ComponentModel.DataAnnotations;
using FloresMvc.Models;
using System.Runtime.CompilerServices;

namespace FloresMvc.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController (FloresDbContext context, IConfiguration configuration )
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task <IActionResult> Index ()
        {
            return View (await _context.Usuarios.ToListAsync());

        }
        public async Task <IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }

            var Usuarios = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuarios == id );

            if (Usuarios == null)
            {
                return NotFound();
            }
            return View (Usuarios);
        }
        [HttpGet]
        public async Task <IActionResult>Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        


    }
}