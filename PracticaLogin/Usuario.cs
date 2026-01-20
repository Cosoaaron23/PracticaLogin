public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } // <--- NUEVO CAMPO
    public string Rol { get; set; }
    public bool Activo { get; set; }

    public string EstadoLegible => Activo ? "Activo" : "Baneado";
}