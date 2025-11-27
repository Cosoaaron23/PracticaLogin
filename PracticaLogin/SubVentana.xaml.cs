using System.Windows;

namespace PracticaLogin
{
    public partial class SubVentana : Window
    {
        // Modificamos el constructor para pedir el nombre de la sección
        public SubVentana(string nombreSeccion)
        {
            InitializeComponent();

            // Cambiamos el texto del título según lo que recibimos
            lblTitulo.Text = nombreSeccion.ToUpper();
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}