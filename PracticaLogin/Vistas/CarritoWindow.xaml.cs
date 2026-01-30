using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class CarritoWindow : Window
    {
        public bool CompraRealizada { get; private set; } = false;
        private Usuario _usuarioActual;

        public CarritoWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Refrescamos la lista visualmente
            lstProductos.ItemsSource = null;
            lstProductos.ItemsSource = CarritoService.Cesta;

            // Calculamos el total
            decimal total = 0;
            foreach (var item in CarritoService.Cesta) total += item.Precio;

            if (lblTotal != null) lblTotal.Text = total.ToString("C");
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

            MetodoPagoWindow stripe = new MetodoPagoWindow(total);
            stripe.ShowDialog();

            if (stripe.PagoConfirmado)
            {
                foreach (var juego in CarritoService.Cesta)
                {
                    if (!DatabaseHelper.UsuarioTieneJuego(_usuarioActual.Id, juego.Id))
                    {
                        DatabaseHelper.ComprarJuego(_usuarioActual.Id, juego.Id);
                    }
                }
                CarritoService.Cesta.Clear();

                // Éxito con CustomMessageBox
                Brush colorCian = (Brush)new BrushConverter().ConvertFrom("#00E5FF");
                CustomMessageBox exito = new CustomMessageBox("Compra Exitosa", "Los juegos se han añadido a tu biblioteca.", colorCian, false);
                exito.ShowDialog();

                this.CompraRealizada = true;
                this.Close();
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}