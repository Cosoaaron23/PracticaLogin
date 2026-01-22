using System; // Necesario para DateTime

namespace PracticaLogin
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }

        public string Suscripcion { get; set; }

        // --- NUEVOS CAMPOS ---
        public int GradoBaneo { get; set; }     // 1-5
        public DateTime? FinBaneo { get; set; } // Fecha fin

        // PROPIEDAD COMPUTADA: Estado (Activo / Baneado)
        public string EstadoLegible => Activo ? "Activo" : "Baneado";

        // PROPIEDAD COMPUTADA: Tiempo Restante (Para la tabla)
        public string TiempoRestante
        {
            get
            {
                if (Activo) return "-"; // Si está activo, no hay tiempo
                if (GradoBaneo == 5) return "∞ PERMANENTE";

                if (FinBaneo.HasValue)
                {
                    TimeSpan restante = FinBaneo.Value - DateTime.Now;
                    if (restante.TotalSeconds <= 0) return "Expirado (Esperando login)";

                    // Formato bonito: "3d 4h" o "5h 30m"
                    if (restante.TotalDays >= 1)
                        return $"{restante.Days}d {restante.Hours}h";
                    else
                        return $"{restante.Hours}h {restante.Minutes}m";
                }
                return "Desconocido";
            }
        }
    }
}