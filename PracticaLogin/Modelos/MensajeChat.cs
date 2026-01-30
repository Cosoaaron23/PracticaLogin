using System;

namespace PracticaLogin
{
    public class MensajeChat
    {
        public int Id { get; set; }
        public int IdRemitente { get; set; }
        public string Texto { get; set; }
        public DateTime Fecha { get; set; }

        // Propiedades visuales para el XAML
        public string Alineacion { get; set; } // "Left" (amigo) o "Right" (yo)
        public string ColorFondo { get; set; } // Color de la burbuja
        public string ColorTexto { get; set; }
    }
}