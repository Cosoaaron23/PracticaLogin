using System;
using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class AdminSoporteWindow : Window
    {
        private int _idAdminActual; // Variable para guardar quién está operando

        // CONSTRUCTOR MODIFICADO: Ahora pide el ID del admin
        public AdminSoporteWindow(int idAdmin)
        {
            InitializeComponent();
            _idAdminActual = idAdmin;
            CargarTickets();
        }

        private void CargarTickets()
        {
            try
            {
                dgTickets.ItemsSource = DatabaseHelper.ObtenerTodosLosTickets();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar tickets: " + ex.Message);
            }
        }

        private void BtnResolver_Click(object sender, RoutedEventArgs e)
        {
            if (dgTickets.SelectedItem is TicketSoporte ticket)
            {
                if (ticket.Estado == "RESUELTO")
                {
                    MessageBox.Show("Este ticket ya está resuelto.");
                    return;
                }

                try
                {
                    // 1. Actualizar estado en BD
                    DatabaseHelper.CerrarTicket(ticket.Id);

                    // 2. Registrar Log (AHORA USAMOS EL ID REAL, NO 0)
                    DatabaseHelper.RegistrarLog(_idAdminActual, "SOPORTE", $"Ticket #{ticket.Id} resuelto");

                    MessageBox.Show($"Ticket #{ticket.Id} cerrado correctamente.");
                    CargarTickets(); // Refrescar tabla
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al resolver ticket: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Selecciona un ticket de la lista primero.");
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}