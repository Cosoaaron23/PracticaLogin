using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class MetodoPagoWindow : Window
    {
        public bool PagoConfirmado { get; private set; } = false;

        public MetodoPagoWindow(decimal total)
        {
            InitializeComponent();
            lblTotal.Text = $"{total:C}";
        }

        private async void BtnPagar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Mostrar capa de "Procesando"
            OverlayProcesando.Visibility = Visibility.Visible;
            PanelPago.IsEnabled = false;

            // 2. Simular espera de red (2.5 segundos)
            await Task.Delay(2500);

            // 3. Confirmar y cerrar
            PagoConfirmado = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}