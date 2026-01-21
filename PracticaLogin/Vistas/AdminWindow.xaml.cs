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

        private void Window_Loaded(object sender, RoutedEventArgs e) => CargarTabla();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        // 1. CARGA
        private void CargarTabla(string filtro = "")
        {
            _listaUsuarios = string.IsNullOrEmpty(filtro) ? DatabaseHelper.ObtenerUsuarios() : DatabaseHelper.BuscarUsuarios(filtro);
            dgUsuarios.ItemsSource = _listaUsuarios;

            // Reset UI
            lblSelectedUser.Text = "Selecciona un usuario";
            txtNuevaPass.Password = "";

            // Botones desactivados por defecto
            btnAplicarSancion.IsEnabled = false;
            btnAplicarSancion.Content = "APLICAR SANCIÓN";
            btnAplicarSancion.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // Gris

            btnLevantarCastigo.Visibility = Visibility.Collapsed; // Oculto
        }
        private void BtnRefrescar_Click(object sender, RoutedEventArgs e) => CargarTabla(txtBusqueda.Text);
        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e) => CargarTabla(txtBusqueda.Text);

        // 2. SELECCIÓN INTELIGENTE
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                lblSelectedUser.Text = u.Username;
                cmbRol.Text = u.Rol;

                // Habilitamos controles
                btnAplicarSancion.IsEnabled = true;

                if (u.Activo)
                {
                    // --- CASO: USUARIO LIMPIO ---
                    btnAplicarSancion.Content = "APLICAR SANCIÓN";
                    btnAplicarSancion.Background = Brushes.Red;
                    btnAplicarSancion.Foreground = Brushes.White;

                    btnLevantarCastigo.Visibility = Visibility.Collapsed; // No hay nada que levantar

                    cmbGravedad.SelectedIndex = 0; // Por defecto Grado 1
                }
                else
                {
                    // --- CASO: USUARIO YA BANEADO ---
                    btnAplicarSancion.Content = "ACTUALIZAR SANCIÓN"; // Para cambiar el grado
                    btnAplicarSancion.Background = Brushes.Orange;    // Naranja para diferenciar
                    btnAplicarSancion.Foreground = Brushes.Black;

                    btnLevantarCastigo.Visibility = Visibility.Visible; // ¡AQUÍ ESTÁ EL BOTÓN DE QUITAR!

                    // Pre-seleccionar el grado actual en el combo
                    if (u.GradoBaneo >= 1 && u.GradoBaneo <= 5)
                        cmbGravedad.SelectedIndex = u.GradoBaneo - 1;
                }
            }
        }

        // 3. APLICAR O ACTUALIZAR SANCIÓN (Botón Rojo/Naranja)
        private void BtnAplicarSancion_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                if (u.Id == _idAdminLogueado) { MostrarMensaje("ERROR", "No puedes sancionarte a ti mismo.", Brushes.Red); return; }

                int grado = cmbGravedad.SelectedIndex + 1; // 1-5
                string accion = u.Activo ? "APLICAR" : "ACTUALIZAR";

                var confirm = new CustomMessageBox("CONFIRMAR", $"¿{accion} Grado {grado} a {u.Username}?", Brushes.Orange);
                if (confirm.ShowDialog() == true)
                {
                    DatabaseHelper.AplicarSancion(u.Id, grado, _idAdminLogueado);

                    // Actualizamos visualmente sin recargar todo
                    u.Activo = false;
                    u.GradoBaneo = grado;
                    // Forzamos recarga para recalcular fecha (opcional, o llamamos a CargarTabla)
                    CargarTabla(txtBusqueda.Text);

                    MostrarMensaje("LISTO", $"Sanción actualizada a Grado {grado}.", Brushes.Red);
                }
            }
        }

        // 4. LEVANTAR CASTIGO (Botón Verde)
        private void BtnLevantarCastigo_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                var confirm = new CustomMessageBox("INDULTO", $"¿Levantar castigo a {u.Username}?", Brushes.Green);
                if (confirm.ShowDialog() == true)
                {
                    DatabaseHelper.LevantarCastigo(u.Id);

                    // Actualizamos visualmente
                    u.Activo = true;
                    u.GradoBaneo = 0;
                    CargarTabla(txtBusqueda.Text);

                    MostrarMensaje("LIBERADO", "El usuario tiene acceso de nuevo.", Brushes.Cyan);
                }
            }
        }

        // 5. RESTO DE FUNCIONES (Igual que antes)
        private void BtnApelaciones_Click(object sender, RoutedEventArgs e) => new ApelacionesWindow().ShowDialog();

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_listaUsuarios != null)
                {
                    foreach (var u in _listaUsuarios)
                    {
                        // Asegúrate de que llamas a la versión nueva que te acabo de pasar (3 argumentos)
                        // ID, EMAIL, ROL
                        DatabaseHelper.ActualizarUsuario(u.Id, u.Email, u.Rol);
                    }
                    new CustomMessageBox("ÉXITO", "Cambios guardados correctamente.", Brushes.LimeGreen).ShowDialog();
                }
            }
            catch (Exception ex)
            {
                // Ahora si falla, el mensaje te dirá POR QUÉ falla (ex.Message)
                new CustomMessageBox("ERROR", "Error al guardar: " + ex.Message, Brushes.Red).ShowDialog();
            }
        }

        private void BtnCambiarPass_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                string pass = txtNuevaPass.Password;
                if (!string.IsNullOrWhiteSpace(pass))
                {
                    DatabaseHelper.AdminCambiarPass(u.Id, pass, _idAdminLogueado);
                    u.Password = pass;
                    dgUsuarios.Items.Refresh();
                    MostrarMensaje("ÉXITO", "Contraseña cambiada.", Brushes.LimeGreen);
                    txtNuevaPass.Password = "";
                }
            }
        }

        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string r = DatabaseHelper.ObtenerReporteLogs();
                string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "Reporte_Admin.txt");
                System.IO.File.WriteAllText(path, r);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch { MostrarMensaje("ERROR", "Fallo al crear reporte.", Brushes.Red); }
        }

        private void MostrarMensaje(string t, string m, Brush c) { new CustomMessageBox(t, m, c).ShowDialog(); }
    }
}