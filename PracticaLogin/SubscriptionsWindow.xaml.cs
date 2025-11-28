using System.Windows;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class SubscriptionsWindow : Window
    {
        private string usuarioActual;

        public SubscriptionsWindow(string usuario)
        {
            InitializeComponent();
            this.usuarioActual = usuario;

            ActualizarInterfaz();
        }

        private void ActualizarInterfaz()
        {
            string plan = DatabaseHelper.GetSubscription(usuarioActual);

            // Resetear todos los botones
            ResetButton(BtnCielo, "SELECCIONAR", Brushes.Transparent, new SolidColorBrush(Color.FromRgb(79, 195, 247)));
            ResetButton(BtnLimbo, "SELECCIONAR", new SolidColorBrush(Color.FromRgb(255, 152, 0)), Brushes.Black);
            ResetButton(BtnInfierno, "CONSEGUIR PODER", new SolidColorBrush(Color.FromRgb(211, 47, 47)), Brushes.White);

            // Marcar el botón del plan actual
            switch (plan)
            {
                case "Cielo": MarcarComoActual(BtnCielo); break;
                case "Limbo": MarcarComoActual(BtnLimbo); break;
                case "Infierno": MarcarComoActual(BtnInfierno); break;
            }
        }

        private void ResetButton(System.Windows.Controls.Button btn, string text, Brush bg, Brush fg)
        {
            btn.Content = text;
            btn.IsEnabled = true;
            if (bg != Brushes.Transparent) { btn.Background = bg; btn.Foreground = fg; }
            btn.Opacity = 1;
        }

        private void MarcarComoActual(System.Windows.Controls.Button btn)
        {
            btn.Content = "SELECCIONADA";
            btn.IsEnabled = false;
            btn.Opacity = 1;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- COMPRAS ---
        private void BtnCielo_Click(object sender, RoutedEventArgs e)
        {
            var colorAzul = new SolidColorBrush(Color.FromRgb(79, 195, 247));
            CustomMessageBox msg = new CustomMessageBox("Cambiar Plan", "¿Quieres volver al plan GRATUITO (Cielo)?", colorAzul);
            if (msg.ShowDialog() == true)
            {
                DatabaseHelper.UpdateSubscription(usuarioActual, "Cielo");
                ActualizarInterfaz();
            }
        }

        private void BtnLimbo_Click(object sender, RoutedEventArgs e)
        {
            var colorNaranja = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            CustomMessageBox msg = new CustomMessageBox("Confirmar Suscripción", "¿Suscribirse al plan LIMBO por 4.99€/mes?", colorNaranja);

            if (msg.ShowDialog() == true)
            {
                DatabaseHelper.UpdateSubscription(usuarioActual, "Limbo");
                CustomMessageBox exito = new CustomMessageBox("¡Bienvenido!", "Disfruta del Limbo.", colorNaranja);
                exito.btnConfirmar.Content = "ACEPTAR"; exito.ShowDialog();
                ActualizarInterfaz();
            }
        }

        private void BtnInfierno_Click(object sender, RoutedEventArgs e)
        {
            var colorRojo = new SolidColorBrush(Color.FromRgb(211, 47, 47));
            CustomMessageBox msg = new CustomMessageBox("Poder Absoluto", "¿Te atreves con EL INFIERNO por 14.99€/mes?", colorRojo);

            if (msg.ShowDialog() == true)
            {
                DatabaseHelper.UpdateSubscription(usuarioActual, "Infierno");
                CustomMessageBox exito = new CustomMessageBox("¡Poder Desatado!", "Ahora eres PREMIUM.", colorRojo);
                exito.btnConfirmar.Content = "ACEPTAR"; exito.ShowDialog();
                ActualizarInterfaz();
            }
        }
    }
}