using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class AdminUsuariosWindow : Window
    {
        private int _idAdmin;
        private Usuario _usuarioSeleccionado;

        public AdminUsuariosWindow(int idAdmin)
        {
            InitializeComponent();
            _idAdmin = idAdmin;
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            try
            {
                dgUsuarios.ItemsSource = DatabaseHelper.ObtenerUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando usuarios: " + ex.Message);
            }
        }

        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _usuarioSeleccionado = dgUsuarios.SelectedItem as Usuario;

            if (_usuarioSeleccionado != null)
            {
                PanelDetalles.IsEnabled = true;
                PanelDetalles.Opacity = 1;

                txtUsername.Text = _usuarioSeleccionado.Username;
                txtEmail.Text = _usuarioSeleccionado.Email;
                cmbRol.Text = _usuarioSeleccionado.Rol;

                if (!_usuarioSeleccionado.Activo)
                {
                    lblEstadoBaneo.Text = "BANEADO";
                    lblEstadoBaneo.Foreground = Brushes.Red;
                    cmbGradoBaneo.SelectedIndex = _usuarioSeleccionado.GradoBaneo - 1;

                    if (_usuarioSeleccionado.FinBaneo.HasValue)
                        lblFinBaneo.Text = "Fin: " + _usuarioSeleccionado.FinBaneo.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        lblFinBaneo.Text = "Fin: Indefinido";
                }
                else
                {
                    lblEstadoBaneo.Text = "ACTIVO";
                    lblEstadoBaneo.Foreground = Brushes.LimeGreen;
                    cmbGradoBaneo.SelectedIndex = -1;
                    lblFinBaneo.Text = "Sin sanciones activas";
                }
            }
            else
            {
                PanelDetalles.IsEnabled = false;
                PanelDetalles.Opacity = 0.5;
                LimpiarFormulario();
            }
        }

        private void BtnGuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;

            string nuevoRol = cmbRol.Text;
            string nuevoEmail = txtEmail.Text;

            if (_usuarioSeleccionado.Rol == "SUPERADMIN" && nuevoRol != "SUPERADMIN")
            {
                MessageBox.Show("No puedes degradar a un SuperAdmin.");
                return;
            }

            DatabaseHelper.ActualizarUsuario(_usuarioSeleccionado.Id, nuevoEmail, nuevoRol);
            MessageBox.Show("Datos actualizados.");
            CargarUsuarios();
        }

        private void BtnCambiarPass_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;
            if (string.IsNullOrWhiteSpace(txtNuevaPass.Text)) return;

            DatabaseHelper.AdminCambiarPass(_usuarioSeleccionado.Id, txtNuevaPass.Text, _idAdmin);
            MessageBox.Show($"Contraseña cambiada para {_usuarioSeleccionado.Username}.");
            txtNuevaPass.Clear();
        }

        private void BtnSancionar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;
            if (cmbGradoBaneo.SelectedIndex == -1) { MessageBox.Show("Selecciona un nivel de sanción."); return; }

            if (_usuarioSeleccionado.Rol.Contains("ADMIN"))
            {
                MessageBox.Show("No puedes banear a otro administrador.");
                return;
            }

            int grado = cmbGradoBaneo.SelectedIndex + 1;

            DatabaseHelper.AplicarSancion(_usuarioSeleccionado.Id, grado, _idAdmin);

            MessageBox.Show($"Usuario sancionado con Nivel {grado}.");
            CargarUsuarios();
        }

        private void BtnIndultar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;

            DatabaseHelper.LevantarCastigo(_usuarioSeleccionado.Id, _idAdmin);
            MessageBox.Show("Sanción levantada. El usuario ya puede entrar.");
            CargarUsuarios();
        }

        // === NUEVAS FUNCIONES ===

        private void BtnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            // Abre la ventana de creación y espera a que cierre
            AdminCreateUserWindow ventanaCrear = new AdminCreateUserWindow(_idAdmin);
            ventanaCrear.ShowDialog();

            // Recargamos la lista por si se ha creado uno nuevo
            CargarUsuarios();
        }

        private void BtnVerApelaciones_Click(object sender, RoutedEventArgs e)
        {
            // Abre la ventana de apelaciones
            ApelacionesWindow ventanaApelaciones = new ApelacionesWindow();
            ventanaApelaciones.ShowDialog();

            // Recargamos por si se ha desbaneado a alguien desde allí
            CargarUsuarios();
        }

        // =======================

        private void LimpiarFormulario()
        {
            txtUsername.Clear();
            txtEmail.Clear();
            txtNuevaPass.Clear();
            cmbRol.SelectedIndex = -1;
            cmbGradoBaneo.SelectedIndex = -1;
            lblEstadoBaneo.Text = "-";
            lblFinBaneo.Text = "-";
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
    }
}