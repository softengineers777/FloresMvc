namespace FloresMvc.Models
{
    public class RolPermiso
    {
        public int IdRol { get; set; }
        public int IdPermiso { get; set; }

        public Roles? Rol { get; set; }
        public Permiso? Permiso { get; set; }
    }
}
