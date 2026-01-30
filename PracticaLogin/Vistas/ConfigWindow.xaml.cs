using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace PracticaLogin
{
    public partial class ConfigWindow : Window
    {
        private Usuario _usuarioActual;

        public ConfigWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            CargarDatosUsuario();

            // Cargamos la configuración guardada al abrir
            CargarConfiguracion();
        }

        private void CargarDatosUsuario()
        {
            if (_usuarioActual != null)
            {
                lblUsuarioCuenta.Text = _usuarioActual.Username.ToUpper();
                lblIdCuenta.Text = $"ID: #{_usuarioActual.Id:D4}";
            }
        }

        // --- LÓGICA DE GUARDADO Y CARGA (FUNCIONALIDAD) ---

        private void CargarConfiguracion()
        {
            // Aquí recuperamos los valores de Properties.Settings.Default
            // Nota: Si no has creado el archivo de Settings, esto usará valores por defecto visuales.

            try
            {
                // Ejemplo: Recuperar Checkboxes (Si existieran en Settings)
                // chkInicioWindows.IsChecked = Properties.Settings.Default.InicioWindows;
                // Como es una práctica, los dejamos como están en el XAML o simulamos carga:

                // Simulación de carga visual
                cmbIdioma.SelectedIndex = 0; // Español
                cmbResolucion.SelectedIndex = 2; // 1920x1080
            }
            catch { }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // A) LÓGICA DE IDIOMA
            string idiomaSeleccionado = "es"; // Por defecto
            if (cmbIdioma.SelectedItem is ComboBoxItem itemLang && itemLang.Tag != null)
            {
                idiomaSeleccionado = itemLang.Tag.ToString();
            }

            // Aquí simulamos que guardamos la preferencia
            // En una app real: Properties.Settings.Default.Idioma = idiomaSeleccionado;

            string mensajeIdioma = "";
            if (idiomaSeleccionado != "es")
            {
                mensajeIdioma = "\n\nNOTA: Has cambiado el idioma. Reinicia el launcher para aplicar los cambios de texto.";
            }

            // B) LÓGICA DE DIRECTORIO
            string rutaJuegos = txtDirectorio.Text;
            // En una app real: Properties.Settings.Default.RutaJuegos = rutaJuegos;

            // C) LÓGICA DE RESOLUCIÓN (Visual)
            if (cmbResolucion.SelectedItem is ComboBoxItem itemRes && itemRes.Tag != null)
            {
                string[] res = itemRes.Tag.ToString().Split(',');
                if (res.Length == 2)
                {
                    this.Width = double.Parse(res[0]);
                    this.Height = double.Parse(res[1]);

                    // Recentrar ventana
                    Rect workArea = SystemParameters.WorkArea;
                    this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
                    this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
                }
            }

            MessageBox.Show($"Configuración Guardada Correctamente.\n\n" +
                            $"📂 Ruta de Juegos: {rutaJuegos}\n" +
                            $"🌍 Idioma: {idiomaSeleccionado.ToUpper()}" +
                            mensajeIdioma,
                            "Ajustes Actualizados", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --- FIN LÓGICA ---

        // Botón Cerrar (X)
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Botón Volver (Flecha)
        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            HomeWindow home = new HomeWindow(_usuarioActual);
            home.Show();
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        // Gestión de Paneles
        private void OcultarTodosPaneles()
        {
            pnlGeneral.Visibility = Visibility.Collapsed;
            pnlGraficos.Visibility = Visibility.Collapsed;
            pnlSonido.Visibility = Visibility.Collapsed;
            pnlCuenta.Visibility = Visibility.Collapsed;
        }

        private void BtnSeleccionarDirectorio_Click(object sender, RoutedEventArgs e)
        {
            // Usamos OpenFileDialog de forma "trucada" para seleccionar carpetas
            // ya que WPF puro no tiene un "FolderBrowser" nativo moderno sencillo sin librerías externas.
            // Le pedimos al usuario que seleccione el ejecutable del juego o simplemente validamos la ruta.

            // Opción más simple compatible con todo: Usar System.Windows.Forms (si está disponible)
            // O usar OpenFileDialog configurado para filtrar.

            var dialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Seleccionar Carpeta", // Truco visual
                Title = "Selecciona la carpeta de instalación de juegos"
            };

            if (dialog.ShowDialog() == true)
            {
                // Obtenemos la ruta del directorio, quitando el nombre de archivo falso
                string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                txtDirectorio.Text = folderPath;
            }
        }

        private void BtnGeneral_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlGeneral.Visibility = Visibility.Visible; }
        private void BtnGraficos_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlGraficos.Visibility = Visibility.Visible; }
        private void BtnSonido_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlSonido.Visibility = Visibility.Visible; }
        private void BtnCuenta_Click(object sender, RoutedEventArgs e) { OcultarTodosPaneles(); pnlCuenta.Visibility = Visibility.Visible; }

        // Gestión de Cuenta
        private void BtnMostrarCambioPass_Click(object sender, RoutedEventArgs e)
        {
            pnlCambioPass.Visibility = (pnlCambioPass.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnConfirmarPass_Click(object sender, RoutedEventArgs e)
        {
            string nuevaPass = txtNuevaPass.Password;
            if (!string.IsNullOrEmpty(nuevaPass))
            {
                DatabaseHelper.UpdatePassword(_usuarioActual.Id, nuevaPass);
                txtNuevaPass.Password = "";
                pnlCambioPass.Visibility = Visibility.Collapsed;
                MessageBox.Show("Contraseña actualizada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("La contraseña no puede estar vacía.");
            }
        }

        private void BtnGestionarSuscripcion_Click(object sender, RoutedEventArgs e)
        {
            SubscriptionsWindow subWindow = new SubscriptionsWindow(_usuarioActual);
            subWindow.Show();
            this.Close();
        }
    }
}