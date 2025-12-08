using System.Windows;
using System.Windows.Controls; // Necesario para TextBlock

namespace PracticaLogin
{
    public partial class SubscriptionsWindow : Window
    {
        private string currentUsername;
        private int currentUserId;

        public SubscriptionsWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            CargarSuscripcion();
        }

        // --- MÉTODO PARA MOSTRAR LA SUSCRIPCIÓN ACTUAL ---
        private void CargarSuscripcion()
        {
            // 1. Obtener ID y Plan actual
            currentUserId = DatabaseHelper.GetUserId(currentUsername);
            string plan = DatabaseHelper.GetSubscription(currentUserId);

            // Intentamos buscar el TextBlock por si acaso tiene un nombre diferente
            // Si en tu XAML el texto donde sale el plan se llama "lblPlanActual", esto funcionará.
            if (this.FindName("lblPlanActual") is TextBlock label)
            {
                label.Text = "PLAN ACTUAL: " + plan.ToUpper();
            }
            // Si usaste un Label en vez de TextBlock:
            else if (this.FindName("lblPlanActual") is Label labelControl)
            {
                labelControl.Content = "PLAN ACTUAL: " + plan.ToUpper();
            }
        }

        // --- BOTÓN PLAN CIELO (Error corregido) ---
        private void BtnCielo_Click(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.UpdateSubscription(currentUsername, "CIELO");
            MessageBox.Show("¡Te has suscrito al plan CIELO!");
            CargarSuscripcion(); // Actualiza el texto en pantalla
        }

        // --- BOTÓN PLAN LIMBO (Error corregido) ---
        private void BtnLimbo_Click(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.UpdateSubscription(currentUsername, "LIMBO");
            MessageBox.Show("¡Te has suscrito al plan LIMBO!");
            CargarSuscripcion();
        }

        // --- BOTÓN PLAN INFIERNO (Error corregido) ---
        private void BtnInfierno_Click(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.UpdateSubscription(currentUsername, "INFIERNO");
            MessageBox.Show("¡Te has suscrito al plan INFIERNO!");
            CargarSuscripcion();
        }

        // --- BOTÓN VOLVER ---
        // Asegúrate de que en tu XAML el botón de volver tenga Click="BtnVolver_Click"
        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            HomeWindow home = new HomeWindow(currentUsername);
            home.Show();
            this.Close();
        }
    }
}