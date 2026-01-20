using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase(); // Inicializamos la BD y tablas
        }

        // --- BOTONES DE VENTANA ---
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // --- NAVEGACIÓN (Login <-> Registro) ---
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

        // --- LÓGICA DE LOGIN (AQUÍ ESTÁ EL CAMBIO IMPORTANTE) ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuarioLogin.Text;
            string pass = txtPasswordLogin.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensajeLogin.Text = "Por favor, llena todos los campos.";
                return;
            }

            // Llamamos al método que devuelve el OBJETO USUARIO (no bool)
            Usuario usuarioLogueado = DatabaseHelper.ValidateUser(user, pass);

            if (usuarioLogueado != null)
            {
                // Pasamos el objeto usuario entero al Home
                HomeWindow home = new HomeWindow(usuarioLogueado);
                home.Show();
                this.Close();
            }
            else
            {
                // El error específico ya saltó en MessageBox desde DatabaseHelper
                lblMensajeLogin.Text = "Credenciales incorrectas.";
            }
        }

        // --- LÓGICA DE REGISTRO (Tu código original) ---
        private void BtnCrearCuenta_Click(object sender, RoutedEventArgs e)
        {
            string nom = txtRegNombre.Text;
            string ape = txtRegApellidos.Text;
            string user = txtRegUser.Text;
            string pass = txtRegPass.Password;
            string mail = txtRegEmail.Text;
            string tlf = txtRegTlf.Text;
            string cp = txtRegCP.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(mail))
            {
                lblMensajeRegistro.Text = "Campos obligatorios vacíos.";
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
                lblMensajeRegistro.Text = "Error: El usuario ya existe.";
            }
        }
    }
}