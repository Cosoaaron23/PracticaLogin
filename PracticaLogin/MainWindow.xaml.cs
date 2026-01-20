using System.Windows;
using System.Windows.Input;
using System.Windows.Media; // Necesario para Brushes

namespace PracticaLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase(); // Inicializamos la BD
        }

        // --- BOTONES DE VENTANA ---
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // --- NAVEGACIÓN ---
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

        // --- LÓGICA DE LOGIN (MODIFICADA) ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuarioLogin.Text;
            string pass = txtPasswordLogin.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensajeLogin.Text = "Por favor, llena todos los campos.";
                return;
            }

            // Variable para recibir el mensaje detallado del Helper
            string respuestaError;

            // Llamamos al método que ahora nos da el usuario Y el mensaje de error si falla
            Usuario usuarioLogueado = DatabaseHelper.ValidateUser(user, pass, out respuestaError);

            if (usuarioLogueado != null)
            {
                // LOGIN ÉXITO
                HomeWindow home = new HomeWindow(usuarioLogueado);
                home.Show();
                this.Close();
            }
            else
            {
                // LOGIN FALLIDO
                // Mostramos el error en el texto rojo (en mayúsculas para que resalte)
                lblMensajeLogin.Text = respuestaError.ToUpper();

                // Si el error es BANEO, sacamos la ventana personalizada ROJA
                if (respuestaError.Contains("BLOQUEADO") || respuestaError.Contains("BANEADA"))
                {
                    var alerta = new CustomMessageBox(
                        "ACCESO DENEGADO",
                        "Tu cuenta ha sido suspendida permanentemente por decisión de un administrador.",
                        Brushes.Red
                    );
                    alerta.ShowDialog();
                }
                else
                {
                    // Si solo es contraseña incorrecta, limpiamos el campo
                    txtPasswordLogin.Password = "";
                }
            }
        }

        // --- LÓGICA DE REGISTRO ---
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
                // Usamos la ventana personalizada para el éxito también
                var exito = new CustomMessageBox("BIENVENIDO", "¡Cuenta creada con éxito! Ahora inicia sesión.", Brushes.Cyan);
                exito.ShowDialog();
                IrALogin_Click(sender, e);
            }
            else
            {
                lblMensajeRegistro.Text = "Error: El usuario ya existe.";
            }
        }
    }
}