using System.Windows;
// Asegúrate de que el namespace coincida con el de tu proyecto
namespace PracticaLogin
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        // -----------------------------------------------------------
        // 1. BOTÓN CONFIGURACIÓN (El de tu menú lateral)
        // -----------------------------------------------------------
        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            // Opcional: Bajar la opacidad de la ventana principal para dar efecto de foco
            this.Opacity = 0.5;

            // Crear y mostrar la ventana de configuración
            ConfigWindow config = new ConfigWindow();

            // ShowDialog() detiene la ejecución aquí hasta que se cierre la ventana de configuración
            // Esto evita que el usuario toque el Home mientras configura algo.
            config.ShowDialog();

            // Restaurar la opacidad al volver
            this.Opacity = 1;
        }

        // -----------------------------------------------------------
        // 2. BOTÓN CERRAR SESIÓN (Requisito de la Rúbrica)
        // -----------------------------------------------------------
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Crear una nueva instancia del Login
            MainWindow login = new MainWindow();
            login.Show();

            // Cerrar la ventana actual (Home)
            this.Close();
        }

        // -----------------------------------------------------------
        // 3. BOTÓN SALIR (Si tienes un botón para cerrar la App completamente)
        // -----------------------------------------------------------
        private void BtnSalirApp_Click(object sender, RoutedEventArgs e)
        {
            // Cierra toda la aplicación y mata todos los procesos
            Application.Current.Shutdown();
        }

        // -----------------------------------------------------------
        // 4. OTROS BOTONES (Placeholders para tu menú "Gamer")
        // -----------------------------------------------------------
        private void BtnTienda_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí se abriría la Tienda.", "AKAY Store");
        }

        private void BtnComunidad_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí iría el chat o foro de la comunidad.", "Comunidad");
        }

        private void BtnMisJuegos_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para filtrar o mostrar la grid de juegos
            MessageBox.Show("Mostrando biblioteca del usuario...", "Mis Juegos");
        }
    }
}