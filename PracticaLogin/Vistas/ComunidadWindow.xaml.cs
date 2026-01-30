using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class ComunidadWindow : Window
    {
        public ComunidadWindow()
        {
            InitializeComponent();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }

        private void BtnSocial_Click(object sender, RoutedEventArgs e)
        {
            // Simulación de abrir navegador
            MessageBox.Show("Abriendo enlace externo en el navegador...", "Redes Sociales");
            // Process.Start(new ProcessStartInfo("https://discord.com") { UseShellExecute = true }); // Código real
        }
    }
}