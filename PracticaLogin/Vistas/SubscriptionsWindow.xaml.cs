using System.Windows;
using System.Windows.Controls;

namespace PracticaLogin
{
    public partial class SubscriptionsWindow : Window
    {
        // CAMBIO 1: Guardamos el objeto completo
        private Usuario _usuarioActual;

        // CAMBIO 2: Constructor recibe Usuario
        public SubscriptionsWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            CargarSuscripcion();
        }

        private void CargarSuscripcion()
        {
            // Obtenemos la suscripción usando el ID del objeto
            string plan = DatabaseHelper.GetSubscription(_usuarioActual.Id);

            // Buscamos el control para poner el texto
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
            DatabaseHelper.UpdateSubscription(_usuarioActual.Username, plan);
            MessageBox.Show($"¡Te has suscrito al plan {plan}!");
            CargarSuscripcion();
        }

        // --- BOTÓN VOLVER (Aquí estaba el otro error) ---
        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos el objeto completo de vuelta al Home
            HomeWindow home = new HomeWindow(_usuarioActual);
            home.Show();
            this.Close();
        }
    }
}