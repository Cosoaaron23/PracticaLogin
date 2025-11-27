using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error BD: " + ex.Message);
            }
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // === BOTÓN LOGIN ===
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuarioLogin.Text;
            string pass = txtPasswordLogin.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensajeLogin.Text = "Faltan datos";
                lblMensajeLogin.Foreground = Brushes.Red;
                return;
            }

            string resultado = DatabaseHelper.ValidateUser(user, pass);

            if (resultado == "OK")
            {
                // AQUI ESTA EL CAMBIO: Enviamos el usuario (user) al Home
                HomeWindow home = new HomeWindow(user);
                home.Show();
                this.Close();
            }
            else if (resultado == "NO_USER")
            {
                lblMensajeLogin.Text = "Usuario no existe";
                lblMensajeLogin.Foreground = Brushes.Red;
            }
            else if (resultado.StartsWith("WRONG_PASS"))
            {
                string[] partes = resultado.Split('|');
                lblMensajeLogin.Text = $"Contraseña mal. Te quedan {partes[1]} intentos.";
                lblMensajeLogin.Foreground = Brushes.Orange;
            }
            else if (resultado.StartsWith("LOCKED"))
            {
                string[] partes = resultado.Split('|');
                lblMensajeLogin.Text = $"BLOQUEADO. Espera {partes[1]} minutos.";
                lblMensajeLogin.Foreground = Brushes.Red;
            }
            else
            {
                lblMensajeLogin.Text = "Error: " + resultado;
            }
        }

        // === BOTÓN REGISTRO ===
        private void BtnCrearCuenta_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtRegNombre.Text;
            string user = txtRegUser.Text;
            string pass = txtRegPass.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(nombre))
            {
                lblMensajeRegistro.Text = "Datos obligatorios faltantes";
                return;
            }

            bool ok = DatabaseHelper.RegisterUser(nombre, txtRegApellidos.Text, user, pass, txtRegEmail.Text, txtRegTlf.Text, txtRegCP.Text);

            if (ok)
            {
                IrALogin_Click(null, null);
                lblMensajeLogin.Text = "¡Registrado! Entra ahora.";
                lblMensajeLogin.Foreground = Brushes.Green;
                txtRegNombre.Clear(); txtRegUser.Clear(); txtRegPass.Clear();
            }
            else
            {
                lblMensajeRegistro.Text = "Usuario ya existe";
                lblMensajeRegistro.Foreground = Brushes.Red;
            }
        }

        private void IrARegistro_Click(object sender, RoutedEventArgs e)
        {
            pnlLogin.Visibility = Visibility.Collapsed;
            pnlRegistro.Visibility = Visibility.Visible;
            lblMensajeLogin.Text = "";
        }

        private void IrALogin_Click(object sender, RoutedEventArgs e)
        {
            pnlRegistro.Visibility = Visibility.Collapsed;
            pnlLogin.Visibility = Visibility.Visible;
            lblMensajeRegistro.Text = "";
        }
    }
}