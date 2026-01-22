using System;
using System.Windows;
using System.Windows.Controls; // Para ComboBoxItem
using System.Windows.Input;
    // Para DatabaseHelper

namespace PracticaLogin
{
    public partial class AdminCreateUserWindow : Window
    {
        private int _idAdmin;

        public AdminCreateUserWindow(int idAdmin)
        {
            InitializeComponent();
            _idAdmin = idAdmin;
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text;
            string pass = txtPass.Text;
            string email = txtEmail.Text;

            // Obtenemos el texto del ComboBox
            string rol = "USER";
            if (cmbRol.SelectedItem is ComboBoxItem item)
                rol = item.Content.ToString();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Usuario y contraseña son obligatorios.");
                return;
            }

            try
            {
                // Llamamos al Helper pasando el ID del Admin para el LOG
                DatabaseHelper.AdminCrearUsuario(user, pass, email, rol, _idAdmin);

                MessageBox.Show("Usuario creado correctamente.", "Éxito");

                // Esto cierra la ventana y devuelve 'true' a quien la llamó
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear (¿Quizás el usuario ya existe?):\n" + ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();

        // Mover ventana
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); }
    }
}