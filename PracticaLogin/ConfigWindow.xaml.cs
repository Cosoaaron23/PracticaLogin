using System.Windows;

namespace PracticaLogin
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la configuración y vuelve (o simplemente cierra si se abrió encima)
            this.Close();
        }
    }
}