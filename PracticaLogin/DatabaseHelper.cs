using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;

namespace PracticaLogin
{
    public static class DatabaseHelper
    {
        // Cadena de conexión (Asegúrate que XAMPP esté corriendo)
        private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=1234;";

        // --- 1. INICIALIZAR BASE DE DATOS ---
        // Soluciona el error: 'DatabaseHelper' no contiene una definición para 'InitializeDatabase'
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Crea la tabla si no existe, asegurando que todos los campos necesarios estén ahí
                    string query = @"
                        CREATE TABLE IF NOT EXISTS usuarios (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            nombre VARCHAR(100),
                            apellidos VARCHAR(100),
                            username VARCHAR(50) UNIQUE,
                            password VARCHAR(255),
                            email VARCHAR(100),
                            telefono VARCHAR(20),
                            cp VARCHAR(10),
                            intentos_fallidos INT DEFAULT 0,
                            suscripcion VARCHAR(20) DEFAULT 'FREE',
                            bloqueado_hasta DATETIME NULL
                        );";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar la BD: " + ex.Message);
            }
        }

        // --- TEST CONEXIÓN ---
        public static bool TestConnection()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con MySQL: " + ex.Message);
                return false;
            }
        }

        // --- REGISTRO DE USUARIO ---
        public static bool RegisterUser(string nombre, string apellidos, string username, string password, string email, string telefono, string cp)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO usuarios (nombre, apellidos, username, password, email, telefono, cp, intentos_fallidos, suscripcion) " +
                                   "VALUES (@nom, @ape, @user, @pass, @mail, @tlf, @cp, 0, 'FREE')";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nom", nombre);
                        cmd.Parameters.AddWithValue("@ape", apellidos);
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.Parameters.AddWithValue("@mail", email);
                        cmd.Parameters.AddWithValue("@tlf", telefono);
                        cmd.Parameters.AddWithValue("@cp", cp);

                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en registro: " + ex.Message);
                return false;
            }
        }

        // --- LOGIN (VALIDAR USUARIO) ---
        public static bool ValidateUser(string username, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT password, intentos_fallidos, bloqueado_hasta FROM usuarios WHERE username = @user";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbPass = reader["password"].ToString();
                                int intentos = Convert.ToInt32(reader["intentos_fallidos"]);
                                var bloqueadoHastaVal = reader["bloqueado_hasta"];

                                // Verificar bloqueo
                                if (bloqueadoHastaVal != DBNull.Value)
                                {
                                    DateTime bloqueadoHasta = Convert.ToDateTime(bloqueadoHastaVal);
                                    if (DateTime.Now < bloqueadoHasta)
                                    {
                                        MessageBox.Show($"Cuenta bloqueada hasta las {bloqueadoHasta:HH:mm:ss}");
                                        return false;
                                    }
                                }

                                reader.Close();

                                // Verificar contraseña
                                if (dbPass == password)
                                {
                                    ResetIntentos(username);
                                    return true;
                                }
                                else
                                {
                                    AumentarIntentos(username, intentos);
                                    return false;
                                }
                            }
                            else
                            {
                                return false; // Usuario no existe
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de Base de Datos: " + ex.Message);
                return false;
            }
        }

        // --- MÉTODOS DE SOPORTE PARA LOGIN ---
        private static void ResetIntentos(string username)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE usuarios SET intentos_fallidos = 0, bloqueado_hasta = NULL WHERE username = @user";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void AumentarIntentos(string username, int intentosActuales)
        {
            int nuevosIntentos = intentosActuales + 1;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                if (nuevosIntentos >= 3)
                {
                    string query = "UPDATE usuarios SET intentos_fallidos = @intentos, bloqueado_hasta = @bloqueo WHERE username = @user";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@intentos", nuevosIntentos);
                        cmd.Parameters.AddWithValue("@bloqueo", DateTime.Now.AddMinutes(5));
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("¡Contraseña incorrecta 3 veces! Bloqueado por 5 minutos.");
                }
                else
                {
                    string query = "UPDATE usuarios SET intentos_fallidos = @intentos WHERE username = @user";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@intentos", nuevosIntentos);
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show($"Contraseña incorrecta. Intento {nuevosIntentos} de 3.");
                }
            }
        }

        // --- NUEVO: OBTENER ID (Requerido para ConfigWindow) ---
        // Soluciona error: 'DatabaseHelper' no contiene definición para 'GetUserId'
        public static int GetUserId(string username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id FROM usuarios WHERE username = @user";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // --- NUEVO: OBTENER SUSCRIPCIÓN POR ID ---
        // Soluciona error: 'DatabaseHelper' no contiene definición para 'GetSubscription'
        public static string GetSubscription(int userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT suscripcion FROM usuarios WHERE id = @id";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "FREE";
                }
            }
        }

        // (Mantenemos el método antiguo por si acaso lo usas en otro sitio por nombre)
        public static string GetUserSubscription(string username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT suscripcion FROM usuarios WHERE username = @user";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "FREE";
                }
            }
        }

        // --- NUEVO: ACTUALIZAR PASSWORD ---
        // Soluciona error: 'DatabaseHelper' no contiene definición para 'UpdatePassword'
        public static void UpdatePassword(int userId, string newPassword)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE usuarios SET password = @pass WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@pass", newPassword);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Contraseña actualizada correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar contraseña: " + ex.Message);
            }
        }

        // --- ACTUALIZAR SUSCRIPCIÓN ---
        public static void UpdateSubscription(string username, string nuevaSuscripcion)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE usuarios SET suscripcion = @sub WHERE username = @user";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@sub", nuevaSuscripcion);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}