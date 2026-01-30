using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading; // Necesario para el Timer

namespace PracticaLogin
{
    public partial class GameDetailWindow : Window
    {
        private Usuario _usuario;
        private Juego _juego;
        private DispatcherTimer _timerDescarga;
        private int _progresoDescarga = 0;

        // Estados del juego
        private const int ESTADO_NO_TIENE = 0;
        private const int ESTADO_COMPRADO = 1;
        private const int ESTADO_DESCARGADO = 2;

        private int _estadoActual = 0;

        public GameDetailWindow(Juego juego, Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _juego = juego;

            CargarDatosVisuales();
            VerificarEstadoJuego();
        }

        private void CargarDatosVisuales()
        {
            lblTitulo.Text = _juego.Titulo.ToUpper();
            lblGenero.Text = _juego.Genero.ToUpper();
            lblEspacio.Text = _juego.TamanoGb + " GB";
            lblJugadores.Text = _juego.NumJugadores == 1 ? "UN JUGADOR" : "MULTIJUGADOR";
            lblPrecio.Text = _juego.Precio == 0 ? "GRATIS" : _juego.Precio.ToString("C");

            // Imágenes con protección
            try { imgPortada.Source = new BitmapImage(new Uri(_juego.ImagenAbsoluta, UriKind.RelativeOrAbsolute)); } catch { }
            try { imgFondo.Source = new BitmapImage(new Uri(_juego.ImagenAbsoluta, UriKind.RelativeOrAbsolute)); } catch { }

            if (_juego.EsOnline) tagOnline.Visibility = Visibility.Visible;
        }

        // --- CEREBRO: Decide qué botón mostrar ---
        private void VerificarEstadoJuego()
        {
            // 1. Preguntamos a la BBDD en qué estado está este juego para este usuario
            _estadoActual = DatabaseHelper.EstadoPropiedadJuego(_usuario.Id, _juego.Id);

            switch (_estadoActual)
            {
                case ESTADO_NO_TIENE:
                    // Si es Gratis, permitimos "Obtener", si no "Añadir al Carrito"
                    if (_juego.Precio == 0)
                    {
                        btnAccion.Content = "AÑADIR A BIBLIOTECA";
                        btnAccion.Background = System.Windows.Media.Brushes.LimeGreen;
                    }
                    else
                    {
                        btnAccion.Content = "AÑADIR AL CARRITO";
                        btnAccion.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#00E5FF");
                    }
                    lblPrecio.Visibility = Visibility.Visible;
                    break;

                case ESTADO_COMPRADO:
                    btnAccion.Content = "⬇ DESCARGAR";
                    btnAccion.Background = System.Windows.Media.Brushes.Orange; // Naranja para acción requerida
                    lblPrecio.Text = "EN PROPIEDAD";
                    break;

                case ESTADO_DESCARGADO:
                    btnAccion.Content = "▶ JUGAR AHORA";
                    btnAccion.Background = System.Windows.Media.Brushes.LimeGreen; // Verde para jugar
                    lblPrecio.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void BtnAccion_Click(object sender, RoutedEventArgs e)
        {
            Brush colorCian = (Brush)new BrushConverter().ConvertFrom("#00E5FF");

            switch (_estadoActual)
            {
                case ESTADO_NO_TIENE:
                    ProcesarCompra();
                    break;

                case ESTADO_COMPRADO:
                    // Usamos tu CustomMessageBox para confirmar la descarga
                    CustomMessageBox msgD = new CustomMessageBox("Descargar", "¿Deseas iniciar la descarga de " + _juego.Titulo + "?", colorCian, true);
                    if (msgD.ShowDialog() == true)
                    {
                        IniciarSimulacionDescarga();
                    }
                    break;

                case ESTADO_DESCARGADO:
                    // Mensaje informativo (esConfirmacion = false para ocultar botón cancelar)
                    CustomMessageBox msgJ = new CustomMessageBox("Ejecutar", "Iniciando " + _juego.Titulo + "... ¡Disfruta!", colorCian, false);
                    msgJ.ShowDialog();
                    break;
            }
        }

        private void ProcesarCompra()
        {
            Brush colorCian = (Brush)new BrushConverter().ConvertFrom("#00E5FF");
            if (_juego.Precio == 0)
            {
                DatabaseHelper.ComprarJuego(_usuario.Id, _juego.Id);
                // Nuevo aviso estético para juego gratuito
                CustomMessageBox msgG = new CustomMessageBox("Éxito", "¡Juego gratuito añadido a tu biblioteca!", colorCian, false);
                msgG.ShowDialog();
                VerificarEstadoJuego();
            }
            else
            {
                CarritoService.Agregar(_juego);
                // NUEVO: Cambio del aviso genérico por el CustomMessageBox
                CustomMessageBox msgC = new CustomMessageBox("Carrito", "Juego añadido al carrito. Ve al carrito para finalizar la compra.", colorCian, false);
                msgC.ShowDialog();
                this.Close();
            }
        }

        // --- SIMULACIÓN DE DESCARGA ---
        private void IniciarSimulacionDescarga()
        {
            btnAccion.IsEnabled = false; // Bloquear botón
            btnAccion.Content = "PREPARANDO...";
            pnlDescarga.Visibility = Visibility.Visible; // Mostrar barra

            _progresoDescarga = 0;
            _timerDescarga = new DispatcherTimer();
            _timerDescarga.Interval = TimeSpan.FromMilliseconds(50); // Velocidad de descarga
            _timerDescarga.Tick += TimerDescarga_Tick;
            _timerDescarga.Start();
        }

        private void TimerDescarga_Tick(object sender, EventArgs e)
        {
            _progresoDescarga++;
            progressBar.Value = _progresoDescarga;
            lblEstadoDescarga.Text = $"Descargando... {_progresoDescarga}%";

            if (_progresoDescarga >= 100)
            {
                _timerDescarga.Stop();
                FinalizarDescarga();
            }
        }

        private void FinalizarDescarga()
        {
            DatabaseHelper.MarcarComoDescargado(_usuario.Id, _juego.Id);
            pnlDescarga.Visibility = Visibility.Collapsed;
            btnAccion.IsEnabled = true;

            Brush colorVerde = Brushes.LimeGreen; // Podemos usar verde para el éxito
            CustomMessageBox msgF = new CustomMessageBox("Listo", "Descarga e instalación completadas.", colorVerde, false);
            msgF.ShowDialog();

            VerificarEstadoJuego();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}