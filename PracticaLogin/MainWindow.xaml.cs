using System.Windows;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Esto crea el archivo de base de datos la primera vez que ejecutas
            DatabaseHelper.InitializeDatabase();
        }

        // --- BOTÓN INICIAR SESIÓN ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text;
            string pass = txtPassword.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MostrarMensaje("Completa todos los campos", Brushes.Red);
                return;
            }

            // Consultar a la base de datos
            bool esValido = DatabaseHelper.ValidateUser(user, pass);

            if (esValido)
            {
                MostrarMensaje("¡Bienvenido!", Brushes.Green);

                // Ir al Home
                HomeWindow home = new HomeWindow();
                home.Show();
                this.Close();
            }
            else
            {
                MostrarMensaje("Usuario no encontrado o clave errónea", Brushes.Red);
            }
        }

        // --- BOTÓN REGISTRARSE ---
        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text;
            string pass = txtPassword.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MostrarMensaje("Ingresa un usuario y clave para registrarte", Brushes.Orange);
                return;
            }

            // Intentar registrar en base de datos
            bool registroExitoso = DatabaseHelper.RegisterUser(user, pass);

            if (registroExitoso)
            {
                MostrarMensaje("¡Usuario creado! Ahora puedes entrar.", Brushes.Blue);
                // Opcional: Limpiar campos
                txtUsuario.Clear();
                txtPassword.Clear();
            }
            else
            {
                MostrarMensaje("El usuario ya existe. Prueba otro.", Brushes.Red);
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Función auxiliar para no repetir código de mensajes
        private void MostrarMensaje(string texto, Brush color)
        {
            lblMensaje.Text = texto;
            lblMensaje.Foreground = color;
        }
    }
}