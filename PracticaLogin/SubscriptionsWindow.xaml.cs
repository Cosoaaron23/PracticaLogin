using System.Windows;
using System.Windows.Media;

namespace PracticaLogin
{
    public partial class SubscriptionsWindow : Window
    {
        public SubscriptionsWindow()
        {
            InitializeComponent();
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- PLAN LIMBO ---
        private void BtnLimbo_Click(object sender, RoutedEventArgs e)
        {
            // Color Naranja
            var colorNaranja = new SolidColorBrush(Color.FromRgb(255, 152, 0));

            CustomMessageBox msg = new CustomMessageBox(
                "Confirmar Suscripción",
                "¿Quieres suscribirte al plan LIMBO por 4.99€/mes?",
                colorNaranja);

            if (msg.ShowDialog() == true)
            {
                CustomMessageBox exito = new CustomMessageBox("¡Bienvenido!", "Disfruta del Limbo sin anuncios.", colorNaranja);
                exito.btnConfirmar.Content = "ACEPTAR";
                exito.ShowDialog();
                this.Close();
            }
        }

        // --- PLAN INFIERNO ---
        private void BtnInfierno_Click(object sender, RoutedEventArgs e)
        {
            // Color Rojo Sangre
            var colorRojo = new SolidColorBrush(Color.FromRgb(211, 47, 47));

            CustomMessageBox msg = new CustomMessageBox(
                "Poder Absoluto",
                "¿Te atreves con EL INFIERNO por 14.99€/mes?\nObtendrás acceso total y skins exclusivas.",
                colorRojo);

            if (msg.ShowDialog() == true)
            {
                CustomMessageBox exito = new CustomMessageBox("¡Poder Desatado!", "Tu cuenta ahora es PREMIUM.", colorRojo);
                exito.btnConfirmar.Content = "ACEPTAR";
                exito.ShowDialog();
                this.Close();
            }
        }
    }
}