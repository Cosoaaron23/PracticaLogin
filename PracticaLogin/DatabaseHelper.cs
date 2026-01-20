using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;

namespace PracticaLogin
{
    public static class DatabaseHelper
    {
        // ⚠️ AJUSTA TU CONTRASEÑA AQUÍ
        private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=1234;";

        // =============================================================
        // 1. INICIALIZACIÓN
        // =============================================================
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Tabla Usuarios
                    string queryUsuarios = @"
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
                            bloqueado_hasta DATETIME NULL,
                            rol VARCHAR(20) DEFAULT 'USER',
                            activo TINYINT(1) DEFAULT 1
                        );";

                    // Tabla Logs (Para la extensión del PDF)
                    string queryLogs = @"
                        CREATE TABLE IF NOT EXISTS log_actividad (
                            id_log INT AUTO_INCREMENT PRIMARY KEY,
                            id_admin INT NOT NULL,
                            accion VARCHAR(50),
                            descripcion TEXT,
                            fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (id_admin) REFERENCES usuarios(id)
                        );";

                    // Admin por defecto
                    string queryAdmin = @"
                        INSERT IGNORE INTO usuarios (nombre, apellidos, username, password, email, rol, activo, suscripcion)
                        VALUES ('Jefe', 'Admin', 'admin', 'admin123', 'admin@akay.com', 'ADMIN', 1, 'VIP');";

                    using (var cmd = new MySqlCommand(queryUsuarios + queryLogs + queryAdmin, connection))
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

        // =============================================================
        // 2. LOGIN Y REGISTRO
        // =============================================================
        public static Usuario ValidateUser(string username, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, password, intentos_fallidos, bloqueado_hasta, rol, activo, email FROM usuarios WHERE username = @user";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var bloqueadoHasta = reader["bloqueado_hasta"];
                                if (bloqueadoHasta != DBNull.Value && DateTime.Now < Convert.ToDateTime(bloqueadoHasta))
                                {
                                    MessageBox.Show("Cuenta bloqueada temporalmente.");
                                    return null;
                                }

                                bool esActivo = Convert.ToBoolean(reader["activo"]);
                                if (!esActivo)
                                {
                                    MessageBox.Show("ESTA CUENTA HA SIDO BANEADA.");
                                    return null;
                                }

                                string dbPass = reader["password"].ToString();
                                if (dbPass == password)
                                {
                                    ResetIntentos(username);
                                    return new Usuario
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Username = username,
                                        Email = reader["email"].ToString(),
                                        Rol = reader["rol"].ToString(),
                                        Activo = true
                                    };
                                }
                                else
                                {
                                    reader.Close();
                                    AumentarIntentos(username);
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error Login: " + ex.Message); }
            return null;
        }

