using System;
using System.Collections.Generic;
using System.Linq; // Necesario para filtros
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PracticaLogin
{
    public partial class HomeWindow : Window
    {
        private Usuario _usuarioActual;

        // _todosLosJuegosActuales: Guarda la lista completa de la sección actual (sin filtrar)
        private List<Juego> _todosLosJuegosActuales;

        // _juegosBanner: Lista específica para el banner
        private List<Juego> _juegosBanner;

        private DispatcherTimer _bannerTimer;
        private int _bannerIndex = 0;

        public HomeWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            if (lblNombreUsuario != null) lblNombreUsuario.Text = _usuarioActual.Username.ToUpper();

            // Verificación de Roles para mostrar botón ADMIN
            if (this.FindName("btnAdminPanel") is Button btnAdmin)
            {
                bool esPersonal = _usuarioActual.Rol == "SUPERADMIN" ||
                                  _usuarioActual.Rol == "GAME_ADMIN" ||
                                  _usuarioActual.Rol == "SUPPORT_ADMIN" ||
                                  _usuarioActual.Rol == "USER_ADMIN";

                btnAdmin.Visibility = esPersonal ? Visibility.Visible : Visibility.Collapsed;
            }

            this.WindowState = WindowState.Maximized;
            if (btnMaximizar != null) btnMaximizar.Content = "❐";

            ConfigurarBanner();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BtnTienda_Click(null, null);
        }

        // ================================================================
        // BÚSQUEDA Y FILTROS (NUEVO)
        // ================================================================

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void CmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_todosLosJuegosActuales == null) return;

            // 1. Empezamos con todos los juegos de la sección
            var resultado = _todosLosJuegosActuales.AsEnumerable();

            // 2. Filtro por Texto (Nombre)
            string busqueda = txtBusqueda.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(busqueda))
            {
                resultado = resultado.Where(j => j.Titulo.ToLower().Contains(busqueda));
            }

            // 3. Ordenación
            if (cmbFiltro.SelectedIndex == 1) // Nombre A-Z
            {
                resultado = resultado.OrderBy(j => j.Titulo);
            }
            else if (cmbFiltro.SelectedIndex == 2) // Precio Menor a Mayor
            {
                resultado = resultado.OrderBy(j => j.Precio);
            }
            else if (cmbFiltro.SelectedIndex == 3) // Precio Mayor a Menor
            {
                resultado = resultado.OrderByDescending(j => j.Precio);
            }

            // 4. Actualizar la vista
            listaJuegos.ItemsSource = resultado.ToList();
        }

        // ================================================================
        // NAVEGACIÓN (Actualiza la lista base y limpia filtros)
        // ================================================================

        private void CargarSeccion(List<Juego> juegos, string titulo)
        {
            _todosLosJuegosActuales = juegos;
            if (lblTituloSeccion != null) lblTituloSeccion.Text = titulo;

            // Limpiamos búsqueda al cambiar de sección (opcional)
            txtBusqueda.Text = "";
            cmbFiltro.SelectedIndex = 0;

            AplicarFiltros();
        }

        private void BtnTienda_Click(object sender, RoutedEventArgs e)
        {
            CargarSeccion(DatabaseHelper.ObtenerJuegos(), "MERCADO GLOBAL");
        }

        private void BtnDestacados_Click(object sender, RoutedEventArgs e)
        {
            CargarSeccion(DatabaseHelper.ObtenerDestacados(), "JUEGOS DESTACADOS");
        }

        private void BtnMisJuegos_Click(object sender, RoutedEventArgs e)
        {
            CargarSeccion(DatabaseHelper.ObtenerMisJuegos(_usuarioActual.Id), "MI BIBLIOTECA");
        }

        // ================================================================
        // BANNER ROTATIVO
        // ================================================================
        private void ConfigurarBanner()
        {
            _juegosBanner = DatabaseHelper.ObtenerJuegos();

            if (_juegosBanner.Count > 0)
            {
                _bannerTimer = new DispatcherTimer();
                _bannerTimer.Interval = TimeSpan.FromSeconds(4);
                _bannerTimer.Tick += BannerTimer_Tick;
                _bannerTimer.Start();
                ActualizarBanner();
            }
            else
            {
                if (lblBannerTitulo != null) lblBannerTitulo.Text = "NO HAY JUEGOS";
            }
        }

        private void BannerTimer_Tick(object sender, EventArgs e)
        {
            _bannerIndex++;
            if (_bannerIndex >= _juegosBanner.Count) _bannerIndex = 0;
            ActualizarBanner();
        }

        private void ActualizarBanner()
        {
            if (_juegosBanner == null || _juegosBanner.Count == 0) return;

            var juego = _juegosBanner[_bannerIndex];

            try
            {
                if (lblBannerTitulo != null)
                    lblBannerTitulo.Text = juego.Titulo.ToUpper();

                if (imgBanner != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(juego.ImagenFondoAbsoluta, UriKind.RelativeOrAbsolute); // Usa la horizontal
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    imgBanner.Source = bitmap;
                }
            }
            catch { }
        }

        // ================================================================
        // ACCIONES
        // ================================================================
        private void BtnVerCarrito_Click(object sender, RoutedEventArgs e)
        {
            CarritoWindow ventanaCarrito = new CarritoWindow();
            ventanaCarrito.ShowDialog();

            if (ventanaCarrito.CompraRealizada)
            {
                foreach (var j in CarritoService.Cesta)
                {
                    if (!DatabaseHelper.UsuarioTieneJuego(_usuarioActual.Id, j.Id))
                        DatabaseHelper.ComprarJuego(_usuarioActual.Id, j.Id);
                }
                CarritoService.Vaciar();
                new CustomMessageBox("COMPRA COMPLETADA", "Juegos añadidos a tu biblioteca.", Brushes.LimeGreen, false).ShowDialog();
                BtnMisJuegos_Click(null, null);
            }
        }

        private void JuegoDinamico_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag != null)
            {
                int idJuego = (int)card.Tag;
                // Importante: Buscar en la lista completa original
                Juego juego = _todosLosJuegosActuales.Find(j => j.Id == idJuego);

                if (juego != null)
                {
                    GameDetailWindow detalle = new GameDetailWindow(juego, _usuarioActual);
                    detalle.ShowDialog();
                    if (lblTituloSeccion.Text == "MI BIBLIOTECA") BtnMisJuegos_Click(null, null);
                }
            }
        }

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            // PASAR "_usuarioActual" (el objeto entero), NO "_usuarioActual.Id"
            AdminWindow admin = new AdminWindow(_usuarioActual);
            admin.ShowDialog();

            // Recargar tienda al salir por si hubo cambios
            BtnTienda_Click(null, null);
        }

        // ================================================================
        // UTILS
        // ================================================================
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left && this.WindowState == WindowState.Normal) this.DragMove(); }
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnMaximizar_Click(object sender, RoutedEventArgs e) { this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized; }

        private void UserProfile_Click(object sender, MouseButtonEventArgs e) { if (UserMenuPopup != null) UserMenuPopup.IsOpen = true; }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }

        private void BtnComunidad_Click(object sender, RoutedEventArgs e)
        {
            // Abrimos la ventana de comunidad
            new ComunidadWindow().ShowDialog();
        }
        private void BtnSoporte_Click(object sender, RoutedEventArgs e)
        {
            // Ahora pasamos "_usuarioActual" para saber quién envía la queja
            new SoporteWindow(_usuarioActual).ShowDialog();
        }
        private void BtnSuscripciones_Click(object sender, RoutedEventArgs e) { new SubscriptionsWindow(_usuarioActual).ShowDialog(); }
        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e) { new ConfigWindow(_usuarioActual).ShowDialog(); }
        private void AbrirSeccion(string t) { new SubVentana(t).ShowDialog(); }

        private void BtnEstadoOnline_Click(object sender, RoutedEventArgs e) => CambiarColorEstado("#23A559");
        private void BtnEstadoAusente_Click(object sender, RoutedEventArgs e) => CambiarColorEstado("#F0B232");
        private void BtnEstadoInvisible_Click(object sender, RoutedEventArgs e) => CambiarColorEstado("#747F8D");
        private void CambiarColorEstado(string c) { if (MainStatusIndicator != null) MainStatusIndicator.Background = (Brush)new BrushConverter().ConvertFromString(c); UserMenuPopup.IsOpen = false; }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.FindName("BackgroundTranslateTransform") is TranslateTransform trans) trans.Y = -(e.VerticalOffset * 0.1);
        }
    }
}