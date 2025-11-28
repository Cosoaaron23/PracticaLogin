using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace PracticaLogin
{
    public class DatabaseHelper
    {
        // -----------------------------------------------------------
        // CONFIGURACIÓN (Pon tu contraseña real aquí)
        // -----------------------------------------------------------
        private const string ConnectionString = "server=localhost;database=PracticaLogin;uid=root;pwd=1234;";

        // 1. Inicializar
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar con MySQL: " + ex.Message);
            }
        }

        // 2. Registrar
        public static bool RegisterUser(string nombre, string apellidos, string user, string pass, string email, string tlf, string cp)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string checkSql = "SELECT COUNT(*) FROM Usuarios WHERE Username = @u";
                    using (var checkCmd = new MySqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@u", user);
                        if ((long)checkCmd.ExecuteScalar() > 0) return false;
                    }

                    // Insertamos con Suscripcion = 'Cielo' (Gratis) por defecto
                    string insertSql = "INSERT INTO Usuarios (Nombre, Apellidos, Username, Password, Email, Telefono, CodigoPostal, Intentos, BloqueadoHasta, Suscripcion) " +
                                       "VALUES (@nom, @ape, @u, @p, @mail, @tlf, @cp, 0, NULL, 'Cielo')";

                    using (var insertCmd = new MySqlCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@nom", nombre);
                        insertCmd.Parameters.AddWithValue("@ape", apellidos);
                        insertCmd.Parameters.AddWithValue("@u", user);
                        insertCmd.Parameters.AddWithValue("@p", pass);
                        insertCmd.Parameters.AddWithValue("@mail", email);
                        insertCmd.Parameters.AddWithValue("@tlf", tlf);
                        insertCmd.Parameters.AddWithValue("@cp", cp);
                        insertCmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch { return false; }
        }

        // 3. Login
        public static string ValidateUser(string user, string pass)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT Id, Password, Intentos, BloqueadoHasta FROM Usuarios WHERE Username = @u";
                    int intentos = 0;
                    DateTime? bloqueadoHasta = null;
                    string passwordReal = "";

                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", user);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return "NO_USER";
                            passwordReal = reader["Password"].ToString();
                            intentos = Convert.ToInt32(reader["Intentos"]);
                            if (reader["BloqueadoHasta"] != DBNull.Value) bloqueadoHasta = Convert.ToDateTime(reader["BloqueadoHasta"]);
                        }
                    }

                    if (bloqueadoHasta != null && bloqueadoHasta > DateTime.Now)
                    {
                        return $"LOCKED|{Math.Ceiling((bloqueadoHasta.Value - DateTime.Now).TotalMinutes)}";
                    }

                    if (pass == passwordReal)
                    {
                        string resetSql = "UPDATE Usuarios SET Intentos = 0, BloqueadoHasta = NULL WHERE Username = @u";
                        using (var updateCmd = new MySqlCommand(resetSql, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@u", user);
                            updateCmd.ExecuteNonQuery();
                        }
                        return "OK";
                    }
                    else
                    {
                        intentos++;
                        string updateSql = (intentos >= 3) ?
                            "UPDATE Usuarios SET Intentos = @i, BloqueadoHasta = DATE_ADD(NOW(), INTERVAL 5 MINUTE) WHERE Username = @u" :
                            "UPDATE Usuarios SET Intentos = @i WHERE Username = @u";

                        using (var lockCmd = new MySqlCommand(updateSql, connection))
                        {
                            lockCmd.Parameters.AddWithValue("@i", intentos);
                            lockCmd.Parameters.AddWithValue("@u", user);
                            lockCmd.ExecuteNonQuery();
                        }
                        return (intentos >= 3) ? "LOCKED|5" : $"WRONG_PASS|{3 - intentos}";
                    }
                }
            }
            catch (Exception ex) { return "ERROR: " + ex.Message; }
        }

        // 4. Obtener ID
        public static string GetUserId(string username)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT Id FROM Usuarios WHERE Username = @u";
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "0000";
                    }
                }
            }
            catch { return "ERR"; }
        }

        // 5. Cambiar Contraseña
        public static bool UpdatePassword(string username, string newPassword)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "UPDATE Usuarios SET Password = @p WHERE Username = @u";
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@p", newPassword);
                        cmd.Parameters.AddWithValue("@u", username);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // 6. ACTUALIZAR SUSCRIPCIÓN (NUEVO)
        public static bool UpdateSubscription(string username, string plan)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "UPDATE Usuarios SET Suscripcion = @plan WHERE Username = @u";
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@plan", plan);
                        cmd.Parameters.AddWithValue("@u", username);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // 7. OBTENER SUSCRIPCIÓN ACTUAL (NUEVO)
        public static string GetSubscription(string username)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT Suscripcion FROM Usuarios WHERE Username = @u";
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        object result = cmd.ExecuteScalar();
                        // Si es null o está vacío, devolvemos "Cielo" (Gratis)
                        if (result == null || result == DBNull.Value || string.IsNullOrEmpty(result.ToString()))
                            return "Cielo";

                        return result.ToString();
                    }
                }
            }
            catch { return "Error"; }
        }
    }
}