        public static bool RegisterUser(string nombre, string apellidos, string username, string password, string email, string telefono, string cp)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO usuarios (nombre, apellidos, username, password, email, telefono, cp) VALUES (@n, @a, @u, @p, @e, @t, @c)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", nombre); cmd.Parameters.AddWithValue("@a", apellidos);
                        cmd.Parameters.AddWithValue("@u", username); cmd.Parameters.AddWithValue("@p", password);
                        cmd.Parameters.AddWithValue("@e", email); cmd.Parameters.AddWithValue("@t", telefono); cmd.Parameters.AddWithValue("@c", cp);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch { return false; }
        }

        // =============================================================
        // 3. FUNCIONES DE ADMINISTRADOR (CON LOGS)
        // =============================================================

        public static List<Usuario> ObtenerUsuarios()
        {
            return BuscarUsuarios("");
        }

        public static List<Usuario> BuscarUsuarios(string filtro)
        {
            List<Usuario> lista = new List<Usuario>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, username, email, password, rol, activo FROM usuarios WHERE username LIKE @filtro";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Usuario
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                Email = reader["email"].ToString(),
                                Password = reader["password"].ToString(),
                                Rol = reader["rol"].ToString(),
                                Activo = Convert.ToBoolean(reader["activo"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // --- BANEO / DESBANEO (CON LOG) ---
        public static bool AlternarBloqueo(int idUsuario, bool estadoActual, int idAdmin)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var trans = conn.BeginTransaction();
                try
                {
                    int nuevoEstado = estadoActual ? 0 : 1;
                    string accion = nuevoEstado == 1 ? "DESBANEO" : "BANEO";

                    string q1 = "UPDATE usuarios SET activo = @act WHERE id = @id";
                    var cmd1 = new MySqlCommand(q1, conn, trans);
                    cmd1.Parameters.AddWithValue("@act", nuevoEstado);
                    cmd1.Parameters.AddWithValue("@id", idUsuario);
                    cmd1.ExecuteNonQuery();

                    string q2 = "INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora) VALUES (@admin, @accion, @desc, NOW())";
                    var cmd2 = new MySqlCommand(q2, conn, trans);
                    cmd2.Parameters.AddWithValue("@admin", idAdmin);
                    cmd2.Parameters.AddWithValue("@accion", accion);
                    cmd2.Parameters.AddWithValue("@desc", $"Usuario {idUsuario} cambiado a: {(nuevoEstado == 1 ? "Activo" : "Baneado")}");
                    cmd2.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch { trans.Rollback(); return false; }
            }
        }

        // --- ACTUALIZAR ROL/EMAIL (CON LOG) ---
        public static void ActualizarUsuario(int id, string nuevoEmail, string nuevoRol, int idAdmin)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var trans = conn.BeginTransaction();
                try
                {
                    string query = "UPDATE usuarios SET email = @email, rol = @rol WHERE id = @id";
                    var cmd = new MySqlCommand(query, conn, trans);
                    cmd.Parameters.AddWithValue("@email", nuevoEmail);
                    cmd.Parameters.AddWithValue("@rol", nuevoRol);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();

                    string log = "INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora) VALUES (@admin, 'EDICION', @desc, NOW())";
                    var cmdLog = new MySqlCommand(log, conn, trans);
                    cmdLog.Parameters.AddWithValue("@admin", idAdmin);
                    cmdLog.Parameters.AddWithValue("@desc", $"Editó datos del usuario {id} (Rol: {nuevoRol})");
                    cmdLog.ExecuteNonQuery();

                    trans.Commit();
                }
                catch { trans.Rollback(); throw; }
            }
        }

        // --- CAMBIAR PASS ADMIN (CON LOG) ---
        public static void AdminCambiarPass(int idUsuario, string nuevaPass, int idAdmin)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var trans = conn.BeginTransaction();
                try
                {
                    string query = "UPDATE usuarios SET password = @p WHERE id = @id";
                    var cmd = new MySqlCommand(query, conn, trans);
                    cmd.Parameters.AddWithValue("@p", nuevaPass);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    cmd.ExecuteNonQuery();

                    string log = "INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora) VALUES (@ad, 'PASSWORD_RESET', @desc, NOW())";
                    var cmdLog = new MySqlCommand(log, conn, trans);
                    cmdLog.Parameters.AddWithValue("@ad", idAdmin);
                    cmdLog.Parameters.AddWithValue("@desc", $"Reset de contraseña al usuario {idUsuario}");
                    cmdLog.ExecuteNonQuery();

                    trans.Commit();
                }
                catch { trans.Rollback(); throw; }
            }
        }

        // --- NUEVO: GENERAR TEXTO PARA EL REPORTE (EVIDENCIA) ---
        public static string ObtenerReporteLogs()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string reporte = "REPORTE DE ACTIVIDAD - AKAY ADMIN\n";
                reporte += "========================================\n";
                reporte += $"FECHA DE EMISIÓN: {DateTime.Now}\n\n";

                string query = @"SELECT L.fecha_hora, U.username, L.accion, L.descripcion 
                                 FROM log_actividad L 
                                 JOIN usuarios U ON L.id_admin = U.id 
                                 ORDER BY L.fecha_hora DESC";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fecha = reader.GetDateTime("fecha_hora").ToString("yyyy-MM-dd HH:mm:ss");
                        string admin = reader.GetString("username");
                        string accion = reader.GetString("accion");
                        string desc = reader.GetString("descripcion");

                        reporte += $"[{fecha}] ADMIN: {admin} | ACCIÓN: {accion}\n";
                        reporte += $"   DETALLE: {desc}\n";
                        reporte += "----------------------------------------\n";
                    }
                }
                return reporte;
            }
        }

        // =============================================================
        // 4. MÉTODOS AUXILIARES
        // =============================================================
        public static int GetUserId(string username)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id FROM usuarios WHERE username=@u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                var res = cmd.ExecuteScalar();
                return res != null ? Convert.ToInt32(res) : -1;
            }
        }

        public static string GetSubscription(int userId)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT suscripcion FROM usuarios WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                var res = cmd.ExecuteScalar();
                return res != null ? res.ToString() : "FREE";
            }
        }

        public static void UpdateSubscription(string username, string sub)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE usuarios SET suscripcion=@s WHERE username=@u", conn);
                cmd.Parameters.AddWithValue("@s", sub); cmd.Parameters.AddWithValue("@u", username);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdatePassword(int userId, string newPass)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE usuarios SET password=@p WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@p", newPass); cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void ResetIntentos(string username) => EjecutarSQL("UPDATE usuarios SET intentos_fallidos = 0, bloqueado_hasta = NULL WHERE username = '" + username + "'");
        private static void AumentarIntentos(string username) => EjecutarSQL("UPDATE usuarios SET intentos_fallidos = intentos_fallidos + 1 WHERE username = '" + username + "'");
        private static void EjecutarSQL(string query)
        {
            using (var conn = new MySqlConnection(connectionString)) { conn.Open(); new MySqlCommand(query, conn).ExecuteNonQuery(); }
        }
    }
}