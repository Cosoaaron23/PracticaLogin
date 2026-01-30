using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PracticaLogin
{
    public partial class GameDetailWindow : Window
    {
        private Juego _juego;
        private Usuario _user;
        private DispatcherTimer _timerDescarga;
        private double _progreso = 0;

        public GameDetailWindow(Juego juego, Usuario usuario)
        {
            InitializeComponent();
            _juego = juego;
            _user = usuario;

            // Datos base
            lblTitulo.Text = juego.Titulo;
            lblGenero.Text = juego.Genero;
            lblGigas.Text = juego.TamanoGb + " GB";
            txtDescripcion.Text = "Descubre la experiencia definitiva en " + juego.Titulo + ". Optimizado para el Launcher AKAY.";

            try { imgPortada.Source = new BitmapImage(new Uri(juego.ImagenAbsoluta)); } catch { }

            ActualizarEstadoBotones();
        }

        private void ActualizarEstadoBotones()
        {
            // Pedimos el estado a la base de datos
            // 0 = No comprado, 1 = Comprado, 2 = Descargado
            int estado = DatabaseHelper.EstadoPropiedadJuego(_user.Id, _juego.Id);

            btnCarrito.Visibility = Visibility.Collapsed;
            btnInstalar.Visibility = Visibility.Collapsed;
            btnJugar.Visibility = Visibility.Collapsed;

            if (estado == 0)
            {
                btnCarrito.Visibility = Visibility.Visible;
                if (_juego.Precio == 0) btnCarrito.Content = "⬇ OBTENER GRATIS";
            }
            else if (estado == 1)
            {
                btnInstalar.Visibility = Visibility.Visible;
            }
            else
            {
                btnJugar.Visibility = Visibility.Visible;
            }
        }

        // --- LÓGICA DE DESCARGA SIMULADA ---
        private void BtnInstalar_Click(object sender, RoutedEventArgs e)
        {
            btnInstalar.Visibility = Visibility.Collapsed;
            panelDescarga.Visibility = Visibility.Visible;

            _timerDescarga = new DispatcherTimer();
            _timerDescarga.Interval = TimeSpan.FromMilliseconds(50); // Velocidad del tick
            _timerDescarga.Tick += Timer_Tick;
            _timerDescarga.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _progreso += 1.2; // Cuánto aumenta la barra en cada paso
            pbDescarga.Value = _progreso;
            lblPorcentaje.Text = $"Descargando... {(int)_progreso}%";

            // Simulación de velocidad variable
            lblVelocidad.Text = new Random().Next(30, 85) + " MB/s";

            if (_progreso >= 100)
            {
                _timerDescarga.Stop();
                // Guardamos en la BD que ya está descargado
                DatabaseHelper.MarcarComoDescargado(_user.Id, _juego.Id);

                panelDescarga.Visibility = Visibility.Collapsed;
                new CustomMessageBox("COMPLETADO", "El juego se ha instalado correctamente.", System.Windows.Media.Brushes.LimeGreen, false).ShowDialog();

                ActualizarEstadoBotones();
            }
        }

        private void BtnCarrito_Click(object sender, RoutedEventArgs e)
        {
            CarritoService.Agregar(_juego);
            MessageBox.Show("Añadido al carrito. Finaliza la compra en la Home.");
            this.Close();
        }

        private void BtnJugar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Abriendo {_juego.Titulo}...", "AKAY Launcher");
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}