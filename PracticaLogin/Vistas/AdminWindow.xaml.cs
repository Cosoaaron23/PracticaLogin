using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; // NECESARIO PARA LOS COLORES (Brushes)

namespace PracticaLogin
{
    public partial class AdminWindow : Window
    {
        private int _idAdmin;
        private List<Usuario> _usuarios;

        public AdminWindow(int id)
        {
            InitializeComponent();
            _idAdmin = id;
        }

        // --- CARGA Y EVENTOS DE VENTANA ---
        private void Window_Loaded(object sender, RoutedEventArgs e) => Cargar();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        // 1. CARGAR DATOS
        void Cargar(string filtro = "")
        {
            _usuarios = string.IsNullOrEmpty(filtro) ? DatabaseHelper.ObtenerUsuarios() : DatabaseHelper.BuscarUsuarios(filtro);
            dgUsuarios.ItemsSource = _usuarios;

            // Resetear UI
            lblSelectedUser.Text = "Ninguno";
            txtNuevaPass.Password = "";

            // Desactivar botones de acción hasta seleccionar usuario
            if (btnAplicarSancion != null) btnAplicarSancion.IsEnabled = false;
            if (btnEliminarUser != null) btnEliminarUser.IsEnabled = false;
            if (btnLevantarCastigo != null) btnLevantarCastigo.Visibility = Visibility.Collapsed;
        }

        // 2. SELECCIÓN EN TABLA
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                lblSelectedUser.Text = u.Username.ToUpper();

                btnAplicarSancion.IsEnabled = true;
                btnEliminarUser.IsEnabled = true;

                if (u.Activo)
                {
                    btnAplicarSancion.Content = "APLICAR SANCIÓN";
                    btnAplicarSancion.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333"));
                    btnAplicarSancion.Foreground = Brushes.Gray;
                    btnLevantarCastigo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnAplicarSancion.Content = "ACTUALIZAR";
                    btnAplicarSancion.Background = Brushes.Orange;
                    btnAplicarSancion.Foreground = Brushes.Black;
                    btnLevantarCastigo.Visibility = Visibility.Visible;
                }
            }
        }

        // 3. BOTONES DE ACCIÓN (CON CUSTOM MESSAGE BOX)

        // --- APLICAR SANCIÓN ---
        private void BtnAplicarSancion_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                if (u.Id == _idAdmin)
                {
                    new CustomMessageBox("ERROR", "No puedes sancionarte a ti mismo.", Brushes.Crimson, false).ShowDialog();
                    return;
                }

                int grado = cmbGravedad.SelectedIndex + 1;
                DatabaseHelper.AplicarSancion(u.Id, grado, _idAdmin);
                Cargar(txtBusqueda.Text);
            }
        }

        // --- ELIMINAR USUARIO (ROJO) ---
        private void BtnEliminarUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                if (u.Id == _idAdmin)
                {
                    new CustomMessageBox("ERROR", "No puedes eliminarte a ti mismo.", Brushes.Crimson, false).ShowDialog();
                    return;
                }

                // Usamos CustomMessageBox en modo CONFIRMACIÓN (Rojo)
                var ventana = new CustomMessageBox("ELIMINAR USUARIO",
                                                   $"¿Estás seguro de eliminar permanentemente a {u.Username}?",
                                                   Brushes.Crimson,
                                                   true);

                if (ventana.ShowDialog() == true)
                {
                    DatabaseHelper.EliminarUsuarioTotal(u.Id, _idAdmin);
                    Cargar(txtBusqueda.Text);
                }
            }
        }

        // --- LEVANTAR CASTIGO (VERDE) ---
        private void BtnLevantarCastigo_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                DatabaseHelper.LevantarCastigo(u.Id, _idAdmin);
                Cargar(txtBusqueda.Text);
                new CustomMessageBox("INDULTO", $"Se ha levantado el castigo a {u.Username}.", Brushes.LimeGreen, false).ShowDialog();
            }
        }

        // --- CAMBIAR CONTRASEÑA (CYAN) ---
        private void BtnCambiarPass_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario u)
            {
                if (!string.IsNullOrWhiteSpace(txtNuevaPass.Password))
                {
                    DatabaseHelper.AdminCambiarPass(u.Id, txtNuevaPass.Password, _idAdmin);
                    u.Password = txtNuevaPass.Password;
                    dgUsuarios.Items.Refresh();

                    // Mensaje informativo (false al final para modo info)
                    new CustomMessageBox("CONTRASEÑA", "Contraseña actualizada correctamente.", Brushes.Cyan, false).ShowDialog();

                    txtNuevaPass.Password = "";
                }
                else
                {
                    new CustomMessageBox("AVISO", "Escribe una contraseña primero.", Brushes.Orange, false).ShowDialog();
                }
            }
        }

        // --- GUARDAR CAMBIOS (VERDE) ---
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarios != null)
            {
                foreach (var u in _usuarios)
                {
                    DatabaseHelper.ActualizarUsuario(u.Id, u.Email, u.Rol);
                }
                new CustomMessageBox("GUARDADO", "Base de datos actualizada correctamente.", Brushes.LimeGreen, false).ShowDialog();
            }
        }

        // 4. BOTONES DE MENÚ Y OTROS

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e) => Cargar(txtBusqueda.Text);

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e) => Cargar(txtBusqueda.Text);

        private void BtnCrearUser_Click(object sender, RoutedEventArgs e)
        {
            AdminCreateUserWindow createUserWindow = new AdminCreateUserWindow(_idAdmin);
            if (createUserWindow.ShowDialog() == true) Cargar();
        }

        private void BtnApelaciones_Click(object sender, RoutedEventArgs e) => new ApelacionesWindow().ShowDialog();

        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            // Abre tu ventana de reporte PRO
            ReporteWindow ventanaReporte = new ReporteWindow();
            ventanaReporte.ShowDialog();
        }
    }
}