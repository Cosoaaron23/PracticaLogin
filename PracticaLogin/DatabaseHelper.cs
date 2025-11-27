using System;
using System.Data;
using MySql.Data.MySqlClient; // Usamos la nueva librería

namespace PracticaLogin
{
    public class DatabaseHelper
    {
        // CONFIGURA AQUÍ TUS DATOS DE MYSQL
        // Server usually is "localhost" or "127.0.0.1"
        // Uid usually is "root"
        // Pwd is YOUR MySQL password (pon tu contraseña real aquí)
        private const string ConnectionString = "server=localhost;database=PracticaLogin;uid=root;pwd=1234;";

        // Probamos la conexión al iniciar (Opcional, pero útil para ver si configuraste bien la IP/Pass)
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    // Si no lanza error, es que conectó bien a MySQL
                }
            }
            catch (Exception ex)
            {
                // Si falla, lanzamos un error para que te des cuenta rápido
                throw new Exception("Error al conectar con MySQL: " + ex.Message);
            }
        }

        // Método para Registrar usuario
        public static bool RegisterUser(string user, string pass)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    // 1. Verificar si existe
                    string checkSql = "SELECT COUNT(*) FROM Usuarios WHERE Username = @u";
                    using (var checkCmd = new MySqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@u", user);
                        long count = (long)checkCmd.ExecuteScalar(); // MySQL devuelve long
                        if (count > 0) return false; // Ya existe
                    }

                    // 2. Insertar
                    string insertSql = "INSERT INTO Usuarios (Username, Password) VALUES (@u, @p)";
                    using (var insertCmd = new MySqlCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@u", user);
                        insertCmd.Parameters.AddWithValue("@p", pass);
                        insertCmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Método para Validar Login
        public static bool ValidateUser(string user, string pass)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM Usuarios WHERE Username = @u AND Password = @p";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@u", user);
                        command.Parameters.AddWithValue("@p", pass);
                        long count = (long)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}