using System;
using System.Windows;
using System.Windows.Controls; // Necesario para ComboBoxItem
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class SoporteWindow : Window
    {
        private Usuario _usuarioActual;

        // AHORA RECIBIMOS EL USUARIO EN EL CONSTRUCTOR
        public SoporteWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones
            if (cmbMotivo.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona un motivo.", "Faltan datos");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMensaje.Text))
            {
                MessageBox.Show("Por favor, describe tu problema.", "Faltan datos");
                return;
            }

            try
            {
                // 2. Obtener datos del formulario
                string motivo = (cmbMotivo.SelectedItem as ComboBoxItem).Content.ToString();
                string mensaje = txtMensaje.Text;

                // 3. Guardar en Base de Datos (REAL)
                DatabaseHelper.EnviarTicketSoporte(_usuarioActual.Id, motivo, mensaje);

                // 4. Confirmación
                string ticketId = "#" + new Random().Next(1000, 9999);
                MessageBox.Show($"Ticket {ticketId} guardado correctamente en la base de datos.\n\nUn administrador revisará tu caso pronto.", "Enviado con Éxito");

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el servidor: " + ex.Message);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
    }
}