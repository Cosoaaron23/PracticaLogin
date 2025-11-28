using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class ConfigWindow : Window
    {
        private string usuarioActual;

        public ConfigWindow(string usuario)
        {
            InitializeComponent();
            this.usuarioActual = usuario;
            CargarDatosCuenta();
        }

        private void CargarDatosCuenta()
        {
            lblUsuarioCuenta.Text = usuarioActual.ToUpper();
            string id = DatabaseHelper.GetUserId(usuarioActual);
            lblIdCuenta.Text = "ID: #" + id + "-AKAY";
        }

        // --- GESTIÓN DE SUSCRIPCIÓN ---
        private void BtnGestionarSuscripcion_Click(object sender, RoutedEventArgs e)
        {
            string plan = DatabaseHelper.GetSubscription(usuarioActual);
            string titulo = "";
            string mensaje = "";
            SolidColorBrush color = Brushes.Gray;

            switch (plan)
            {
                case "Cielo":
                    titulo = "PLAN ACTUAL: EL CIELO";
                    color = new SolidColorBrush(Color.FromRgb(79, 195, 247));
                    mensaje = "✔ Acceso básico\n✔ Con anuncios\nEstado: GRATIS";
                    break;
                case "Limbo":
                    titulo = "PLAN ACTUAL: LIMBO";
                    color = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    mensaje = "✔ Sin Publicidad\n✔ Multijugador\nEstado: ACTIVO (4.99€/mes)";
                    break;
                case "Infierno":
                    titulo = "PLAN ACTUAL: EL INFIERNO";
                    color = new SolidColorBrush(Color.FromRgb(211, 47, 47));
                    mensaje = "★ ACCESO TOTAL\n★ Skins Demon\n★ Descargas Ultra\nEstado: ACTIVO (14.99€/mes)";
                    break;
                default:
                    titulo = "SIN SUSCRIPCIÓN";
                    color = Brushes.Gray;
                    mensaje = "No tienes suscripción activa.";
                    break;
            }

            CustomMessageBox info = new CustomMessageBox(titulo, mensaje, color);
            info.btnConfirmar.Content = "ENTENDIDO";
            info.ShowDialog();
        }

        // --- CAMBIO DE CONTRASEÑA ---
        private void BtnMostrarCambioPass_Click(object sender, RoutedEventArgs e)
        {
            if (pnlCambioPass.Visibility == Visibility.Collapsed) pnlCambioPass.Visibility = Visibility.Visible;
            else pnlCambioPass.Visibility = Visibility.Collapsed;
        }

        private void BtnConfirmarPass_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNuevaPass.Password)) { MessageBox.Show("Contraseña vacía"); return; }
            if (DatabaseHelper.UpdatePassword(usuarioActual, txtNuevaPass.Password))
            {
                MessageBox.Show("Contraseña actualizada.");
                pnlCambioPass.Visibility = Visibility.Collapsed; txtNuevaPass.Clear();
            }
            else MessageBox.Show("Error al actualizar.");
        }

        // --- NAVEGACIÓN Y CONTROLES ---
        private void BtnGeneral_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlGeneral.Visibility = Visibility.Visible; }
        private void BtnGraficos_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlGraficos.Visibility = Visibility.Visible; }
        private void BtnSonido_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlSonido.Visibility = Visibility.Visible; }
        private void BtnCuenta_Click(object sender, RoutedEventArgs e) { OcultarTodos(); pnlCuenta.Visibility = Visibility.Visible; }

        private void OcultarTodos()
        {
            pnlGeneral.Visibility = Visibility.Collapsed; pnlGraficos.Visibility = Visibility.Collapsed;
            pnlSonido.Visibility = Visibility.Collapsed; pnlCuenta.Visibility = Visibility.Collapsed;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Guardado."); this.Close(); }
        private void BtnVolver_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
    }
}