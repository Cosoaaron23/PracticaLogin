using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class EnviarApelacionWindow : Window
    {
        public EnviarApelacionWindow(string usernameCargado)
        {
            InitializeComponent();
            txtUsuario.Text = usernameCargado; // Ya viene relleno con el usuario que intentó entrar
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMensaje.Text))
            {
                MessageBox.Show("Escribe un mensaje explicando por qué deberían desbanearte.");
                return;
            }

            // Enviamos a la BD
            DatabaseHelper.EnviarApelacion(txtUsuario.Text, txtMensaje.Text);

            new CustomMessageBox("ENVIADO", "Tu solicitud ha sido enviada al administrador.", System.Windows.Media.Brushes.Green).ShowDialog();
            this.Close();
        }
    }
}