using System;

namespace PracticaLogin
{
    public class Apelacion
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Username { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }

        // Para mostrar bonito en la lista
        public string FechaLegible => Fecha.ToString("dd/MM/yyyy HH:mm");
    }
}