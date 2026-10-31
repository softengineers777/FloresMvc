using System;
using FloresMvc.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FloresMvc.Models
{
    public class FloresDbContext : DbContext
    {
        // Constructor principal que recibe las opciones (cadena de conexión, etc.)
        public FloresDbContext(DbContextOptions<FloresDbContext> options)
            : base(options)
        { 
        }

        public FloresDbContext()
        {
        }

        // Mapeo de las tablas (DbSet)
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Login> Login { get; set; }
        public DbSet<Areas> Areas { get; set; }
        public DbSet<Fincas> Fincas { get; set; }
        public DbSet<Presupuesto> Presupuesto { get; set; }
        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<SubCategorias> SubCategorias { get; set; }

        protected override void  OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuarios>(entity=>
            {
                entity.HasKey(u=> u.IdUsuarios);
                entity.Property(u=>u.Nombre);
                entity.Property(u=>u.Apellido);
                entity.Property(u=>u.email);
                entity.Property(u=>u.fecha_creacion);
            });

            modelBuilder.Entity<Roles>(entity=>
            {
               entity.HasKey(r=>r.IdRol); 
               entity.Property(r=>r.Nombre); 
               entity.Property(p=>p.fecha_Creacion);
            });

            modelBuilder.Entity<Login>(entity =>
            {
               entity.HasKey(l=>l.IdLogin);
               entity.Property(l=>l.user_id);
               entity.Property(l=>l.password_hash);
               entity.Property(l=>l.ultimo_acceso);
               entity.Property(l=>l.activo); 
            });
            modelBuilder.Entity<Fincas>(entity =>
            {
               entity.HasKey(f=>f.IdFinca);
               entity.Property(f=>f.NomFinca); 
               entity.Property(p=>p.fecha_Creacion);
            });
            modelBuilder.Entity<Areas>(entity =>
            {
               entity.HasKey(a=>a.IdArea);
               entity.Property(a=>a.NomArea); 
               entity.Property(p=>p.fecha_Creacion);
            });
            modelBuilder.Entity<Presupuesto>(entity =>
            {
               entity.HasKey(p=>p.IdPresupuesto);
               entity.Property(p=>p.IdArea);
               entity.Property(p=>p.IdFinca);
               entity.Property(p=>p.IdCategoria);
               entity.Property(p=>p.IdSubcategoria);
               entity.Property(p=>p.fecha_Creacion);
               entity.Property(p=>p.Nombre);
               entity.Property(p=>p.Apellido);
               entity.Property(p=>p.Cedula);
               entity.Property(p=>p.Subtotal);
               entity.Property(p=>p.Total);
               entity.Property(p=>p.Observaciones);            
            });
            modelBuilder.Entity<Categorias>(entity =>
            {
               entity.HasKey(c=> c.IdCategoria);
               entity.Property(c=> c.Nomcategoria);
               entity.Property(p=>p.fecha_Creacion);

            });
            modelBuilder.Entity<SubCategorias>(entity =>
            {
               entity.HasKey(c=> c.IdSubcategoria);
               entity.Property(c=> c.NomSubcategoria); 
               entity.Property(p=>p.fecha_Creacion);

            });

        }
    }
}