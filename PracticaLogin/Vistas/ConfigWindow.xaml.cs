using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class ConfigWindow : Window
    {
        // CAMBIO 1: Guardamos el objeto completo, no solo el string
        private Usuario _usuarioActual;

        // CAMBIO 2: El constructor recibe el objeto Usuario
        public ConfigWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            CargarDatosUsuario();
        }

        private void CargarDatosUsuario()
        {
            // Usamos los datos del objeto que ya tenemos en memoria
            lblUsuarioCuenta.Text = _usuarioActual.Username.ToUpper();
            lblIdCuenta.Text = $"ID: #{_usuarioActual.Id:D4}";
        }

        // --- BOTÓN VOLVER (Aquí estaba tu error) ---
        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Ahora pasamos el OBJETO _usuarioActual, no un string
            HomeWindow home = new HomeWindow(_usuarioActual);
            home.Show();
            this.Close();
        }

        // --- RESTO DE TU CÓDIGO (Sin cambios importantes, solo referencias) ---

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); }

        // Paneles
        private void OcultarTodosPaneles() { pnlGeneral.Visibility = Visibility.Collapsed; pnlGraficos.Visibility = Visibility.Collapsed; pnlSonido.Visibility = Visibility.Collapsed; pnlCuenta.Visibility = Visibility.Collapsed; }
        private void BtnGeneral_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlGeneral.Visibility = Visibility.Visible; }
        private void BtnGraficos_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlGraficos.Visibility = Visibility.Visible; }
        private void BtnSonido_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlSonido.Visibility = Visibility.Visible; }
        private void BtnCuenta_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlCuenta.Visibility = Visibility.Visible; }

        // Lógica Cuenta
        private void BtnMostrarCambioPass_Click(object sender, RoutedEventArgs e)
        {
            pnlCambioPass.Visibility = (pnlCambioPass.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnConfirmarPass_Click(object sender, RoutedEventArgs e)
        {
            string nuevaPass = txtNuevaPass.Password;
            if (!string.IsNullOrEmpty(nuevaPass))
            {
                DatabaseHelper.UpdatePassword(_usuarioActual.Id, nuevaPass); // Usamos ID del objeto
                txtNuevaPass.Password = "";
                pnlCambioPass.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("La contraseña no puede estar vacía.");
            }
        }

        private void BtnGestionarSuscripcion_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos el objeto usuario a la siguiente ventana
            SubscriptionsWindow subWindow = new SubscriptionsWindow(_usuarioActual);
            subWindow.Show();
            this.Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Configuración guardada.");
    }
}