using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class ConfigWindow : Window
    {
        private string usuarioActual;

        // CONSTRUCTOR MODIFICADO: Ahora pide el nombre del usuario
        public ConfigWindow(string usuario)
        {
            InitializeComponent();

            this.usuarioActual = usuario;
            CargarDatosCuenta();
        }

        // Método para rellenar los datos de la pantalla Cuenta
        private void CargarDatosCuenta()
        {
            lblUsuarioCuenta.Text = usuarioActual.ToUpper();

            // Buscamos la ID en la base de datos
            string id = DatabaseHelper.GetUserId(usuarioActual);
            lblIdCuenta.Text = "ID: #" + id + "-AKAY";
        }

        // --- BOTONES DE LA PESTAÑA CUENTA ---

        // 1. Mostrar el hueco para escribir la contraseña
        private void BtnMostrarCambioPass_Click(object sender, RoutedEventArgs e)
        {
            if (pnlCambioPass.Visibility == Visibility.Collapsed)
                pnlCambioPass.Visibility = Visibility.Visible;
            else
                pnlCambioPass.Visibility = Visibility.Collapsed;
        }

        // 2. Guardar la contraseña en la BD
        private void BtnConfirmarPass_Click(object sender, RoutedEventArgs e)
        {
            string nuevaPass = txtNuevaPass.Password;

            if (string.IsNullOrEmpty(nuevaPass))
            {
                MessageBox.Show("La contraseña no puede estar vacía.");
                return;
            }

            bool exito = DatabaseHelper.UpdatePassword(usuarioActual, nuevaPass);

            if (exito)
            {
                MessageBox.Show("¡Contraseña actualizada con éxito!", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Information);
                pnlCambioPass.Visibility = Visibility.Collapsed;
                txtNuevaPass.Clear();
            }
            else
            {
                MessageBox.Show("Error al actualizar la contraseña.");
            }
        }

        // --- NAVEGACIÓN DEL MENÚ ---
        private void BtnGeneral_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlGeneral.Visibility = Visibility.Visible; }
        private void BtnGraficos_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlGraficos.Visibility = Visibility.Visible; }
        private void BtnSonido_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlSonido.Visibility = Visibility.Visible; }
        private void BtnCuenta_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlCuenta.Visibility = Visibility.Visible; }

        private void OcultarTodos()
        {
            pnlGeneral.Visibility = Visibility.Collapsed;
            pnlGraficos.Visibility = Visibility.Collapsed;
            pnlSonido.Visibility = Visibility.Collapsed;
            pnlCuenta.Visibility = Visibility.Collapsed;
        }

        // --- BOTONES GENÉRICOS ---
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Configuración guardada correctamente.");
            this.Close();
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}