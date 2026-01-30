using System;

namespace PracticaLogin
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Añadido para mostrarla
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public string Suscripcion { get; set; }
        public int GradoBaneo { get; set; }
        public DateTime? FinBaneo { get; set; }

        // PROPIEDAD CALCULADA (Esto hace la magia del tiempo restante)
        public string TiempoRestante
        {
            get
            {
                if (Activo || FinBaneo == null) return "SIN SANCIÓN";

                if (FinBaneo.Value.Year > 3000) return "PERMANENTE"; // Si es año 9999

                TimeSpan restante = FinBaneo.Value - DateTime.Now;

                if (restante.TotalSeconds <= 0) return "EXPIRADO";

                if (restante.TotalDays >= 1)
                    return $"{(int)restante.TotalDays} días, {restante.Hours}h";
                else
                    return $"{restante.Hours}h {restante.Minutes}min";
            }
        }
    }
}