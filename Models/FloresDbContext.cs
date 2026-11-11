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
      public DbSet<ModeloDisciplinario> ModeloDisciplinario { get; set; }
      public DbSet<CitacionDescargos> CitacionDescargos { get; set; }
      public DbSet<ActaDescargos> ActaDescargos { get; set; }
      public DbSet<ActaDescargosPreguntas> ActaDescargosPreguntas { get; set; }
      public DbSet<Cargo> Cargo { get; set; }
      public DbSet<Sucursal>Sucursal {get; set;}
      public DbSet<Colaborador>Colaborador {get; set;}
      public DbSet<ConsultaPeps> ConsultaPeps { get; set; }
      public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
      public DbSet<Permiso> Permisos { get; set; }
      public DbSet<RolPermiso> RolPermisos { get; set; }
      public DbSet<PreguntasConflictoInteres> PreguntasConflictoInteres { get; set; }
      public DbSet<DeclaracionConflictoInteres> DeclaracionConflictoInteres { get; set; }
      public DbSet<DeclaracionConflictoInteresPreguntas> DeclaracionConflictoInteresPreguntas { get; set; }




      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);

         modelBuilder.Entity<Usuarios>(entity =>
         {
            entity.HasKey(u => u.IdUsuarios);
            entity.Property(u => u.Nombre);
            entity.Property(u => u.Apellido);
            entity.Property(u => u.email);
            entity.Property(u => u.Activo).HasColumnName("Activo").HasDefaultValue(true);
            entity.Property(u => u.fecha_creacion);
         });

         modelBuilder.Entity<Roles>(entity =>
         {
            entity.ToTable("Roles");
            entity.HasKey(r => r.IdRol);
            entity.Property(r => r.IdRol).HasColumnName("IdRol");
            entity.Property(r => r.Nombre).HasColumnName("Nombre").IsRequired();
            entity.Property(r => r.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp with time zone");
            entity.Property(r => r.Owner).HasColumnName("Owner");
            entity.Property(r => r.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(r => r.LastModify).HasColumnName("LastModify");
            entity.Property(r => r.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
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
            entity.HasKey(f => f.IdFinca);
            entity.Property(f => f.IdFinca).HasColumnName("idfincas");
            entity.Property(f => f.NomFinca).HasColumnName("nomfinca");
            entity.Property(f => f.fecha_Creacion).HasColumnName("fecha_creacion").HasColumnType("timestamp without time zone");
            entity.Property(f => f.Owner).HasColumnName("owner");
            entity.Property(f => f.DateOwner).HasColumnName("dateowner").HasColumnType("timestamp without time zone");
            entity.Property(f => f.LastModify).HasColumnName("lastmodify");
            entity.Property(f => f.LastDate).HasColumnName("lastdate").HasColumnType("timestamp without time zone");
         });
         modelBuilder.Entity<Areas>(entity =>
         {
            entity.ToTable("areas");
            entity.HasKey(a => a.IdArea);
            entity.Property(a => a.IdArea).HasColumnName("idarea");
            entity.Property(a => a.NomArea).HasColumnName("nomarea");
            entity.Property(a => a.fecha_Creacion).HasColumnName("fecha_creacion").HasColumnType("timestamp without time zone");
            entity.Property(a => a.Owner).HasColumnName("owner");
            entity.Property(a => a.DateOwner).HasColumnName("dateowner").HasColumnType("timestamp without time zone");
            entity.Property(a => a.LastModify).HasColumnName("lastmodify");
            entity.Property(a => a.LastDate).HasColumnName("lastdate").HasColumnType("timestamp without time zone");
         });
         modelBuilder.Entity<Presupuesto>(entity =>
         {
            entity.HasKey(p => p.IdPresupuesto);
            entity.Property(p => p.IdArea);
            entity.Property(p => p.IdFinca);
            entity.Property(p => p.IdCategoria);
            entity.Property(p => p.IdSubcategoria);
            entity.Property(p => p.fecha_Creacion);
            entity.Property(p => p.Nombre);
            entity.Property(p => p.Apellido);
            entity.Property(p => p.Cedula);
            entity.Property(p => p.Subtotal);
            entity.Property(p => p.Total);
            entity.Property(p => p.Observaciones);
         });
         modelBuilder.Entity<Categorias>(entity =>
         {
            entity.HasKey(c => c.IdCategoria);
            entity.Property(c => c.Nomcategoria);
            entity.Property(c => c.fecha_Creacion).HasColumnType("date");
            entity.Property(c => c.Owner).HasColumnName("Owner");
            entity.Property(c => c.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(c => c.LastModify).HasColumnName("LastModify");
            entity.Property(c => c.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");

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
            entity.HasKey(i => i.IdInventario);
            entity.Property(i => i.Nombre);
            entity.Property(i => i.Descripcion);
            entity.Property(i => i.Cantidad);
            entity.Property(i => i.Precio);
            entity.Property(i => i.fecha_Creacion);
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

         modelBuilder.Entity<ModeloDisciplinario>(entity =>
         {
            entity.ToTable("ModeloDisciplinario");
            entity.HasKey(md => md.IdModelo);
            entity.Property(md => md.IdModelo).HasColumnName("IdModelo");
            entity.Property(md => md.IdArea).HasColumnName("IdArea");
            entity.Property(md => md.fecha_creacion).HasColumnName("fecha_creacion").HasColumnType("timestamp without time zone");
            entity.Property(md => md.para).HasColumnName("para").HasMaxLength(50).IsRequired();
            entity.Property(md => md.asunto).HasColumnName("asunto").HasMaxLength(50).IsRequired();
            entity.Property(md => md.NomTrabajador).HasColumnName("NomTrabajador").HasMaxLength(50).IsRequired();
            entity.Property(md => md.cargo).HasColumnName("Cargo");
            entity.Property(md => md.Proceso).HasColumnName("Proceso").HasMaxLength(50).IsRequired();
            entity.Property(md => md.Situacion).HasColumnName("Situacion").HasMaxLength(255).IsRequired();
            entity.Property(md => md.Firma).HasColumnName("Firma").HasColumnType("text").IsRequired();
            entity.Property(md => md.Owner).HasColumnName("Owner").HasMaxLength(30);
            entity.Property(md => md.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(md => md.LastModify).HasColumnName("LastModify").HasMaxLength(30);
            entity.Property(md => md.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
         });

         modelBuilder.Entity<Cargo>(entity =>
         {
            entity.ToTable("Cargo");
            entity.HasKey(c => c.IdCargo);
            entity.Property(c => c.IdCargo).HasColumnName("IdCargo");
            entity.Property(c => c.NomCargo).HasColumnName("NomCargo").IsRequired();
            entity.Property(c => c.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp without time zone");
            entity.Property(c => c.Owner).HasColumnName("Owner");
            entity.Property(c => c.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(c => c.LastModify).HasColumnName("LastModify");
            entity.Property(c => c.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
         });

         modelBuilder.Entity<Sucursal>(entity =>
         {
            entity.ToTable("Sucursal");
            entity.HasKey(s => s.IdSucursal);
            entity.Property(s => s.IdSucursal).HasColumnName("IdSucursal");
            entity.Property(s => s.NombreSuc).HasColumnName("NombreSuc").IsRequired();
            entity.Property(s => s.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp without time zone");
            entity.Property(s => s.Owner).HasColumnName("Owner");
            entity.Property(s => s.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(s => s.LastModify).HasColumnName("LastModify");
            entity.Property(s => s.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
         });

         modelBuilder.Entity<CitacionDescargos>(entity =>
         {
            entity.ToTable("CitacionDescargos");
            entity.HasKey(c => c.IdCitacion);
            entity.Property(c => c.IdCitacion).HasColumnName("IdCitacion");
            entity.Property(c => c.Ciudad).HasColumnName("Ciudad").HasMaxLength(50);
            entity.Property(c => c.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp without time zone");
            entity.Property(c => c.NomTrabajador).HasColumnName("NomTrabajador").HasMaxLength(20).IsRequired();
            entity.Property(c => c.IdCargo).HasColumnName("IdCargo");
            entity.Property(c => c.IdArea).HasColumnName("IdArea");
            entity.Property(c => c.Proceso).HasColumnName("Proceso").HasMaxLength(20).IsRequired();
            entity.Property(c => c.fecha_Diligencia).HasColumnName("fecha_Diligencia").HasColumnType("timestamp without time zone");
            entity.Property(c => c.HoraDiligencia).HasColumnName("HoraDiligencia").HasColumnType("timestamp without time zone");
            entity.Property(c => c.FechaHechos).HasColumnName("FechaHechos").HasColumnType("timestamp without time zone");
            entity.Property(c => c.CcRecibido).HasColumnName("CcRecibido").HasMaxLength(20).IsRequired();
            entity.Property(c => c.Owner).HasColumnName("Owner").HasMaxLength(30);
            entity.Property(c => c.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(c => c.LastModify).HasColumnName("LastModify").HasMaxLength(30);
            entity.Property(c => c.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
         });

         modelBuilder.Entity<ActaDescargos>(entity =>
         {
            entity.ToTable("ActaDescargos");
            entity.HasKey(a => a.IdActa);
            entity.Property(a => a.IdActa).HasColumnName("IdActa");
            entity.Property(a => a.Ciudad).HasColumnName("Ciudad").HasMaxLength(100);
            entity.Property(a => a.fecha_Creacion).HasColumnName("fecha_Creacion").HasColumnType("timestamp without time zone");
            entity.Property(a => a.FechaInformeDisciplinario).HasColumnName("FechaInformeDisciplinario").HasColumnType("date");
            entity.Property(a => a.NombreTrabajador).HasColumnName("NombreTrabajador").HasMaxLength(150).IsRequired();
            entity.Property(a => a.CcTrabajador).HasColumnName("CcTrabajador").HasMaxLength(50).IsRequired();
            entity.Property(a => a.DireccionResidencia).HasColumnName("DireccionResidencia").HasMaxLength(200);
            entity.Property(a => a.Telefono).HasColumnName("Telefono").HasMaxLength(50);
            entity.Property(a => a.IdCargo).HasColumnName("IdCargo");
            entity.Property(a => a.IdArea).HasColumnName("IdArea");
            entity.Property(a => a.Proceso).HasColumnName("Proceso").HasMaxLength(100).IsRequired();
            entity.Property(a => a.FechaIngreso).HasColumnName("FechaIngreso").HasColumnType("date");
            entity.Property(a => a.AsistidoTestigos).HasColumnName("AsistidoTestigos");
            entity.Property(a => a.NombreTestigo).HasColumnName("NombreTestigo").HasMaxLength(150);
            entity.Property(a => a.HoraInicio).HasColumnName("HoraInicio").HasMaxLength(20);
            entity.Property(a => a.HoraFin).HasColumnName("HoraFin").HasMaxLength(20);
            entity.Property(a => a.FechaDiligencia).HasColumnName("FechaDiligencia").HasColumnType("timestamp without time zone");
            entity.Property(a => a.ConceptoGestionHumana).HasColumnName("ConceptoGestionHumana").HasColumnType("text");
            entity.Property(a => a.DiasSancion).HasColumnName("DiasSancion");
            entity.Property(a => a.FechaInicioSancion).HasColumnName("FechaInicioSancion").HasColumnType("date");
            entity.Property(a => a.Owner).HasColumnName("Owner").HasMaxLength(30);
            entity.Property(a => a.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(a => a.LastModify).HasColumnName("LastModify").HasMaxLength(30);
            entity.Property(a => a.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
         });

         modelBuilder.Entity<ActaDescargosPreguntas>(entity =>
         {
            entity.ToTable("ActaDescargosPreguntas");
            entity.HasKey(p => p.IdPregunta);
            entity.Property(p => p.IdPregunta).HasColumnName("IdPregunta");
            entity.Property(p => p.IdActa).HasColumnName("IdActa");
            entity.Property(p => p.Orden).HasColumnName("Orden");
            entity.Property(p => p.Pregunta).HasColumnName("Pregunta").HasColumnType("text").IsRequired();
            entity.Property(p => p.Respuesta).HasColumnName("Respuesta").HasColumnType("text").IsRequired();
            entity.Property(p => p.Owner).HasColumnName("Owner").HasMaxLength(30);
            entity.Property(p => p.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(p => p.LastModify).HasColumnName("LastModify").HasMaxLength(30);
            entity.Property(p => p.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");

            entity.HasOne(p => p.Acta)
                  .WithMany(a => a.Preguntas)
                  .HasForeignKey(p => p.IdActa)
                  .OnDelete(DeleteBehavior.Cascade);
         });

         modelBuilder.Entity<Colaborador>(entity =>
         {
            entity.ToTable("Colaborador");
            entity.HasKey(cb=> cb.IdColaborador);
            entity.Property(cb=>cb.IdCargo);
            entity.Property(cb=>cb.Nombre);
            entity.Property(cb=>cb.Apellido);
            entity.Property(cb=>cb.CodigoEmp);
            entity.Property(cb=>cb.Cedula);
         });

         modelBuilder.Entity<ConsultaPeps>(entity =>
         {
            entity.ToTable("ConsultaPeps");
            entity.HasKey(c => c.IdConsulta);
            entity.Property(c => c.IdConsulta).HasColumnName("IdConsulta");
            entity.Property(c => c.IdUsuario).HasColumnName("IdUsuario");
            entity.Property(c => c.Cedula).HasColumnName("Cedula").HasMaxLength(50).IsRequired();
            entity.Property(c => c.IdCargo).HasColumnName("IdCargo");
            entity.Property(c => c.IdArea).HasColumnName("IdArea");
            entity.Property(c => c.EsPep).HasColumnName("EsPep");
            entity.Property(c => c.Firma).HasColumnName("Firma").HasColumnType("text").IsRequired(false);
            entity.Property(c => c.Owner).HasColumnName("Owner").HasMaxLength(30);
            entity.Property(c => c.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(c => c.LastModify).HasColumnName("LastModify").HasMaxLength(30);
            entity.Property(c => c.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");
         });

         modelBuilder.Entity<PasswordResetToken>(entity =>
         {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(t => t.IdToken);
            entity.Property(t => t.IdToken).HasColumnName("IdToken");
            entity.Property(t => t.UserId).HasColumnName("UserId");
            entity.Property(t => t.TokenHash).HasColumnName("TokenHash").HasMaxLength(128).IsRequired();
            entity.Property(t => t.ExpiresAt).HasColumnName("ExpiresAt").HasColumnType("timestamp without time zone");
            entity.Property(t => t.UsedAt).HasColumnName("UsedAt").HasColumnType("timestamp without time zone");
            entity.Property(t => t.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp without time zone");
            entity.HasIndex(t => t.TokenHash);
            entity.HasIndex(t => t.UserId);
         });

         modelBuilder.Entity<Permiso>(entity =>
         {
            entity.ToTable("Permisos");
            entity.HasKey(p => p.IdPermiso);
            entity.Property(p => p.IdPermiso).HasColumnName("IdPermiso");
            entity.Property(p => p.Clave).HasColumnName("Clave").HasMaxLength(100).IsRequired();
            entity.Property(p => p.Modulo).HasColumnName("Modulo").HasMaxLength(50).IsRequired();
            entity.Property(p => p.Descripcion).HasColumnName("Descripcion").HasMaxLength(200).IsRequired();
            entity.HasIndex(p => p.Clave).IsUnique();
         });

         modelBuilder.Entity<RolPermiso>(entity =>
         {
            entity.ToTable("RolPermisos");
            entity.HasKey(rp => new { rp.IdRol, rp.IdPermiso });
            entity.Property(rp => rp.IdRol).HasColumnName("IdRol");
            entity.Property(rp => rp.IdPermiso).HasColumnName("IdPermiso");

            entity.HasOne(rp => rp.Rol)
                  .WithMany()
                  .HasForeignKey(rp => rp.IdRol)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permiso)
                  .WithMany(p => p.RolPermisos)
                  .HasForeignKey(rp => rp.IdPermiso)
                  .OnDelete(DeleteBehavior.Cascade);
         });

         modelBuilder.Entity<PreguntasConflictoInteres>(entity =>
         {
            entity.ToTable("PreguntasConflictoInteres");
            entity.HasKey(p => p.IdPregunta);
            entity.Property(p => p.IdPregunta).HasColumnName("IdPregunta");
            entity.Property(p => p.Codigo).HasColumnName("Codigo").HasMaxLength(50).IsRequired();
            entity.Property(p => p.Descripcion).HasColumnName("Descripcion").HasColumnType("text").IsRequired();
            entity.Property(p => p.Activo).HasColumnName("Activo").HasDefaultValue(true);
            entity.HasIndex(p => p.Codigo).IsUnique();
         });

         modelBuilder.Entity<DeclaracionConflictoInteres>(entity =>
         {
            entity.ToTable("DeclaracionConflictoInteres");
            entity.HasKey(d => d.IdDeclaracion);
            entity.Property(d => d.IdDeclaracion).HasColumnName("IdDeclaracion");
            entity.Property(d => d.IdUsuario).HasColumnName("IdUsuario");
            entity.Property(d => d.TipoDoc).HasColumnName("TipoDoc").HasMaxLength(10).HasDefaultValue("CC");
            entity.Property(d => d.NumeroDocumento).HasColumnName("NumeroDocumento").HasMaxLength(50).IsRequired();
            entity.Property(d => d.IdCargo).HasColumnName("IdCargo");
            entity.Property(d => d.IdArea).HasColumnName("IdArea");
            entity.Property(d => d.FechaDeclaracion).HasColumnName("FechaDeclaracion").HasColumnType("date");
            entity.Property(d => d.NombreFamiliar).HasColumnName("NombreFamiliar").HasMaxLength(150);
            entity.Property(d => d.CargoFamiliar).HasColumnName("CargoFamiliar").HasMaxLength(150);
            entity.Property(d => d.AreaFamiliar).HasColumnName("AreaFamiliar").HasMaxLength(150);
            entity.Property(d => d.Observaciones).HasColumnName("Observaciones").HasColumnType("text");
            entity.Property(d => d.Firma).HasColumnName("Firma").HasColumnType("text").IsRequired(false);
            entity.Property(d => d.Owner).HasColumnName("Owner").HasMaxLength(30);
            entity.Property(d => d.DateOwner).HasColumnName("DateOwner").HasColumnType("timestamp without time zone");
            entity.Property(d => d.LastModify).HasColumnName("LastModify").HasMaxLength(30);
            entity.Property(d => d.LastDate).HasColumnName("LastDate").HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Usuario)
                  .WithMany()
                  .HasForeignKey(d => d.IdUsuario)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Cargo)
                  .WithMany()
                  .HasForeignKey(d => d.IdCargo)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Area)
                  .WithMany()
                  .HasForeignKey(d => d.IdArea)
                  .OnDelete(DeleteBehavior.SetNull);
         });

         modelBuilder.Entity<DeclaracionConflictoInteresPreguntas>(entity =>
         {
            entity.ToTable("DeclaracionConflictoInteresPreguntas");
            entity.HasKey(dp => dp.IdDeclaracionPregunta);
            entity.Property(dp => dp.IdDeclaracionPregunta).HasColumnName("IdDeclaracionPregunta");
            entity.Property(dp => dp.IdDeclaracion).HasColumnName("IdDeclaracion");
            entity.Property(dp => dp.IdPregunta).HasColumnName("IdPregunta");
            entity.Property(dp => dp.Aplica).HasColumnName("Aplica").HasDefaultValue(false);

            entity.HasOne(dp => dp.Declaracion)
                  .WithMany(d => d.Preguntas)
                  .HasForeignKey(dp => dp.IdDeclaracion)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dp => dp.Pregunta)
                  .WithMany(p => p.DeclaracionesPreguntas)
                  .HasForeignKey(dp => dp.IdPregunta)
                  .OnDelete(DeleteBehavior.Restrict);
         });

      }
   }
}