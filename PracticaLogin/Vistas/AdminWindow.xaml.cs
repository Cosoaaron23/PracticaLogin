using System.Windows;
using System.Windows.Input;

namespace PracticaLogin
{
    public partial class AdminWindow : Window
    {
        private Usuario _adminUser;

        // Constructor: Recibe el usuario completo para leer su ROL
        public AdminWindow(Usuario adminUser)
        {
            InitializeComponent();
            _adminUser = adminUser;

            // IMPORTANTE: Llamamos a la función que filtra los botones
            ConfigurarPermisos();
        }

        private void ConfigurarPermisos()
        {
            // 1. PRIMERO OCULTAMOS TODO (Reset)
            btnJuegos.Visibility = Visibility.Collapsed;
            btnUsuarios.Visibility = Visibility.Collapsed;
            btnSoporte.Visibility = Visibility.Collapsed;

            // El botón de reportes lo dejamos visible para todos (opcional)
            btnReportes.Visibility = Visibility.Visible;

            // 2. MOSTRAMOS SOLO LO QUE PERMITE EL ROL
            // Usamos .ToUpper() para evitar errores de mayúsculas/minúsculas
            string rol = _adminUser.Rol.ToUpper();

            switch (rol)
            {
                case "SUPERADMIN":
                    // El jefe lo ve todo
                    btnJuegos.Visibility = Visibility.Visible;
                    btnUsuarios.Visibility = Visibility.Visible;
                    btnSoporte.Visibility = Visibility.Visible;
                    break;

                case "GAME_ADMIN":
                    // Solo juegos
                    btnJuegos.Visibility = Visibility.Visible;
                    break;

                case "USER_ADMIN":
                    // Solo usuarios y apelaciones
                    btnUsuarios.Visibility = Visibility.Visible;
                    break;

                case "SUPPORT_ADMIN":
                    // Solo soporte técnico
                    btnSoporte.Visibility = Visibility.Visible;
                    break;

                default:
                    MessageBox.Show("Tu rol de administrador no tiene permisos asignados.");
                    break;
            }
        }

        // --- NAVEGACIÓN ---

        private void BtnJuegos_Click(object sender, RoutedEventArgs e)
        {
            new AdminJuegosWindow().ShowDialog();
        }

        private void BtnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos el ID del admin para los logs
            new AdminUsuariosWindow(_adminUser.Id).ShowDialog();
        }

        private void BtnSoporte_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos el ID del admin para los logs
            new AdminSoporteWindow(_adminUser.Id).ShowDialog();
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            // Mostramos el reporte general
            new ReporteWindow().ShowDialog(); // Si tienes una ventana de reportes
            // O si usas MessageBox simple:
            // MessageBox.Show(DatabaseHelper.ObtenerReporteLogs());
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}