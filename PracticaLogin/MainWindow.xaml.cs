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
            // Inicializar la base de datos al arrancar para crear tablas si no existen
            DatabaseHelper.InitializeDatabase();
        }

        // --- BOTONES DE VENTANA (Cerrar / Minimizar) ---
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Permite arrastrar la ventana aunque no tenga bordes
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // --- NAVEGACIÓN ENTRE PANELES (Login <-> Registro) ---
        private void IrARegistro_Click(object sender, RoutedEventArgs e)
        {
            pnlLogin.Visibility = Visibility.Collapsed;
            pnlRegistro.Visibility = Visibility.Visible;
            lblMensajeLogin.Text = "";
            lblMensajeRegistro.Text = "";
        }

        private void IrALogin_Click(object sender, RoutedEventArgs e)
        {
            pnlRegistro.Visibility = Visibility.Collapsed;
            pnlLogin.Visibility = Visibility.Visible;
            lblMensajeLogin.Text = "";
            lblMensajeRegistro.Text = "";
        }

        // --- LÓGICA DE LOGIN ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuarioLogin.Text;
            string pass = txtPasswordLogin.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensajeLogin.Text = "Por favor, llena todos los campos.";
                return;
            }

            bool loginExitoso = DatabaseHelper.ValidateUser(user, pass);

            if (loginExitoso)
            {
                HomeWindow home = new HomeWindow(user);
                home.Show();
                this.Close();
            }
            else
            {
                // El mensaje de error específico (bloqueo, intentos) ya lo muestra el DatabaseHelper en un MessageBox,
                // pero aquí ponemos un texto rojo genérico por si acaso.
                lblMensajeLogin.Text = "Credenciales incorrectas.";
            }
        }

        // --- LÓGICA DE REGISTRO ---
        private void BtnCrearCuenta_Click(object sender, RoutedEventArgs e)
        {
            // Recoger datos
            string nom = txtRegNombre.Text;
            string ape = txtRegApellidos.Text;
            string user = txtRegUser.Text;
            string pass = txtRegPass.Password;
            string mail = txtRegEmail.Text;
            string tlf = txtRegTlf.Text;
            string cp = txtRegCP.Text;

            // Validación simple
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(mail))
            {
                lblMensajeRegistro.Text = "Usuario, Contraseña y Email son obligatorios.";
                return;
            }

            bool registroExitoso = DatabaseHelper.RegisterUser(nom, ape, user, pass, mail, tlf, cp);

            if (registroExitoso)
            {
                MessageBox.Show("¡Cuenta creada con éxito! Ahora inicia sesión.");
                IrALogin_Click(sender, e);
            }
            else
            {
                lblMensajeRegistro.Text = "Error al crear la cuenta. El usuario podría existir.";
            }
        }
    }
}