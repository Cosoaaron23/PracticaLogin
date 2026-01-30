using System.Collections.Generic;
using System.Linq;

namespace PracticaLogin
{
    public static class CarritoService
    {
        public static List<Juego> Cesta = new List<Juego>();

        public static void Agregar(Juego juego)
        {
            if (!Cesta.Any(x => x.Id == juego.Id)) Cesta.Add(juego);
        }

        public static void Remover(Juego juego)
        {
            var item = Cesta.FirstOrDefault(x => x.Id == juego.Id);
            if (item != null) Cesta.Remove(item);
        }

        public static void Vaciar() => Cesta.Clear();
        public static decimal Total() => Cesta.Sum(x => x.Precio);
    }
}