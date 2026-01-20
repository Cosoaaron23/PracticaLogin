using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class AdminWindow : Window
    {
        private int _idAdminLogueado;
        private List<Usuario> _listaUsuarios;

        public AdminWindow(int idAdmin)
        {
            InitializeComponent();
            _idAdminLogueado = idAdmin;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarTabla();
        }

        // Arrastrar ventana
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- 1. CARGAR DATOS ---
        private void CargarTabla(string filtro = "")
        {
            if (string.IsNullOrEmpty(filtro))
                _listaUsuarios = DatabaseHelper.ObtenerUsuarios();
            else
                _listaUsuarios = DatabaseHelper.BuscarUsuarios(filtro);

            dgUsuarios.ItemsSource = _listaUsuarios;

            // Resetear UI
            lblSelectedUser.Text = "Ninguno";
            txtNuevaPass.Password = "";

            if (btnBanear != null)
            {
                btnBanear.IsEnabled = false;
                btnBanear.Content = "BLOQUEAR ACCESO";
                btnBanear.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // Gris original
                btnBanear.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e) => CargarTabla(txtBusqueda.Text);

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarTabla(txtBusqueda.Text);
        }

        // --- 2. SELECCIÓN DE USUARIO (COLORES ARREGLADOS) ---
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                lblSelectedUser.Text = u.Username;
                cmbRol.Text = u.Rol;

                if (btnBanear != null)
                {
                    btnBanear.IsEnabled = true;
                    // CORRECCIÓN: Letras NEGRAS para que se vean bien
                    btnBanear.Foreground = Brushes.Black;

                    if (u.Activo)
                    {
                        btnBanear.Content = "BANEAR (BLOQUEAR)";
                        btnBanear.Background = Brushes.Red;
                    }
                    else
                    {
                        btnBanear.Content = "DESBANEAR (ACTIVAR)";
                        btnBanear.Background = Brushes.LimeGreen;
                    }
                }
            }
        }

        // --- 3. GUARDAR CAMBIOS MASIVOS ---
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_listaUsuarios != null)
                {
                    foreach (var u in _listaUsuarios)
                    {
                        // Pasamos el ID del admin para el Log
                        DatabaseHelper.ActualizarUsuario(u.Id, u.Email, u.Rol, _idAdminLogueado);
                    }
                    MostrarMensaje("ÉXITO", "Cambios guardados y registrados en el Log.", Brushes.LimeGreen);
                    CargarTabla(txtBusqueda.Text);
                }
            }
            catch
            {
                MostrarMensaje("ERROR", "Error al guardar cambios.", Brushes.Red);
            }
        }

        // --- 4. BANEAR / DESBANEAR ---
        private void BtnBanear_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                if (u.Id == _idAdminLogueado)
                {
                    MostrarMensaje("ERROR", "No puedes banearte a ti mismo.", Brushes.Red);
                    return;
                }

                var confirm = new CustomMessageBox("CONFIRMACIÓN", $"¿Cambiar estado de {u.Username}?", Brushes.Orange);
                if (confirm.ShowDialog() == true)
                {
                    bool exito = DatabaseHelper.AlternarBloqueo(u.Id, u.Activo, _idAdminLogueado);
                    if (exito)
                    {
                        u.Activo = !u.Activo;
                        dgUsuarios.Items.Refresh();
                        DgUsuarios_SelectionChanged(null, null); // Actualiza color botón
                        MostrarMensaje("LISTO", "Estado actualizado.", Brushes.Cyan);
                    }
                }
            }
        }

        // --- 5. CAMBIAR CONTRASEÑA ---
        private void BtnCambiarPass_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                string pass = txtNuevaPass.Password;
                if (string.IsNullOrWhiteSpace(pass))
                {
                    MostrarMensaje("AVISO", "Escribe una contraseña.", Brushes.Yellow);
                    return;
                }

                var confirm = new CustomMessageBox("SEGURIDAD", $"¿Resetear contraseña a {u.Username}?", Brushes.Red);
                if (confirm.ShowDialog() == true)
                {
                    DatabaseHelper.AdminCambiarPass(u.Id, pass, _idAdminLogueado);

                    u.Password = pass;
                    dgUsuarios.Items.Refresh();

                    MostrarMensaje("ÉXITO", "Contraseña actualizada.", Brushes.LimeGreen);
                    txtNuevaPass.Password = "";
                }
            }
            else
            {
                MostrarMensaje("AVISO", "Selecciona un usuario.", Brushes.Yellow);
            }
        }

        // --- 6. GENERAR REPORTE (EXTENSIÓN PDF) ---
        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string reporte = DatabaseHelper.ObtenerReporteLogs();
                string rutaEscritorio = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string rutaArchivo = System.IO.Path.Combine(rutaEscritorio, "Reporte_Actividad_Admin.txt");

                System.IO.File.WriteAllText(rutaArchivo, reporte);

                MostrarMensaje("REPORTE CREADO", "Se ha guardado en el Escritorio.", Brushes.Cyan);

                // Abrir archivo automáticamente
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = rutaArchivo,
                    UseShellExecute = true
                });
            }
            catch (System.Exception ex)
            {
                MostrarMensaje("ERROR", ex.Message, Brushes.Red);
            }
        }

        // --- MÉTODO AUXILIAR PARA MENSAJES OSCUROS ---
        private void MostrarMensaje(string titulo, string mensaje, Brush color)
        {
            CustomMessageBox msg = new CustomMessageBox(titulo, mensaje, color);
            msg.ShowDialog();
        }
    }
}