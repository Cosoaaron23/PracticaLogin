using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PracticaLogin
{
    public partial class HomeWindow : Window
    {
        private Usuario _usuarioActual;
        private List<Juego> _todosLosJuegosActuales = new List<Juego>();
        private List<Juego> _juegosBanner = new List<Juego>();
        private DispatcherTimer _timerChat;
        private DispatcherTimer _timerBanner;
        private Amigo _amigoChatActual = null;
        private bool _panelSocialAbierto = false;
        private int _indiceBanner = 0;

        public HomeWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            // Timer Chat
            _timerChat = new DispatcherTimer(); _timerChat.Interval = TimeSpan.FromSeconds(3); _timerChat.Tick += TimerChat_Tick; _timerChat.Start();
            // Timer Banner
            _timerBanner = new DispatcherTimer(); _timerBanner.Interval = TimeSpan.FromSeconds(5); _timerBanner.Tick += TimerBanner_Tick; _timerBanner.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatosUsuario();
            ConfigurarPermisos();
            _juegosBanner = DatabaseHelper.ObtenerDestacados();
            if (_juegosBanner.Count > 0) ActualizarBannerUI(_juegosBanner[0]);
            CargarJuegosIniciales();
        }

        // --- LOGICA BANNER ---
        private void TimerBanner_Tick(object sender, EventArgs e)
        {
            if (_juegosBanner != null && _juegosBanner.Count > 1)
            {
                _indiceBanner++; if (_indiceBanner >= _juegosBanner.Count) _indiceBanner = 0;
                ActualizarBannerUI(_juegosBanner[_indiceBanner]);
            }
        }
        private void ActualizarBannerUI(Juego juego)
        {
            if (juego == null) return;
            try
            {
                if (lblBannerTitulo != null) lblBannerTitulo.Text = juego.Titulo.ToUpper();
                if (imgBanner != null) imgBanner.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(juego.ImagenFondoAbsoluta, UriKind.RelativeOrAbsolute));
            }
            catch { }
        }

        // --- NAVEGACIÓN MODAL ---
        private void AbrirVentana(Window ventanaNueva)
        {
            if (DimmerOverlay != null) DimmerOverlay.Visibility = Visibility.Visible;
            ventanaNueva.Owner = this; ventanaNueva.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ventanaNueva.ShowDialog();
            if (DimmerOverlay != null) DimmerOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnSuscripciones_Click(object sender, RoutedEventArgs e) => AbrirVentana(new SubscriptionsWindow(_usuarioActual));
        private void BtnComunidad_Click(object sender, RoutedEventArgs e) => AbrirVentana(new ComunidadWindow(_usuarioActual));
        private void BtnVerCarrito_Click(object sender, RoutedEventArgs e) => AbrirVentana(new CarritoWindow(_usuarioActual));
        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e) => AbrirVentana(new AdminWindow(_usuarioActual));
        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e) => AbrirVentana(new ConfigWindow(_usuarioActual));
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }

        // CORREGIDO: Abre SoporteWindow con control de errores
        private void BtnSoporte_Click(object sender, RoutedEventArgs e)
        {
            try { AbrirVentana(new SoporteWindow(_usuarioActual)); } catch { MessageBox.Show("Soporte no disponible.", "Aviso"); }
        }

        private void JuegoDinamico_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.Tag is int idJuego)
            {
                var juego = _todosLosJuegosActuales.FirstOrDefault(j => j.Id == idJuego);
                if (juego != null) { try { AbrirVentana(new GameDetailWindow(juego, _usuarioActual)); } catch { } }
            }
        }

        // --- CARGA Y FILTROS ---
        private void CargarJuegosIniciales() => BtnDestacados_Click(null, null);
        private void CargarSeccion(List<Juego> juegos, string titulo)
        {
            _todosLosJuegosActuales = juegos ?? new List<Juego>();
            if (lblTituloSeccion != null) lblTituloSeccion.Text = titulo;
            AplicarFiltros();
        }
        private void BtnDestacados_Click(object sender, RoutedEventArgs e) => CargarSeccion(DatabaseHelper.ObtenerDestacados(), "MERCADO");
        private void BtnTienda_Click(object sender, RoutedEventArgs e) => CargarSeccion(DatabaseHelper.ObtenerJuegos(), "TIENDA");
        private void BtnMisJuegos_Click(object sender, RoutedEventArgs e) => CargarSeccion(DatabaseHelper.ObtenerBiblioteca(_usuarioActual.Id), "MI BIBLIOTECA");

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltros();
        private void CmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltros();
        // Evento nuevo para ordenación
        private void CmbOrden_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltros();

        private void AplicarFiltros()
        {
            if (listaJuegos == null || _todosLosJuegosActuales == null) return;

            string texto = (txtBusqueda != null) ? txtBusqueda.Text.ToLower() : "";
            string genero = (cmbFiltro != null && cmbFiltro.SelectedItem is ComboBoxItem itemG) ? itemG.Content.ToString() : "Todos";
            string orden = (cmbOrden != null && cmbOrden.SelectedItem is ComboBoxItem itemO) ? itemO.Content.ToString() : "A - Z";

            var query = _todosLosJuegosActuales.Where(j => j.Titulo.ToLower().Contains(texto));

            if (genero != "Todos" && !string.IsNullOrEmpty(genero)) query = query.Where(j => j.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));

            // Lógica de Ordenación
            switch (orden)
            {
                case "A - Z": query = query.OrderBy(j => j.Titulo); break;
                case "Z - A": query = query.OrderByDescending(j => j.Titulo); break;
                case "Precio: Bajo a Alto": query = query.OrderBy(j => j.Precio); break;
                case "Precio: Alto a Bajo": query = query.OrderByDescending(j => j.Precio); break;
                case "Tamaño: Pequeño a Grande": query = query.OrderBy(j => j.TamanoGb); break;
                default: query = query.OrderBy(j => j.Titulo); break;
            }

            listaJuegos.ItemsSource = query.ToList();
        }

        // --- UI ---
        private void CargarDatosUsuario() { if (_usuarioActual != null && lblNombreUsuario != null) lblNombreUsuario.Text = _usuarioActual.Username.ToUpper(); }
        private void ConfigurarPermisos() { if (btnAdminPanel != null) btnAdminPanel.Visibility = (_usuarioActual.Rol.Contains("ADMIN")) ? Visibility.Visible : Visibility.Collapsed; }
        private void UserProfile_Click(object sender, MouseButtonEventArgs e) { UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen; }
        private void BtnEstadoOnline_Click(object sender, RoutedEventArgs e) { CambiarEstadoColor("#23A559"); }
        private void BtnEstadoAusente_Click(object sender, RoutedEventArgs e) { CambiarEstadoColor("#FAA61A"); }
        private void BtnEstadoInvisible_Click(object sender, RoutedEventArgs e) { CambiarEstadoColor("#747F8D"); }
        private void CambiarEstadoColor(string c) { if (MainStatusIndicator != null) MainStatusIndicator.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(c); UserMenuPopup.IsOpen = false; }
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMaximizar_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) { if (BackgroundTranslateTransform != null) BackgroundTranslateTransform.Y = -MainScrollViewer.VerticalOffset * 0.2; }

        // CORREGIDO: MENÚ LATERAL SE OCULTA TOTALMENTE
        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (SideMenuBorder != null) SideMenuBorder.Visibility = (SideMenuBorder.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        }

        // --- SOCIAL ---
        private void BtnToggleSocial_Click(object sender, RoutedEventArgs e) { _panelSocialAbierto = !_panelSocialAbierto; if (SocialPanel != null) SocialPanel.Visibility = _panelSocialAbierto ? Visibility.Visible : Visibility.Collapsed; if (_panelSocialAbierto) RefrescarDatosSocial(); }
        private void BtnCerrarPanelSocial_Click(object sender, RoutedEventArgs e) { _panelSocialAbierto = false; SocialPanel.Visibility = Visibility.Collapsed; }
        private void TimerChat_Tick(object sender, EventArgs e) { if (_panelSocialAbierto) RefrescarDatosSocial(); }
        private void RefrescarDatosSocial() { if (_amigoChatActual == null) { if (listaAmigosUI != null) listaAmigosUI.ItemsSource = ChatHelper.ObtenerAmigos(_usuarioActual.Id); } else { if (listaMensajesUI != null) listaMensajesUI.ItemsSource = ChatHelper.ObtenerConversacion(_usuarioActual.Id, _amigoChatActual.Id); } }
        private void ItemAmigo_Click(object sender, MouseButtonEventArgs e) { if (sender is Border b && b.Tag is int id) { var lista = listaAmigosUI.ItemsSource as List<Amigo>; _amigoChatActual = lista?.Find(a => a.Id == id); if (_amigoChatActual != null) { lblNombreAmigoChat.Text = _amigoChatActual.Username.ToUpper(); VistaListaAmigos.Visibility = Visibility.Collapsed; VistaChatAbierto.Visibility = Visibility.Visible; RefrescarDatosSocial(); } } }
        private void BtnCerrarChat_Click(object sender, RoutedEventArgs e) { _amigoChatActual = null; VistaChatAbierto.Visibility = Visibility.Collapsed; VistaListaAmigos.Visibility = Visibility.Visible; RefrescarDatosSocial(); }
        private void BtnEnviarMensaje_Click(object sender, RoutedEventArgs e) => EnviarMensaje();
        private void TxtMensaje_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) EnviarMensaje(); }
        private void EnviarMensaje() { if (!string.IsNullOrWhiteSpace(txtMensaje.Text) && _amigoChatActual != null) { ChatHelper.EnviarMensaje(_usuarioActual.Id, _amigoChatActual.Id, txtMensaje.Text); txtMensaje.Text = ""; RefrescarDatosSocial(); if (scrollMensajes != null) scrollMensajes.ScrollToBottom(); } }
        private void BtnAddFriend_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Añadir amigos próximamente.");
        private void BtnSocialSettings_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Ajustes de privacidad.");
    }
}