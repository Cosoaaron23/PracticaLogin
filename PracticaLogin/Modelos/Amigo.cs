namespace PracticaLogin
{
    public class Amigo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Estado { get; set; } // 'Online', 'Ausente', etc.
        public string ColorEstado { get; set; } // Para pintar el circulito (Verde, Amarillo...)

        // Propiedad auxiliar para saber qué color poner según el estado
        public void ActualizarColor()
        {
            switch (Estado)
            {
                case "Online": ColorEstado = "#23A559"; break;   // Verde
                case "Ausente": ColorEstado = "#FAA61A"; break;  // Naranja
                case "Invisible": ColorEstado = "#747F8D"; break;// Gris
                default: ColorEstado = "#747F8D"; break;         // Offline
            }
        }
    }
}