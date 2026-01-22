using System.Windows;
using System.Windows.Controls; // Necesario para Button
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class CustomMessageBox : Window
    {
        // Constructor modificado: añade 'esConfirmacion' al final
        public CustomMessageBox(string titulo, string mensaje, Brush colorTema, bool esConfirmacion = true)
        {
            InitializeComponent();

            lblTitulo.Text = titulo.ToUpper();
            lblMensaje.Text = mensaje;

            // Aplicar el color del tema al borde y al botón principal
            MainBorder.BorderBrush = colorTema;
            btnConfirmar.Background = colorTema;

            // Efecto de sombra neón en el título
            lblTitulo.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = ((SolidColorBrush)colorTema).Color,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.5
            };

            // LÓGICA VISUAL:
            // Si NO es una confirmación (es solo info), cambiamos el texto a "ACEPTAR"
            // e intentamos ocultar el botón cancelar si tiene nombre en el XAML.
            if (!esConfirmacion)
            {
                btnConfirmar.Content = "ACEPTAR";

                // Buscamos el botón cancelar por si le pusiste x:Name="btnCancelar" en el XAML
                // Si no le pusiste nombre, esto simplemente se ignora y no da error.
                var btnCancel = this.FindName("btnCancelar") as Button;
                if (btnCancel != null)
                {
                    btnCancel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Devuelve TRUE
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Devuelve FALSE
            this.Close();
        }
    }
}