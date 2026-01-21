using System;
using System.Windows;
using System.Windows.Media.Imaging; // Necesario para las imágenes

namespace PracticaLogin
{
    public partial class GameDetailWindow : Window
    {
        // Constructor modificado para recibir TODOS los datos
        public GameDetailWindow(string titulo, string imagePath, string genero, string precio, string jugadores, string descripcion)
        {
            InitializeComponent();

            // Asignar los textos
            lblTitulo.Text = titulo;
            lblGenero.Text = genero;
            lblPrecio.Text = precio;
            lblJugadores.Text = jugadores;
            txtDescripcion.Text = descripcion;

            // Asignar la imagen dinámicamente
            try
            {
                // Convierte la ruta de texto en una imagen real
                imgPortada.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                // Si falla la imagen, no hace nada (se queda negro)
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}