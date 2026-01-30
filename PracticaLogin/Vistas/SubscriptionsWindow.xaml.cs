using System.Windows;
using System.Windows.Controls;

namespace PracticaLogin
{
    public partial class SubscriptionsWindow : Window
    {
        private Usuario _usuarioActual;

        public SubscriptionsWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            CargarSuscripcion();
        }

        private void CargarSuscripcion()
        {
            // Usamos el ID del usuario para obtener su plan
            string plan = DatabaseHelper.GetSubscription(_usuarioActual.Id);

            // Buscamos el control para poner el texto (soporta tanto Label como TextBlock)
            if (this.FindName("lblPlanActual") is TextBlock label)
                label.Text = "PLAN ACTUAL: " + plan.ToUpper();
            else if (this.FindName("lblPlanActual") is Label labelControl)
                labelControl.Content = "PLAN ACTUAL: " + plan.ToUpper();
        }

        // Botones de planes
        private void BtnCielo_Click(object sender, RoutedEventArgs e) { ActualizarPlan("CIELO"); }
        private void BtnLimbo_Click(object sender, RoutedEventArgs e) { ActualizarPlan("LIMBO"); }
        private void BtnInfierno_Click(object sender, RoutedEventArgs e) { ActualizarPlan("INFIERNO"); }

        private void ActualizarPlan(string plan)
        {
            // CORRECCIÓN AQUÍ: Pasamos '_usuarioActual.Id' (int) en lugar de '.Username'
            DatabaseHelper.UpdateSubscription(_usuarioActual.Id, plan);

            MessageBox.Show($"¡Te has suscrito al plan {plan}!");
            CargarSuscripcion();
        }

        // --- BOTÓN VOLVER CORREGIDO ---
        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Solo cerramos esta ventana, porque la HomeWindow original sigue abierta detrás
            this.Close();
        }
    }
}