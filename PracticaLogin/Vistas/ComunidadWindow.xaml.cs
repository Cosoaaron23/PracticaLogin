using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class ComunidadWindow : Window
    {
        private Usuario _usuarioActual;

        // CORRECCIÓN: El constructor ahora acepta un Usuario
        public ComunidadWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }

        private void BtnSocial_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Usuario {_usuarioActual.Username} abriendo redes sociales...", "Redes Sociales");
            // Process.Start(new ProcessStartInfo("https://discord.com") { UseShellExecute = true });
        }
    }
}