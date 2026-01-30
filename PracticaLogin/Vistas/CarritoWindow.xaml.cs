using System.Windows;
using System.Windows.Controls;

namespace PracticaLogin
{
    public partial class CarritoWindow : Window
    {
        public bool CompraRealizada { get; private set; } = false;

        public CarritoWindow()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            lstProductos.ItemsSource = null;
            lstProductos.ItemsSource = CarritoService.Cesta;
            lblTotal.Text = CarritoService.Total().ToString("C");
        }

       

        private void BtnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Juego juego)
            {
                CarritoService.Remover(juego);
                CargarDatos();
            }
        }



        private void BtnPagar_Click(object sender, RoutedEventArgs e)
        {
            if (CarritoService.Cesta.Count == 0) return;

            decimal total = 0;
            foreach (var j in CarritoService.Cesta) total += j.Precio;

            // Abrir pasarela Stripe
            MetodoPagoWindow stripe = new MetodoPagoWindow(total);
            stripe.ShowDialog();

            if (stripe.PagoConfirmado)
            {
                this.CompraRealizada = true;
                this.Close();
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}