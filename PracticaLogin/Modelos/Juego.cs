using System;
using System.IO;

namespace PracticaLogin
{
    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Genero { get; set; }
        public decimal Precio { get; set; }
        public double TamanoGb { get; set; }
        public bool EsOnline { get; set; }
        public int NumJugadores { get; set; }

        // 1. IMAGEN VERTICAL (PORTADA)
        public string ImagenUrl { get; set; }

        // 2. IMAGEN HORIZONTAL (BANNER) - ¡NUEVO!
        public string ImagenFondoUrl { get; set; }

        public string TipoConexion => EsOnline ? "Online" : "Local";

        // Lógica Portada Vertical
        public string ImagenAbsoluta => ProcesarRuta(ImagenUrl);

        // Lógica Banner Horizontal
        public string ImagenFondoAbsoluta
        {
            get
            {
                // Si no hay fondo específico, usamos la portada para que no salga negro
                string ruta = string.IsNullOrEmpty(ImagenFondoUrl) ? ImagenUrl : ImagenFondoUrl;
                return ProcesarRuta(ruta);
            }
        }

        // Método auxiliar para evitar repetir código
        private string ProcesarRuta(string ruta)
        {
            if (string.IsNullOrEmpty(ruta)) return "/Assets/logo.png";
            if (ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return ruta;

            try
            {
                string rutaLimpia = ruta.TrimStart('/', '\\');
                string rutaFisica = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rutaLimpia);
                if (File.Exists(rutaFisica)) return rutaFisica;
            }
            catch { }

            return "/Assets/logo.png";
        }
    }
}