using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PracticaLogin
{
    public partial class HomeWindow : Window
    {
        private string usuarioLogueado;

        public HomeWindow(string nombreUsuario)
        {
            InitializeComponent();
            this.usuarioLogueado = nombreUsuario;

            if (lblNombreUsuario != null)
            {
                lblNombreUsuario.Text = nombreUsuario.ToUpper();
            }
        }

        // ==========================================
        // 1. CONTROLES DE VENTANA Y POPUP
        // ==========================================

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
            else this.WindowState = WindowState.Maximized;
        }
        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }

        private void UserProfile_Click(object sender, MouseButtonEventArgs e)
        {
            UserMenuPopup.IsOpen = true;
        }

        private void CambiarEstado(string hexColor)
        {
            var converter = new BrushConverter();
            MainStatusIndicator.Background = (Brush)converter.ConvertFromString(hexColor);
            UserMenuPopup.IsOpen = false;
        }

        private void BtnEstadoOnline_Click(object sender, RoutedEventArgs e) { CambiarEstado("#23A559"); }
        private void BtnEstadoAusente_Click(object sender, RoutedEventArgs e) { CambiarEstado("#F0B232"); }
        private void BtnEstadoInvisible_Click(object sender, RoutedEventArgs e) { CambiarEstado("#747F8D"); }

        private void BtnMiPerfil_Click(object sender, RoutedEventArgs e) { BtnConfiguracion_Click(sender, e); }

        // ==========================================
        // 2. LÓGICA PARALLAX (CORREGIDA)
        // ==========================================

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Solo necesitamos la transformación, el InvalidateVisual fue removido.
            if (e.VerticalChange != 0)
            {
                double parallaxFactor = 0.1;
                double newOffsetY = e.VerticalOffset * parallaxFactor;

                BackgroundTranslateTransform.Y = -newOffsetY;
            }
        }

        // El método OnRender que causaba el lag fue eliminado aquí.

        // ==========================================
        // 3. NAVEGACIÓN Y CLICS
        // ==========================================

        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
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

        private void BtnTienda_Click(object sender, RoutedEventArgs e) { AbrirSeccion("TIENDA AKAY"); }
        private void BtnSuscripciones_Click(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0.4;
            SubscriptionsWindow subWindow = new SubscriptionsWindow(usuarioLogueado);
            subWindow.ShowDialog();
            this.Opacity = 1;
        }
        private void BtnComunidad_Click(object sender, RoutedEventArgs e) { AbrirSeccion("COMUNIDAD GLOBAL"); }
        private void BtnSoporte_Click(object sender, RoutedEventArgs e) { AbrirSeccion("SOPORTE TÉCNICO"); }
        private void BtnDestacados_Click(object sender, RoutedEventArgs e) { AbrirSeccion("DESTACADOS"); }
        private void BtnMisJuegos_Click(object sender, RoutedEventArgs e) { AbrirSeccion("BIBLIOTECA"); }

        private void AbrirSeccion(string titulo)
        {
            this.Opacity = 0.4;
            SubVentana ventana = new SubVentana(titulo);
            ventana.ShowDialog();
            this.Opacity = 1;
        }

        // --- CLICS EN JUEGOS ---
        private void Juego1_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("PHANTOM", "/Assets/juego1.jpg", "Acción", "GRATIS", "1P", "En un futuro distópico, conviértete en un fantasma digital capaz de hackear la realidad."); }
        private void Juego2_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("DRAGON", "/Assets/juego2.jpg", "RPG", "49€", "MMO", "Únete a miles de jugadores en un mundo abierto masivo. Caza dragones, forja alianzas y conquista castillos."); }
        private void Juego3_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("VOID", "/Assets/juego3.jpg", "Terror", "19€", "Coop", "Estás atrapado en una estación submarina abandonada. El oxígeno se agota y algo acecha en la oscuridad."); }
        private void Juego4_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("SILENT ECHO", "/Assets/juego4.jpg", "Puzzle", "14€", "1P", "Un eco del pasado te persigue. Resuelve intrincados rompecabezas en este thriller."); }
        private void Juego5_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("STAR JUMPER", "/Assets/juego7.jpg", "Plataformas", "29€", "1P", "¡El nuevo éxito del año! Salta entre planetas en este colorido juego de plataformas. Ideal para todas las edades."); }
        private void Juego6_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("IRON EMPEROR", "/Assets/juego6.jpg", "Estrategia", "39€", "Online", "Comanda ejércitos masivos y domina el continente en tiempo real."); }

        private void AbrirDetalleJuego(string t, string img, string gen, string prec, string jug, string desc)
        {
            this.Opacity = 0.4;
            GameDetailWindow detalle = new GameDetailWindow(t, img, gen, prec, jug, desc);
            detalle.ShowDialog();
            this.Opacity = 1;
        }
    }
}