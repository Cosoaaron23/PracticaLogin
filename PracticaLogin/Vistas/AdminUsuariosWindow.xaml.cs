using System;
using System.Collections.Generic;
using System.Linq; // NECESARIO PARA EL FILTRO
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
        private List<Usuario> _listaCompletaUsuarios; // Para guardar todos y filtrar en memoria

        public AdminUsuariosWindow(int idAdmin)
        {
            InitializeComponent();
            _idAdmin = idAdmin;
            CargarUsuarios();
        }

        private void CargarUsuarios(string filtro = "")
        {
            try
            {
                // 1. Obtenemos TODOS los usuarios de la BD
                _listaCompletaUsuarios = DatabaseHelper.ObtenerUsuarios();

                // 2. Si hay texto en el buscador, filtramos la lista en memoria
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    var listaFiltrada = _listaCompletaUsuarios.Where(u =>
                        u.Username.ToLower().Contains(filtro.ToLower()) ||
                        u.Email.ToLower().Contains(filtro.ToLower())
                    ).ToList();

                    dgUsuarios.ItemsSource = listaFiltrada;
                }
                else
                {
                    // Si no hay filtro, mostramos todos
                    dgUsuarios.ItemsSource = _listaCompletaUsuarios;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando usuarios: " + ex.Message);
            }
        }

        // --- EVENTO DEL BUSCADOR ---
        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarUsuarios(txtBusqueda.Text);
        }

        // --- SELECCIÓN EN LA TABLA ---
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
                    cmbGradoBaneo.SelectedIndex = (_usuarioSeleccionado.GradoBaneo > 0) ? _usuarioSeleccionado.GradoBaneo - 1 : 0;

                    if (_usuarioSeleccionado.FinBaneo.HasValue)
                        lblFinBaneo.Text = "Fin: " + _usuarioSeleccionado.FinBaneo.Value.ToString("dd/MM HH:mm");
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

        // --- ACCIONES DEL PANEL DERECHO ---
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
            CargarUsuarios(txtBusqueda.Text); // Recargar manteniendo filtro
        }

        private void BtnCambiarPass_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null || string.IsNullOrWhiteSpace(txtNuevaPass.Text)) return;
            DatabaseHelper.AdminCambiarPass(_usuarioSeleccionado.Id, txtNuevaPass.Text, _idAdmin);
            MessageBox.Show($"Contraseña cambiada para {_usuarioSeleccionado.Username}.");
            txtNuevaPass.Clear();
        }

        private void BtnSancionar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;
            if (cmbGradoBaneo.SelectedIndex == -1) { MessageBox.Show("Selecciona un nivel de sanción."); return; }
            if (_usuarioSeleccionado.Rol.Contains("ADMIN")) { MessageBox.Show("No puedes banear a otro admin."); return; }

            int grado = cmbGradoBaneo.SelectedIndex + 1;
            DatabaseHelper.AplicarSancion(_usuarioSeleccionado.Id, grado, _idAdmin);
            MessageBox.Show($"Usuario sancionado (Nivel {grado}).");
            CargarUsuarios(txtBusqueda.Text);
        }

        private void BtnIndultar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;
            DatabaseHelper.LevantarCastigo(_usuarioSeleccionado.Id, _idAdmin);
            MessageBox.Show("Sanción levantada.");
            CargarUsuarios(txtBusqueda.Text);
        }

        private void BtnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AdminCreateUserWindow v = new AdminCreateUserWindow(_idAdmin);
                v.ShowDialog();
                CargarUsuarios(txtBusqueda.Text);
            }
            catch { MessageBox.Show("Ventana 'AdminCreateUserWindow' no encontrada."); }
        }

        private void BtnVerApelaciones_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApelacionesWindow v = new ApelacionesWindow();
                v.ShowDialog();
                CargarUsuarios(txtBusqueda.Text);
            }
            catch { MessageBox.Show("Ventana 'ApelacionesWindow' no encontrada."); }
        }

        private void LimpiarFormulario()
        {
            txtUsername.Clear(); txtEmail.Clear(); txtNuevaPass.Clear();
            cmbRol.SelectedIndex = -1; cmbGradoBaneo.SelectedIndex = -1;
            lblEstadoBaneo.Text = "-"; lblFinBaneo.Text = "-";
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
    }
}