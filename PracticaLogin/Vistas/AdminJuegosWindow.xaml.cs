using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PracticaLogin
{
    public partial class AdminJuegosWindow : Window
    {
        private string _rutaPortada = "";
        private string _rutaFondo = "";
        private Juego _juegoSeleccionado = null;

        public AdminJuegosWindow()
        {
            InitializeComponent();
            RefrescarTabla();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void RefrescarTabla()
        {
            try { dgJuegos.ItemsSource = DatabaseHelper.ObtenerJuegos(); }
            catch (Exception ex) { new CustomMessageBox("ERROR", ex.Message, Brushes.Red, false).ShowDialog(); }
        }

        private void DgJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _juegoSeleccionado = dgJuegos.SelectedItem as Juego;
            if (_juegoSeleccionado != null)
            {
                txtTitulo.Text = _juegoSeleccionado.Titulo;
                txtPrecio.Text = _juegoSeleccionado.Precio.ToString();
                txtGigas.Text = _juegoSeleccionado.TamanoGb.ToString();
                cmbGenero.Text = _juegoSeleccionado.Genero;
                chkOnline.IsChecked = _juegoSeleccionado.EsOnline;

                if (txtJugadores != null) txtJugadores.Text = _juegoSeleccionado.NumJugadores.ToString();

                // Cargar imágenes en los cuadros de vista previa
                _rutaPortada = _juegoSeleccionado.ImagenUrl;
                _rutaFondo = _juegoSeleccionado.ImagenFondoUrl;

                CargarPreview(imgPreviewPortada, _juegoSeleccionado.ImagenAbsoluta);
                CargarPreview(imgPreviewFondo, _juegoSeleccionado.ImagenFondoAbsoluta);

                btnCrear.Visibility = Visibility.Collapsed;
                btnActualizar.Visibility = Visibility.Visible;
            }
        }

        private void CargarPreview(Image imgControl, string ruta)
        {
            try { imgControl.Source = new BitmapImage(new Uri(ruta, UriKind.RelativeOrAbsolute)); } catch { imgControl.Source = null; }
        }

        // --- SUBIR PORTADA ---
        private void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            ProcesarSubidaImagen(ref _rutaPortada, imgPreviewPortada);
        }

        // --- SUBIR BANNER ---
        private void BtnSeleccionarFondo_Click(object sender, RoutedEventArgs e)
        {
            ProcesarSubidaImagen(ref _rutaFondo, imgPreviewFondo);
        }

        private void ProcesarSubidaImagen(ref string rutaDestino, Image imgControl)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string carpetaDestino = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
                    if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                    string nombreArchivo = "img_" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(openFileDialog.FileName);
                    string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                    File.Copy(openFileDialog.FileName, rutaCompleta, true);

                    rutaDestino = "/Assets/" + nombreArchivo;
                    CargarPreview(imgControl, rutaCompleta);
                }
                catch (Exception ex) { new CustomMessageBox("ERROR", ex.Message, Brushes.Red, false).ShowDialog(); }
            }
        }

        // --- BOTÓN CREAR ---
        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarDatos(out decimal precio, out double gb))
            {
                var confirm = new CustomMessageBox("NUEVO JUEGO", "¿Crear este videojuego?", Brushes.Cyan);
                if (confirm.ShowDialog() == true)
                {
                    int jugadores = 1;
                    if (txtJugadores != null) int.TryParse(txtJugadores.Text, out jugadores);

                    // Pasamos las DOS rutas
                    DatabaseHelper.CrearJuego(txtTitulo.Text, precio, gb, cmbGenero.Text, jugadores, chkOnline.IsChecked == true, _rutaPortada, _rutaFondo);

                    new CustomMessageBox("ÉXITO", "Juego creado.", Brushes.LimeGreen, false).ShowDialog();
                    FinalizarAccion();
                }
            }
        }

        // --- BOTÓN ACTUALIZAR ---
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_juegoSeleccionado != null && ValidarDatos(out decimal precio, out double gb))
            {
                var confirm = new CustomMessageBox("ACTUALIZAR", "¿Guardar cambios?", Brushes.Cyan);
                if (confirm.ShowDialog() == true)
                {
                    int jugadores = 1;
                    if (txtJugadores != null) int.TryParse(txtJugadores.Text, out jugadores);

                    DatabaseHelper.ActualizarJuego(_juegoSeleccionado.Id, txtTitulo.Text, precio, gb, cmbGenero.Text, jugadores, chkOnline.IsChecked == true, _rutaPortada, _rutaFondo);

                    new CustomMessageBox("ÉXITO", "Juego actualizado.", Brushes.LimeGreen, false).ShowDialog();
                    FinalizarAccion();
                }
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_juegoSeleccionado == null) return;
            var confirm = new CustomMessageBox("ELIMINAR", $"¿Borrar {_juegoSeleccionado.Titulo}?", Brushes.Red);
            if (confirm.ShowDialog() == true)
            {
                DatabaseHelper.EliminarJuego(_juegoSeleccionado.Id);
                new CustomMessageBox("BORRADO", "Juego eliminado.", Brushes.Orange, false).ShowDialog();
                FinalizarAccion();
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarFormulario();

        private void LimpiarFormulario()
        {
            _juegoSeleccionado = null;
            txtTitulo.Clear(); txtPrecio.Clear(); txtGigas.Clear();
            if (txtJugadores != null) txtJugadores.Clear();

            _rutaPortada = "";
            _rutaFondo = "";
            imgPreviewPortada.Source = null;
            imgPreviewFondo.Source = null;

            btnCrear.Visibility = Visibility.Visible;
            btnActualizar.Visibility = Visibility.Collapsed;
            dgJuegos.SelectedItem = null;
        }

        private bool ValidarDatos(out decimal precio, out double gb)
        {
            precio = 0; gb = 0;
            if (string.IsNullOrWhiteSpace(txtTitulo.Text)) { new CustomMessageBox("ERROR", "Escribe un título.", Brushes.Red, false).ShowDialog(); return false; }
            if (!decimal.TryParse(txtPrecio.Text.Replace(".", ","), out precio)) { new CustomMessageBox("ERROR", "Precio no válido.", Brushes.Red, false).ShowDialog(); return false; }
            if (!double.TryParse(txtGigas.Text.Replace(".", ","), out gb)) { new CustomMessageBox("ERROR", "Tamaño no válido.", Brushes.Red, false).ShowDialog(); return false; }
            return true;
        }

        private void FinalizarAccion() { RefrescarTabla(); LimpiarFormulario(); }
    }
}