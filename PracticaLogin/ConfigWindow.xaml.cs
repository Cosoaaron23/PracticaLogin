using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class ConfigWindow : Window
    {
        private string currentUsername;
        private int currentUserId;

        public ConfigWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;

            // Cargar datos del usuario al iniciar
            CargarDatosUsuario();
        }

        // --- LOGICA DE DATOS ---
        private void CargarDatosUsuario()
        {
            // 1. Obtener ID
            currentUserId = DatabaseHelper.GetUserId(currentUsername);

            // 2. Mostrar datos en la interfaz (lblUsuarioCuenta y lblIdCuenta existen en tu XAML)
            lblUsuarioCuenta.Text = currentUsername.ToUpper();
            lblIdCuenta.Text = $"ID: #{currentUserId:D4}"; // Formato 0001
        }

        // --- ARRASTRAR VENTANA ---
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // --- MENÚ LATERAL (Ocultar/Mostrar Paneles) ---
        private void OcultarTodosPaneles()
        {
            pnlGeneral.Visibility = Visibility.Collapsed;
            pnlGraficos.Visibility = Visibility.Collapsed;
            pnlSonido.Visibility = Visibility.Collapsed;
            pnlCuenta.Visibility = Visibility.Collapsed;
        }

        private void BtnGeneral_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodosPaneles();
            pnlGeneral.Visibility = Visibility.Visible;
        }

        private void BtnGraficos_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodosPaneles();
            pnlGraficos.Visibility = Visibility.Visible;
        }

        private void BtnSonido_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodosPaneles();
            pnlSonido.Visibility = Visibility.Visible;
        }

        private void BtnCuenta_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodosPaneles();
            pnlCuenta.Visibility = Visibility.Visible;
        }

        // --- LOGICA PESTAÑA CUENTA ---

        // Botón "CAMBIAR CONTRASEÑA" (Muestra el panel pequeño)
        private void BtnMostrarCambioPass_Click(object sender, RoutedEventArgs e)
        {
            if (pnlCambioPass.Visibility == Visibility.Collapsed)
                pnlCambioPass.Visibility = Visibility.Visible;
            else
                pnlCambioPass.Visibility = Visibility.Collapsed;
        }

        // Botón "CONFIRMAR CAMBIO"
        private void BtnConfirmarPass_Click(object sender, RoutedEventArgs e)
        {
            string nuevaPass = txtNuevaPass.Password;

            if (!string.IsNullOrEmpty(nuevaPass))
            {
                DatabaseHelper.UpdatePassword(currentUserId, nuevaPass);
                txtNuevaPass.Password = "";
                pnlCambioPass.Visibility = Visibility.Collapsed; // Ocultar panel al terminar
            }
            else
            {
                MessageBox.Show("La contraseña no puede estar vacía.");
            }
        }

        // Botón "GESTIONAR SUSCRIPCIÓN" -> Abre la otra ventana
        private void BtnGestionarSuscripcion_Click(object sender, RoutedEventArgs e)
        {
            SubscriptionsWindow subWindow = new SubscriptionsWindow(currentUsername);
            subWindow.Show();
            this.Close();
        }

        // Botón "GUARDAR CAMBIOS" (Simulado)
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Configuración guardada correctamente.");
        }

        // Botón "VOLVER" (Cerrar sesión y volver al Login)
        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            HomeWindow home = new HomeWindow(currentUsername);
            home.Show();
            this.Close();
        }
    }
}