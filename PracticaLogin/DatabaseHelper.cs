using System;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace PracticaLogin
{
    public class DatabaseHelper
    {
        // =========================================================================
        // CONFIGURACIÓN DE CONEXIÓN (PORTABLE - ARCHIVO LOCAL)
        // =========================================================================
        private const string DbName = "AkayData.db";
        private const string ConnectionString = "Data Source=" + DbName + ";Version=3;";

        // 1. INICIALIZAR (Crea el archivo de base de datos y la tabla si no existen)
        public static void InitializeDatabase()
        {
            try
            {
                // 1. Si el archivo no existe, lo creamos
                if (!File.Exists(DbName))
                {
                    SQLiteConnection.CreateFile(DbName);
                }

                // 2. Abrimos conexión y creamos la tabla si es la primera vez
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string sql = @"
                        CREATE TABLE IF NOT EXISTS Usuarios (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre VARCHAR(50),
                            Apellidos VARCHAR(100),
                            Email VARCHAR(100),
                            Telefono VARCHAR(20),
                            CodigoPostal VARCHAR(10),
                            Username VARCHAR(50) NOT NULL UNIQUE,
                            Password VARCHAR(100) NOT NULL,
                            Intentos INT DEFAULT 0,
                            BloqueadoHasta DATETIME DEFAULT NULL,
                            Suscripcion VARCHAR(20) DEFAULT 'Cielo'
                        );";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 3. Insertar el admin si la tabla estaba vacía
                    string checkAdmin = "SELECT COUNT(*) FROM Usuarios WHERE Username = 'admin'";
                    using (var checkCmd = new SQLiteCommand(checkAdmin, connection))
                    {
                        if (Convert.ToInt64(checkCmd.ExecuteScalar()) == 0)
                        {
                            string insertAdmin = @"
                                INSERT INTO Usuarios (Nombre, Apellidos, Username, Password, Suscripcion, Intentos) 
                                VALUES ('Administrador', 'Sistema', 'admin', '1234', 'Infierno', 0);";
                            using (var insertCmd = new SQLiteCommand(insertAdmin, connection))
                            {
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar con SQLite: " + ex.Message);
            }
        }

        // 2. REGISTRAR USUARIO (Adaptado a SQLite)
        public static bool RegisterUser(string nombre, string apellidos, string user, string pass, string email, string tlf, string cp)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string checkSql = "SELECT COUNT(*) FROM Usuarios WHERE Username = @u";
                    using (var checkCmd = new SQLiteCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@u", user);
                        if (Convert.ToInt64(checkCmd.ExecuteScalar()) > 0) return false;
                    }

                    string insertSql = "INSERT INTO Usuarios (Nombre, Apellidos, Username, Password, Email, Telefono, CodigoPostal, Intentos, BloqueadoHasta, Suscripcion) " +
                                       "VALUES (@nom, @ape, @u, @p, @mail, @tlf, @cp, 0, NULL, 'Cielo')";

                    using (var insertCmd = new SQLiteCommand(insertSql, connection))
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

        // 3. VALIDAR LOGIN (Adaptado a SQLite)
        public static string ValidateUser(string user, string pass)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string sql = "SELECT Id, Password, Intentos, BloqueadoHasta FROM Usuarios WHERE Username = @u";

                    int intentos = 0;
                    DateTime? bloqueadoHasta = null;
                    string passwordReal = "";

                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", user);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return "NO_USER";
                            passwordReal = reader["Password"].ToString();
                            intentos = Convert.ToInt32(reader["Intentos"]);

                            if (reader["BloqueadoHasta"] != DBNull.Value)
                            {
                                // Parseamos el string de fecha que devuelve SQLite
                                bloqueadoHasta = DateTime.Parse(reader["BloqueadoHasta"].ToString());
                            }
                        }
                    }

                    if (bloqueadoHasta != null && bloqueadoHasta > DateTime.Now)
                    {
                        TimeSpan tiempoRestante = bloqueadoHasta.Value - DateTime.Now;
                        return $"LOCKED|{Math.Ceiling(tiempoRestante.TotalMinutes)}";
                    }

                    if (pass == passwordReal)
                    {
                        string resetSql = "UPDATE Usuarios SET Intentos = 0, BloqueadoHasta = NULL WHERE Username = @u";
                        using (var updateCmd = new SQLiteCommand(resetSql, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@u", user);
                            updateCmd.ExecuteNonQuery();
                        }
                        return "OK";
                    }
                    else
                    {
                        // Fallo
                        intentos++;
                        string bloqueoFecha = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss");

                        string updateSql = (intentos >= 3) ?
                            "UPDATE Usuarios SET Intentos = @i, BloqueadoHasta = @bh WHERE Username = @u" :
                            "UPDATE Usuarios SET Intentos = @i WHERE Username = @u";

                        using (var lockCmd = new SQLiteCommand(updateSql, connection))
                        {
                            lockCmd.Parameters.AddWithValue("@i", intentos);
                            lockCmd.Parameters.AddWithValue("@bh", bloqueoFecha);
                            lockCmd.Parameters.AddWithValue("@u", user);
                            lockCmd.ExecuteNonQuery();
                        }
                        return (intentos >= 3) ? "LOCKED|5" : $"WRONG_PASS|{3 - intentos}";
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        // 4. Obtener ID del Usuario (Adaptado a SQLite)
        public static string GetUserId(string username)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT Id FROM Usuarios WHERE Username = @u";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "0000";
                    }
                }
            }
            catch { return "ERR"; }
        }

        // 5. Cambiar Contraseña (Adaptado a SQLite)
        public static bool UpdatePassword(string username, string newPassword)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "UPDATE Usuarios SET Password = @p WHERE Username = @u";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@p", newPassword);
                        cmd.Parameters.AddWithValue("@u", username);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // 6. Actualizar Suscripción (Adaptado a SQLite)
        public static bool UpdateSubscription(string username, string plan)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "UPDATE Usuarios SET Suscripcion = @plan WHERE Username = @u";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@plan", plan);
                        cmd.Parameters.AddWithValue("@u", username);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // 7. Obtener Suscripción Actual (Adaptado a SQLite)
        public static string GetSubscription(string username)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT Suscripcion FROM Usuarios WHERE Username = @u";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        object result = cmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value || string.IsNullOrEmpty(result.ToString()))
                            return "Cielo";

                        return result.ToString();
                    }
                }
            }
            catch { return "ERR"; }
        }
    }
}