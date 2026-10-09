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

        // Constructor vacío (opcional)
        public FloresDbContext()
        {
        }

        // Mapeo de las tablas (DbSet)
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Login> Logins { get; set; }

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
            });

            modelBuilder.Entity<Login>(entity =>
            {
               entity.HasKey(l=>l.IdLogin);
               entity.Property(l=>l.user_id);
               entity.Property(l=>l.password_hash);
               entity.Property(l=>l.ultimo_acceso);
               entity.Property(l=>l.activo); 
            });
        }
    }
}