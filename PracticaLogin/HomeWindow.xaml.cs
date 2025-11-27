using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class HomeWindow : Window
    {
        private string usuarioLogueado;

        // Constructor
        public HomeWindow(string nombreUsuario)
        {
            InitializeComponent();

            this.usuarioLogueado = nombreUsuario;

            if (lblNombreUsuario != null)
            {
                lblNombreUsuario.Text = nombreUsuario.ToUpper();
            }
        }

        // --- CONTROLES VENTANA ---
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // --- NAVEGACIÓN ---
        private void BtnTienda_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeccion("TIENDA AKAY");
        }

        // NUEVO: Abre la ventana de suscripciones
        private void BtnSuscripciones_Click(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0.4;

            SubscriptionsWindow subWindow = new SubscriptionsWindow();
            subWindow.ShowDialog();

            this.Opacity = 1;
        }

        private void BtnComunidad_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeccion("COMUNIDAD GLOBAL");
        }

        private void BtnSoporte_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeccion("SOPORTE TÉCNICO");
        }

        private void BtnDestacados_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeccion("DESTACADOS DE LA SEMANA");
        }

        private void BtnMisJuegos_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeccion("TU BIBLIOTECA DE JUEGOS");
        }

        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0.5;
            ConfigWindow config = new ConfigWindow(usuarioLogueado);
            config.ShowDialog();
            this.Opacity = 1;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void AbrirSeccion(string titulo)
        {
            this.Opacity = 0.4;
            SubVentana ventana = new SubVentana(titulo);
            ventana.ShowDialog();
            this.Opacity = 1;
        }

        // --- CLICS EN JUEGOS ---
        private void Juego1_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirDetalleJuego("PHANTOM", "/Assets/juego1.jpg", "Acción / Cyberpunk", "GRATIS", "1 Jugador", "En un futuro distópico, conviértete en un fantasma digital capaz de hackear la realidad. Phantom ofrece una experiencia inmersiva.");
        }

        private void Juego2_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirDetalleJuego("DRAGON'S SHADOW", "/Assets/juego2.jpg", "RPG / Fantasía", "49.99€", "MMO Online", "Únete a miles de jugadores en un mundo abierto masivo. Caza dragones, forja alianzas y conquista castillos.");
        }

        private void Juego3_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirDetalleJuego("VOID DIVER", "/Assets/juego3.jpg", "Terror", "19.99€", "Cooperativo", "Estás atrapado en una estación submarina abandonada. El oxígeno se agota y algo acecha en la oscuridad.");
        }

        private void Juego5_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirDetalleJuego("STAR JUMPER", "/Assets/juego7.jpg", "Plataformas", "29.99€", "1 Jugador", "¡El nuevo éxito del año! Salta entre planetas en este colorido juego de plataformas. Ideal para todas las edades.");
        }

        private void AbrirDetalleJuego(string t, string img, string gen, string prec, string jug, string desc)
        {
            this.Opacity = 0.4;
            GameDetailWindow detalle = new GameDetailWindow(t, img, gen, prec, jug, desc);
            detalle.ShowDialog();
            this.Opacity = 1;
        }
    }
}