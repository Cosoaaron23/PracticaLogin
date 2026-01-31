using System;
using System.Windows;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class EnviarApelacionWindow : Window
    {
        private string _username;

        public EnviarApelacionWindow(string username)
        {
            InitializeComponent();
            _username = username;
            txtUsuario.Text = _username; // Mostramos el usuario que intenta apelar
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMensaje.Text))
            {
                MessageBox.Show("Por favor, escribe un motivo.", "Error");
                return;
            }

            try
            {
                // Llamamos al método que ya existe en tu DatabaseHelper
                DatabaseHelper.EnviarApelacion(_username, txtMensaje.Text);

                // Usamos tu CustomMessageBox para confirmar
                new CustomMessageBox("Enviado", "Tu solicitud ha sido enviada a los administradores.", Brushes.LimeGreen, false).ShowDialog();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar: " + ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}