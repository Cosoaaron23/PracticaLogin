using System;
using System.Data;
using MySql.Data.MySqlClient; // Asegúrate de tener el paquete NuGet MySql.Data instalado

namespace PracticaLogin
{
    public class DatabaseHelper
    {
        // =========================================================================
        // CONFIGURACIÓN DE CONEXIÓN
        // =========================================================================
        // ¡OJO! Pon tu contraseña real de MySQL aquí abajo
        private const string ConnectionString = "server=localhost;database=PracticaLogin;uid=root;pwd=1234;";

        // 1. INICIALIZAR (Probar conexión al arrancar)
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    // Si llega aquí sin error, la conexión es correcta
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar con MySQL: " + ex.Message);
            }
        }

        // 2. REGISTRAR USUARIO (Con todos los campos)
        public static bool RegisterUser(string nombre, string apellidos, string user, string pass, string email, string tlf, string cp)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    // A) Verificar si el usuario ya existe
                    string checkSql = "SELECT COUNT(*) FROM Usuarios WHERE Username = @u";
                    using (var checkCmd = new MySqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@u", user);
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count > 0) return false; // Ya existe, no registramos
                    }

                    // B) Insertar nuevo usuario (Iniciamos Intentos en 0 y BloqueadoHasta en NULL)
                    string insertSql = "INSERT INTO Usuarios (Nombre, Apellidos, Username, Password, Email, Telefono, CodigoPostal, Intentos, BloqueadoHasta) " +
                                       "VALUES (@nom, @ape, @u, @p, @mail, @tlf, @cp, 0, NULL)";

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
                    return true; // Registro exitoso
                }
            }
            catch
            {
                return false; // Fallo técnico
            }
        }

        // 3. VALIDAR LOGIN (Con sistema de bloqueo de seguridad)
        // Retorna códigos: "OK", "NO_USER", "WRONG_PASS|intentos", "LOCKED|minutos"
        public static string ValidateUser(string user, string pass)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    // A) Buscar datos del usuario
                    string sql = "SELECT Id, Password, Intentos, BloqueadoHasta FROM Usuarios WHERE Username = @u";

                    int intentos = 0;
                    DateTime? bloqueadoHasta = null;
                    string passwordReal = "";

                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", user);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return "NO_USER"; // El usuario no existe

                            passwordReal = reader["Password"].ToString();
                            intentos = Convert.ToInt32(reader["Intentos"]);

                            // Comprobar si hay fecha de bloqueo
                            if (reader["BloqueadoHasta"] != DBNull.Value)
                            {
                                bloqueadoHasta = Convert.ToDateTime(reader["BloqueadoHasta"]);
                            }
                        }
                    }

                    // B) Verificar si está bloqueado actualmente por tiempo
                    if (bloqueadoHasta != null && bloqueadoHasta > DateTime.Now)
                    {
                        TimeSpan tiempoRestante = bloqueadoHasta.Value - DateTime.Now;
                        // Devolvemos código LOCKED y los minutos que faltan
                        return $"LOCKED|{Math.Ceiling(tiempoRestante.TotalMinutes)}";
                    }

                    // C) Verificar Contraseña
                    if (pass == passwordReal)
                    {
                        // -- CONTRASEÑA CORRECTA --
                        // Reseteamos contador a 0 y quitamos bloqueo
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
                        // -- CONTRASEÑA INCORRECTA --
                        intentos++;
                        string updateSql = "";

                        if (intentos >= 3)
                        {
                            // Superó los 3 intentos: Bloqueamos por 5 minutos
                            updateSql = "UPDATE Usuarios SET Intentos = @i, BloqueadoHasta = DATE_ADD(NOW(), INTERVAL 5 MINUTE) WHERE Username = @u";
                            using (var lockCmd = new MySqlCommand(updateSql, connection))
                            {
                                lockCmd.Parameters.AddWithValue("@i", intentos);
                                lockCmd.Parameters.AddWithValue("@u", user);
                                lockCmd.ExecuteNonQuery();
                            }
                            return "LOCKED|5";
                        }
                        else
                        {
                            // Aún tiene intentos: Solo sumamos el contador
                            updateSql = "UPDATE Usuarios SET Intentos = @i WHERE Username = @u";
                            using (var incCmd = new MySqlCommand(updateSql, connection))
                            {
                                incCmd.Parameters.AddWithValue("@i", intentos);
                                incCmd.Parameters.AddWithValue("@u", user);
                                incCmd.ExecuteNonQuery();
                            }
                            return $"WRONG_PASS|{3 - intentos}"; // Devolvemos intentos restantes
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        // 4. OBTENER ID DEL USUARIO (Para la pestaña Cuenta)
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

                        if (result != null) return result.ToString();
                    }
                }
                return "Unknown";
            }
            catch { return "ERR"; }
        }

        // 5. CAMBIAR CONTRASEÑA (Para la pestaña Cuenta)
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
                        int rows = cmd.ExecuteNonQuery();

                        return rows > 0; // True si se cambió algo
                    }
                }
            }
            catch { return false; }
        }
    }
}