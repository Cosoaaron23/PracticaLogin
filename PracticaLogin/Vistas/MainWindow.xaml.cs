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
            LimpiarMensajes();
        }

        private void IrALogin_Click(object sender, RoutedEventArgs e)
        {
            pnlRegistro.Visibility = Visibility.Collapsed;
            pnlLogin.Visibility = Visibility.Visible;
            LimpiarMensajes();
        }

        private void LimpiarMensajes()
        {
            lblMensajeLogin.Text = "";
            lblMensajeRegistro.Text = "";
            if (btnApelar != null) btnApelar.Visibility = Visibility.Collapsed;
        }

        // --- LÓGICA DE LOGIN ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuarioLogin.Text;
            string pass = txtPasswordLogin.Password;

            // 1. Resetear interfaz
            if (btnApelar != null) btnApelar.Visibility = Visibility.Collapsed;
            lblMensajeLogin.Text = "";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensajeLogin.Text = "Por favor, llena todos los campos.";
                return;
            }

            // 2. Validar Usuario
            string respuestaError;
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
                // FALLO -> Analizamos el error

                // CASO 1: USUARIO BANEADO
                if (respuestaError == "Bloqueado")
                {
                    lblMensajeLogin.Text = "CUENTA SUSPENDIDA";

                    // Mostramos la alerta bonita preguntando si quiere apelar
                    // true = muestra botón cancelar (es una pregunta)
                    var alerta = new CustomMessageBox("ACCESO DENEGADO",
                        "Tu cuenta ha sido bloqueada por infringir las normas.\n\n¿Deseas enviar una apelación?",
                        Brushes.Red, true);

                    // Si le da a "CONFIRMAR" (SÍ), abrimos la ventana de apelación
                    if (alerta.ShowDialog() == true)
                    {
                        EnviarApelacionWindow ventana = new EnviarApelacionWindow(user);
                        ventana.ShowDialog();
                    }
                    else
                    {
                        // Si dice que NO, dejamos el botón visible por si se arrepiente
                        if (btnApelar != null) btnApelar.Visibility = Visibility.Visible;
                    }
                }
                // CASO 2: CONTRASEÑA INCORRECTA U OTRO ERROR
                else
                {
                    lblMensajeLogin.Text = "Usuario o contraseña incorrectos.";

                    // Alerta informativa (false = solo botón aceptar)
                    var alerta = new CustomMessageBox("ERROR DE LOGIN", "Las credenciales no coinciden.", Brushes.Orange, false);
                    alerta.ShowDialog();

                    txtPasswordLogin.Password = ""; // Limpiar pass
                }
            }
        }

        // --- BOTÓN DE APELAR (Por si cancelaron el popup y quieren darle después) ---
        private void BtnApelar_Click(object sender, RoutedEventArgs e)
        {
            string usuarioBaneado = txtUsuarioLogin.Text;
            if (!string.IsNullOrEmpty(usuarioBaneado))
            {
                EnviarApelacionWindow ventana = new EnviarApelacionWindow(usuarioBaneado);
                ventana.ShowDialog();
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
                lblMensajeRegistro.Text = "Faltan campos obligatorios.";
                return;
            }

            bool exito = DatabaseHelper.RegisterUser(nom, ape, user, pass, mail, tlf, cp);

            if (exito)
            {
                // Mensaje de éxito (false = solo botón aceptar)
                var msg = new CustomMessageBox("BIENVENIDO", "¡Cuenta creada correctamente! Ahora puedes iniciar sesión.", Brushes.Cyan, false);
                msg.ShowDialog();

                IrALogin_Click(sender, e);
            }
            else
            {
                lblMensajeRegistro.Text = "El nombre de usuario ya existe.";
            }
        }
    }
}