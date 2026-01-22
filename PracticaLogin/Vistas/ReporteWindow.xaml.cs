using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class ReporteWindow : Window
    {
        public ReporteWindow()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Llamamos a tu Helper que ya funciona perfecto y pegamos el texto
            string reporte = DatabaseHelper.ObtenerReporteLogs();
            txtLogContent.Text = reporte;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Para poder mover la ventana aunque no tenga bordes
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}