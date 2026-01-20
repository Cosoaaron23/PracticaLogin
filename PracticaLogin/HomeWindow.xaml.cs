using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class HomeWindow : Window
    {
        // Guardamos el objeto usuario completo
        private Usuario _usuarioActual;

        // CONSTRUCTOR MODIFICADO: Recibe 'Usuario' en vez de 'string'
        public HomeWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            // 1. Mostrar nombre (Usamos tu variable lblNombreUsuario)
            if (lblNombreUsuario != null)
            {
                lblNombreUsuario.Text = _usuarioActual.Username.ToUpper();
            }

            // 2. LÓGICA DE ADMIN (Mostrar botón si es necesario)
            // IMPORTANTE: Asegúrate de añadir el botón en el XAML como explico abajo
            if (this.FindName("btnAdminPanel") is Button btnAdmin)
            {
                if (_usuarioActual.Rol == "ADMIN")
                    btnAdmin.Visibility = Visibility.Visible;
                else
                    btnAdmin.Visibility = Visibility.Collapsed;
            }
        }

        // --- NUEVO: EVENTO DEL BOTÓN ADMIN ---
        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow admin = new AdminWindow(_usuarioActual.Id);
            admin.ShowDialog();
        }

        // ==========================================
        // TUS MÉTODOS ORIGINALES (RESTORED)
        // ==========================================

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
            else this.WindowState = WindowState.Maximized;
        }

        private void UserProfile_Click(object sender, MouseButtonEventArgs e)
        {
            if (UserMenuPopup != null) UserMenuPopup.IsOpen = true;
        }

        // Estados
        private void BtnEstadoOnline_Click(object sender, RoutedEventArgs e) { CambiarEstado("#23A559"); }
        private void BtnEstadoAusente_Click(object sender, RoutedEventArgs e) { CambiarEstado("#F0B232"); }
        private void BtnEstadoInvisible_Click(object sender, RoutedEventArgs e) { CambiarEstado("#747F8D"); }

        private void CambiarEstado(string hexColor)
        {
            if (MainStatusIndicator != null && UserMenuPopup != null)
            {
                var converter = new BrushConverter();
                MainStatusIndicator.Background = (Brush)converter.ConvertFromString(hexColor);
                UserMenuPopup.IsOpen = false;
            }
        }

        // Navegación
        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            if (UserMenuPopup != null) UserMenuPopup.IsOpen = false;
            this.Opacity = 0.5;

            // CORRECCIÓN: Pasamos '_usuarioActual' (el objeto), NO '_usuarioActual.Username' (el string)
            ConfigWindow config = new ConfigWindow(_usuarioActual);

            config.ShowDialog();
            this.Opacity = 1;
        }

        private void BtnSuscripciones_Click(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0.5;

            // CORRECCIÓN: Pasamos '_usuarioActual' (el objeto)
            SubscriptionsWindow subWindow = new SubscriptionsWindow(_usuarioActual);

            subWindow.ShowDialog();
            this.Opacity = 1;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void BtnMiPerfil_Click(object sender, RoutedEventArgs e) => BtnConfiguracion_Click(sender, e);

        // Menú Principal
        private void BtnTienda_Click(object sender, RoutedEventArgs e) { AbrirSeccion("TIENDA AKAY"); }
        private void BtnComunidad_Click(object sender, RoutedEventArgs e) { AbrirSeccion("COMUNIDAD GLOBAL"); }
        private void BtnSoporte_Click(object sender, RoutedEventArgs e) { AbrirSeccion("SOPORTE TÉCNICO"); }
        private void BtnDestacados_Click(object sender, RoutedEventArgs e) { AbrirSeccion("DESTACADOS"); }
        private void BtnMisJuegos_Click(object sender, RoutedEventArgs e) { AbrirSeccion("BIBLIOTECA"); }

        private void AbrirSeccion(string titulo)
        {
            this.Opacity = 0.5;
            SubVentana ventana = new SubVentana(titulo);
            ventana.ShowDialog();
            this.Opacity = 1;
        }

        // Juegos
        private void Juego1_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("PHANTOM", "/Assets/juego1.jpg", "Acción", "GRATIS", "1P", "Hackea la realidad."); }
        private void Juego2_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("DRAGON", "/Assets/juego2.jpg", "RPG", "49€", "MMO", "Caza dragones."); }
        private void Juego3_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("VOID", "/Assets/juego3.jpg", "Terror", "19€", "Coop", "Terror espacial."); }
        private void Juego4_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("SILENT ECHO", "/Assets/juego4.jpg", "Puzzle", "14€", "1P", "Misterio."); }
        private void Juego5_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("STAR JUMPER", "/Assets/juego7.jpg", "Plataformas", "29€", "1P", "Aventura."); }
        private void Juego6_Click(object sender, MouseButtonEventArgs e) { AbrirDetalleJuego("IRON EMPEROR", "/Assets/juego6.jpg", "Estrategia", "39€", "Online", "Guerra total."); }

        private void AbrirDetalleJuego(string t, string img, string gen, string prec, string jug, string desc)
        {
            this.Opacity = 0.5;
            GameDetailWindow detalle = new GameDetailWindow(t, img, gen, prec, jug, desc);
            detalle.ShowDialog();
            this.Opacity = 1;
        }

        // Scroll
        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Solo intentamos mover el fondo si existe en el XAML (para evitar errores si usas tu diseño antiguo)
            if (e.VerticalChange != 0 && this.FindName("BackgroundTranslateTransform") is TranslateTransform trans)
            {
                trans.Y = -(e.VerticalOffset * 0.1);
            }
        }
    }
}