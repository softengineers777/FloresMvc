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
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<Remisiones> Remisiones { get; set; }
        public DbSet<RemisionesDTO> RemisionesDTO { get; set; }

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
               entity.ToTable("Roles");
               entity.HasKey(r=>r.IdRol);
               entity.Property(r=>r.IdRol).HasColumnName("IdRol");
               entity.Property(r=>r.Nombre).HasColumnName("Nombre").IsRequired();
               entity.Property(r=>r.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp with time zone");
               entity.Property(r=>r.Owner).HasColumnName("Owner");
               entity.Property(r=>r.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
               entity.Property(r=>r.LastModify).HasColumnName("LastModify");
               entity.Property(r=>r.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Login>(entity =>
            {
               // La tabla real en PostgreSQL es "login" y sus columnas están
               // en minúsculas, por eso se mapean explícitamente.
               entity.ToTable("login");
               entity.HasKey(l => l.IdLogin);
               entity.Property(l => l.IdLogin).HasColumnName("idlogin");
               entity.Property(l => l.user_id).HasColumnName("user_id");
               entity.Property(l => l.password_hash).HasColumnName("password_hash");
               entity.Property(l => l.ultimo_acceso).HasColumnName("ultimo_acceso").HasColumnType("timestamp without time zone");
               entity.Property(l => l.activo).HasColumnName("activo");
               entity.Property(l => l.Owner).HasColumnName("owner");
               entity.Property(l => l.DateOwner).HasColumnName("dateowner").HasColumnType("timestamp without time zone");
               entity.Property(l => l.LastModify).HasColumnName("lastmodify");
               entity.Property(l => l.LastDate).HasColumnName("lastdate").HasColumnType("timestamp without time zone");
            });
            modelBuilder.Entity<Fincas>(entity =>
            {
               // La tabla y sus columnas están en minúscula en PostgreSQL.
               entity.ToTable("fincas");
               entity.HasKey(f=>f.IdFinca);
               entity.Property(f=>f.IdFinca).HasColumnName("idfincas");
               entity.Property(f=>f.NomFinca).HasColumnName("nomfinca");
               entity.Property(f=>f.fecha_Creacion).HasColumnName("fecha_creacion").HasColumnType("timestamp without time zone");
               entity.Property(f=>f.Owner).HasColumnName("owner");
               entity.Property(f=>f.DateOwner).HasColumnName("dateowner").HasColumnType("timestamp without time zone");
               entity.Property(f=>f.LastModify).HasColumnName("lastmodify");
               entity.Property(f=>f.LastDate).HasColumnName("lastdate").HasColumnType("timestamp without time zone");
            });
            modelBuilder.Entity<Areas>(entity =>
            {
               entity.ToTable("areas");
               entity.HasKey(a=>a.IdArea);
               entity.Property(a=>a.IdArea).HasColumnName("idarea");
               entity.Property(a=>a.NomArea).HasColumnName("nomarea");
               entity.Property(a=>a.fecha_Creacion).HasColumnName("fecha_creacion").HasColumnType("timestamp without time zone");
               entity.Property(a=>a.Owner).HasColumnName("owner");
               entity.Property(a=>a.DateOwner).HasColumnName("dateowner").HasColumnType("timestamp without time zone");
               entity.Property(a=>a.LastModify).HasColumnName("lastmodify");
               entity.Property(a=>a.LastDate).HasColumnName("lastdate").HasColumnType("timestamp without time zone");
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
               entity.Property(c=> c.fecha_Creacion).HasColumnType("date");
               entity.Property(c=> c.Owner).HasColumnName("Owner");
               entity.Property(c=> c.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
               entity.Property(c=> c.LastModify).HasColumnName("LastModify");
               entity.Property(c=> c.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");

            });
            modelBuilder.Entity<SubCategorias>(entity =>
            {
               entity.ToTable("SubCategorias");
               entity.HasKey(c => c.IdSubcategoria);
               entity.Property(c => c.IdSubcategoria).HasColumnName("IdSubcategoria");
               entity.Property(c => c.NomSubcategoria).HasColumnName("NomSubcategoria");
               // Las columnas de fecha son "timestamp without time zone".
               // Sin este mapeo Npgsql las trata como "timestamp with time zone"
               // y rechaza DateTime.Now (Kind=Local).
               entity.Property(c => c.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp without time zone");
               entity.Property(c => c.Owner).HasColumnName("Owner");
               entity.Property(c => c.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
               entity.Property(c => c.LastModify).HasColumnName("LastModify");
               entity.Property(c => c.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
            });
            modelBuilder.Entity<Inventario>(entity =>
            {
               entity.HasKey(i=> i.IdInventario);
               entity.Property(i=> i.Nombre);
               entity.Property(i=> i.Descripcion);
               entity.Property(i=> i.Cantidad);
               entity.Property(i=> i.Precio);
               entity.Property(i=> i.fecha_Creacion);
            });

            modelBuilder.Entity<Remisiones>(entity =>
            {
               // La tabla y sus columnas se crearon con identificadores
               // capitalizados, por eso se mapean explícitamente.
               entity.ToTable("Remisiones");
               entity.HasKey(r => r.IdRemision);
               entity.Property(r => r.IdRemision).HasColumnName("IdRemision");
               entity.Property(r => r.NumeroRemision).HasColumnName("NumeroRemision").HasMaxLength(50).IsRequired();
               entity.Property(r => r.Nombre).HasColumnName("Nombre").HasMaxLength(50).IsRequired();
               entity.Property(r => r.Apellido).HasColumnName("Apellido").HasMaxLength(50).IsRequired();
               entity.Property(r => r.Direccion).HasColumnName("Direccion").HasMaxLength(255).IsRequired();
               entity.Property(r => r.IdArea).HasColumnName("IdArea");
               entity.Property(r => r.IdFinca).HasColumnName("IdFincas");
               entity.Property(r => r.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp without time zone");
               entity.Property(r => r.PlacaVehiculo).HasColumnName("PlacaVehiculo").HasMaxLength(20).IsRequired();
               entity.Property(r => r.DespachadoPor).HasColumnName("DespachadoPor").HasMaxLength(100);
               entity.Property(r => r.TransportadoPor).HasColumnName("TransportadoPor").HasMaxLength(100);
               entity.Property(r => r.RecibidoPor).HasColumnName("RecibidoPor").HasMaxLength(100);
               entity.Property(r => r.PesoTotal).HasColumnName("PesoTotal");
               entity.Property(r => r.Observaciones).HasColumnName("Observaciones").HasMaxLength(255);
               entity.Property(r => r.Owner).HasColumnName("Owner").HasMaxLength(30);
               entity.Property(r => r.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
               entity.Property(r => r.LastModify).HasColumnName("LastModify").HasMaxLength(30);
               entity.Property(r => r.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");

               entity.HasOne(r => r.Area)
                     .WithMany()
                     .HasForeignKey(r => r.IdArea)
                     .OnDelete(DeleteBehavior.Restrict);

               entity.HasOne(r => r.Finca)
                     .WithMany()
                     .HasForeignKey(r => r.IdFinca)
                     .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RemisionesDTO>(entity =>
            {
               entity.ToTable("RemisionesDTO");
               entity.HasKey(d => d.IdDetalle);
               entity.Property(d => d.IdDetalle).HasColumnName("IdDetalle");
               entity.Property(d => d.IdRemision).HasColumnName("IdRemision");
               entity.Property(d => d.referencia).HasColumnName("referencia").HasMaxLength(100).IsRequired();
               entity.Property(d => d.Cantidad).HasColumnName("Cantidad");
               entity.Property(d => d.Descripcion).HasColumnName("Descripcion").HasMaxLength(255).IsRequired();
               entity.Property(d => d.Owner).HasColumnName("Owner").HasMaxLength(30);
               entity.Property(d => d.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
               entity.Property(d => d.LastModify).HasColumnName("LastModify").HasMaxLength(30);
               entity.Property(d => d.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");

               entity.HasOne(d => d.Remision)
                     .WithMany(r => r.Detalles)
                     .HasForeignKey(d => d.IdRemision)
                     .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}