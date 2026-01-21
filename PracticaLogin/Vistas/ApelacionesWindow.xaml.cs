using System.Windows;
using System.Windows.Controls; // Necesario para Button
using System.Windows.Media;    // Necesario para Brushes

namespace PracticaLogin
{
    public partial class ApelacionesWindow : Window
    {
        public ApelacionesWindow()
        {
            InitializeComponent();
            CargarMensajes();
        }

        private void CargarMensajes()
        {
            dgMensajes.ItemsSource = DatabaseHelper.ObtenerApelaciones();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        // --- NUEVA LÓGICA DE BORRADO ---
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Averiguar qué fila se pulsó
            Button botonPulsado = sender as Button;

            // "DataContext" contiene el objeto Apelacion de esa fila concreta
            if (botonPulsado.DataContext is Apelacion apelacionSeleccionada)
            {
                // 2. Confirmar (Opcional, pero recomendado)
                var confirm = new CustomMessageBox("CONFIRMAR", "¿Borrar este mensaje?", Brushes.Red);

                if (confirm.ShowDialog() == true)
                {
                    // 3. Borrar de la BD
                    DatabaseHelper.EliminarApelacion(apelacionSeleccionada.Id);

                    // 4. Refrescar la tabla para que desaparezca visualmente
                    CargarMensajes();
                }
            }
        }
    }
}