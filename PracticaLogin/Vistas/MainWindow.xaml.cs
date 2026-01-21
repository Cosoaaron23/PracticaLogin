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
            DatabaseHelper.InitializeDatabase(); // Siempre inicializamos la BD al arrancar
        }

        // --- VENTANA SIN BORDES (Drag & Control) ---
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // --- NAVEGACIÓN ENTRE PANELES ---
        private void IrARegistro_Click(object sender, RoutedEventArgs e)
        {
            pnlLogin.Visibility = Visibility.Collapsed;
            pnlRegistro.Visibility = Visibility.Visible;
            // Limpiamos mensajes al cambiar
            lblMensajeLogin.Text = "";
            lblMensajeRegistro.Text = "";
            btnApelar.Visibility = Visibility.Collapsed;
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

            // 1. Resetear interfaz (esconder botón apelar)
            btnApelar.Visibility = Visibility.Collapsed;
            lblMensajeLogin.Text = "";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensajeLogin.Text = "Por favor, llena todos los campos.";
                return;
            }

            // 2. Validar Usuario
            string respuestaError;
            // Usamos el Helper inteligente que detecta si el baneo caducó
            Usuario usuarioLogueado = DatabaseHelper.ValidateUser(user, pass, out respuestaError);

            if (usuarioLogueado != null)
            {
                // ÉXITO -> Abrimos Home
                HomeWindow home = new HomeWindow(usuarioLogueado);
                home.Show();
                this.Close();
            }
            else
            {
                // FALLO -> Mostramos el error
                lblMensajeLogin.Text = respuestaError.ToUpper();

                // 3. DETECTAR SI ES BANEO PARA MOSTRAR BOTÓN APELAR
                // Buscamos palabras clave que pusimos en el Helper ("BANEADO", "SUSPENDIDA", "BLOQUEADO")
                if (respuestaError.Contains("BANEADO") || respuestaError.Contains("SUSPENDIDA") || respuestaError.Contains("BLOQUEADO"))
                {
                    // Mostramos la alerta visual bonita
                    var alerta = new CustomMessageBox("ACCESO DENEGADO", respuestaError, Brushes.Red);
                    alerta.ShowDialog();

                    // ¡AQUÍ APARECE EL BOTÓN!
                    btnApelar.Visibility = Visibility.Visible;
                }
                else
                {
                    // Si es contraseña incorrecta, solo borramos el campo pass
                    txtPasswordLogin.Password = "";
                }
            }
        }

        // --- BOTÓN DE APELAR ---
        private void BtnApelar_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos el nombre de usuario escrito para que no tenga que volver a ponerlo
            string usuarioBaneado = txtUsuarioLogin.Text;

            EnviarApelacionWindow ventana = new EnviarApelacionWindow(usuarioBaneado);
            ventana.ShowDialog();
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
                lblMensajeRegistro.Text = "Faltan campos obligatorios.";
                return;
            }

            bool exito = DatabaseHelper.RegisterUser(nom, ape, user, pass, mail, tlf, cp);

            if (exito)
            {
                var msg = new CustomMessageBox("BIENVENIDO", "¡Cuenta creada! Inicia sesión.", Brushes.Cyan);
                msg.ShowDialog();
                IrALogin_Click(sender, e);
            }
            else
            {
                lblMensajeRegistro.Text = "El usuario ya existe.";
            }
        }
    }
}