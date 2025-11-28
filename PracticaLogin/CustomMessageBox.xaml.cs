using System.Windows;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class CustomMessageBox : Window
    {
        // Constructor especial que recibe los datos y el Color
        public CustomMessageBox(string titulo, string mensaje, Brush colorTema)
        {
            InitializeComponent();

            lblTitulo.Text = titulo.ToUpper();
            lblMensaje.Text = mensaje;

            MainBorder.BorderBrush = colorTema;
            btnConfirmar.Background = colorTema;

            lblTitulo.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = ((SolidColorBrush)colorTema).Color,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.5
            };
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}