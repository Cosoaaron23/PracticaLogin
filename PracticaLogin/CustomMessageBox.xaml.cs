using System.Windows;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class CustomMessageBox : Window
    {
        // Constructor modificado para recibir Título, Mensaje y Color
        public CustomMessageBox(string titulo, string mensaje, Brush colorTema)
        {
            InitializeComponent();

            lblTitulo.Text = titulo.ToUpper();
            lblMensaje.Text = mensaje;

            // Aplicamos el color del tema al borde y al botón de confirmar
            MainBorder.BorderBrush = colorTema;
            btnConfirmar.Background = colorTema;

            // Efecto de sombra del título también del color del tema
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
            this.DialogResult = true; // Esto equivale al "Yes"
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Esto equivale al "No"
            this.Close();
        }
    }
}