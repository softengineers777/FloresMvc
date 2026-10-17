using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Controllers;
using FloresMvc.Models;

namespace FloresMvc.Controllers
{
    public class RolesController : Controller
    {
        private readonly FloresDbContext _context;
        private readonly IConfiguration _configuration;

        public RolesController(FloresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
                public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }
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
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRol,Nombre")] Roles roles)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roles);
        }

                [HttpGet]
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
                    _context.Update(roles);
                    await _context.SaveChangesAsync();
                    // return RedirectToAction(nameof(Index));
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
                RedirectToAction(nameof(Index));
            }
            return View();
        }
        public bool RolesExist(int id)
        {
            return _context.Roles.Any(r => r.IdRol == id);
        }

        [HttpGet, ActionName("Delete")]
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
    }
}
