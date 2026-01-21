using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;

namespace PracticaLogin
{
    public static class DatabaseHelper
    {
        // ⚠️ CONFIGURACIÓN DE CONEXIÓN
        private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=1234;";

        // =============================================================
        // 1. INICIALIZACIÓN DE LA BASE DE DATOS
        // =============================================================
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // A. Tabla Usuarios (Actualizada con campos para Grados de Baneo)
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
                            activo TINYINT(1) DEFAULT 1,
                            grado_baneo INT DEFAULT 0,    -- 0: Limpio, 1-5: Sancionado
                            fin_baneo DATETIME NULL       -- Fecha exacta de fin del castigo
                        );";

                    // B. Tabla Logs de Actividad (Para el reporte PDF)
                    string queryLogs = @"
                        CREATE TABLE IF NOT EXISTS log_actividad (
                            id_log INT AUTO_INCREMENT PRIMARY KEY,
                            id_admin INT NOT NULL,
                            accion VARCHAR(50),
                            descripcion TEXT,
                            fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (id_admin) REFERENCES usuarios(id)
                        );";

                    // C. Tabla Apelaciones (NUEVO: Buzón de mensajes)
                    string queryApelaciones = @"
                        CREATE TABLE IF NOT EXISTS apelaciones (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            id_usuario INT NOT NULL,
                            username VARCHAR(50),
                            mensaje TEXT,
                            fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (id_usuario) REFERENCES usuarios(id)
                        );";

                    // D. Admin por defecto
                    string queryAdmin = @"
                        INSERT IGNORE INTO usuarios (nombre, apellidos, username, password, email, rol, activo, suscripcion)
                        VALUES ('Jefe', 'Admin', 'admin', 'admin123', 'admin@akay.com', 'ADMIN', 1, 'VIP');";

                    using (var cmd = new MySqlCommand(queryUsuarios + queryLogs + queryApelaciones + queryAdmin, connection))
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
        // 2. LOGIN INTELIGENTE (AUTO-DESBANEO)
        // =============================================================
        public static Usuario ValidateUser(string user, string pass, out string errorMsg)
        {
            errorMsg = "";
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM usuarios WHERE username = @user AND password = @pass";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.Parameters.AddWithValue("@pass", pass);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool activo = Convert.ToBoolean(reader["activo"]);
                            DateTime? finBaneo = reader["fin_baneo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["fin_baneo"]) : null;
                            int idUsuario = Convert.ToInt32(reader["id"]);

                            // --- AQUÍ ESTÁ LA LÓGICA AUTOMÁTICA ---

                            if (!activo) // Si está marcado como baneado...
                            {
                                // 1. COMPROBAMOS SI YA CUMPLIÓ EL TIEMPO
                                if (finBaneo.HasValue && finBaneo.Value <= DateTime.Now)
                                {
                                    // ¡YA ES LIBRE! 
                                    // Tenemos que cerrar el Reader actual para poder hacer un UPDATE
                                    reader.Close();

                                    // Actualizamos la BD: Lo ponemos ACTIVO y borramos la fecha de baneo
                                    string unbanQuery = "UPDATE usuarios SET activo = 1, grado_baneo = 0, fin_baneo = NULL WHERE id = @id";
                                    using (var cmdUnban = new MySqlCommand(unbanQuery, conn))
                                    {
                                        cmdUnban.Parameters.AddWithValue("@id", idUsuario);
                                        cmdUnban.ExecuteNonQuery();
                                    }

                                    // Volvemos a leer al usuario (o lo creamos manual) para dejarle entrar
                                    // Para hacerlo fácil, hacemos recursividad: llamamos de nuevo a ValidateUser ahora que ya está limpio
                                    return ValidateUser(user, pass, out errorMsg);
                                }
                                else
                                {
                                    // SIGUE BANEADO (Aún no ha pasado la fecha)
                                    string fechaFin = finBaneo.HasValue ? finBaneo.Value.ToString("dd/MM/yyyy HH:mm") : "Indefinido";

                                    if (Convert.ToInt32(reader["grado_baneo"]) == 5)
                                        errorMsg = "CUENTA BLOQUEADA PERMANENTEMENTE.";
                                    else
                                        errorMsg = $"TU CUENTA ESTÁ SUSPENDIDA HASTA EL: {fechaFin}";

                                    return null;
                                }
                            }

                            // SI LLEGA AQUÍ, ES QUE ESTÁ ACTIVO Y PUEDE ENTRAR
                            return new Usuario
                            {
                                Username = reader["username"].ToString(),
                                Password = reader["password"].ToString(),
                                Email = reader["email"].ToString(),
                                Rol = reader["rol"].ToString(),
                                Activo = true,
                                GradoBaneo = 0
                            };
                        }
                    }
                }
            }
            errorMsg = "Usuario o contraseña incorrectos.";
            return null;
        }

        // =============================================================
        // 3. REGISTRO DE USUARIOS
        // =============================================================
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
        // 4. FUNCIONES DE ADMIN (LECTURA Y BÚSQUEDA)
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
                // AÑADIDOS: grado_baneo y fin_baneo
                string query = "SELECT id, username, email, password, rol, activo, grado_baneo, fin_baneo FROM usuarios WHERE username LIKE @filtro";
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
                                Activo = Convert.ToBoolean(reader["activo"]),
                                // MAPEO DE LOS NUEVOS CAMPOS
                                GradoBaneo = reader["grado_baneo"] != DBNull.Value ? Convert.ToInt32(reader["grado_baneo"]) : 0,
                                FinBaneo = reader["fin_baneo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["fin_baneo"]) : null
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // =============================================================
        // 5. NUEVA LÓGICA DE SANCIONES (GRADOS 1-5)
        // =============================================================

        // Aplicar una sanción con fecha de caducidad
        public static void AplicarSancion(int idUsuario, int grado, int idAdmin)
        {
            // 1. Calcular la fecha de fin en C# para evitar errores de SQL
            DateTime fechaFin = DateTime.Now;

            switch (grado)
            {
                case 1: fechaFin = fechaFin.AddDays(1); break;
                case 2: fechaFin = fechaFin.AddDays(3); break;
                case 3: fechaFin = fechaFin.AddDays(7); break;
                case 4: fechaFin = fechaFin.AddMonths(1); break;
                case 5: fechaFin = new DateTime(9999, 12, 31); break; // Permanente
                default: fechaFin = fechaFin.AddDays(1); break;
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Actualizamos al usuario: Lo marcamos como NO ACTIVO (0) y ponemos la fecha
                string query = "UPDATE usuarios SET activo = 0, grado_baneo = @grado, fin_baneo = @fecha WHERE id = @id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@grado", grado);
                    cmd.Parameters.AddWithValue("@fecha", fechaFin);
                    cmd.Parameters.AddWithValue("@id", idUsuario);

                    cmd.ExecuteNonQuery();
                }

                // (Opcional) Aquí podrías añadir un INSERT en una tabla de 'logs' si la tienes
                // LogHelper.RegistrarAccion(idAdmin, "Baneó al usuario ID " + idUsuario);
            }
        }

        // Levantar castigo (Indulto)
        public static void LevantarCastigo(int idUsuario)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string q = "UPDATE usuarios SET activo=1, grado_baneo=0, fin_baneo=NULL WHERE id=@id";
                var cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        // Mantener este método antiguo por compatibilidad con el botón actual (se convertirá en baneo simple o indulto)
        public static bool AlternarBloqueo(int idUsuario, bool estadoActual, int idAdmin)
        {
            if (estadoActual) // Si está activo, lo baneamos (Por defecto usaremos Grado 5 Permanente si usamos el botón viejo)
            {
                AplicarSancion(idUsuario, 5, idAdmin);
                return true;
            }
            else // Si está baneado, lo liberamos
            {
                LevantarCastigo(idUsuario);

                // Log de indulto manual
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string q = "INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora) VALUES (@ad, 'DESBANEO', @desc, NOW())";
                    var cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@ad", idAdmin);
                    cmd.Parameters.AddWithValue("@desc", $"Levantó castigo manualmente al usuario {idUsuario}");
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
        }

        // =============================================================
        // 6. NUEVA LÓGICA DE APELACIONES (BUZÓN)
        // =============================================================

        // Usuario envía mensaje
        public static void EnviarApelacion(string username, string mensaje)
        {
            int idUser = GetUserId(username);
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string q = "INSERT INTO apelaciones (id_usuario, username, mensaje, fecha) VALUES (@id, @u, @m, NOW())";
                var cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", idUser);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@m", mensaje);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarApelacion(int idApelacion)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM apelaciones WHERE id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idApelacion);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Admin lee mensajes
        public static List<Apelacion> ObtenerApelaciones()
        {
            var lista = new List<Apelacion>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string q = "SELECT * FROM apelaciones ORDER BY fecha DESC";
                var cmd = new MySqlCommand(q, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Apelacion
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                            Username = reader["username"].ToString(),
                            Mensaje = reader["mensaje"].ToString(),
                            Fecha = Convert.ToDateTime(reader["fecha"])
                        });
                    }
                }
            }
            return lista;
        }

        // =============================================================
        // 7. FUNCIONES DE ACTUALIZACIÓN Y LOGS (Admin)
        // =============================================================

        public static void ActualizarUsuario(int idUsuario, string email, string rol)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Solo actualizamos Email y Rol (el resto no se toca desde el botón guardar)
                string query = "UPDATE usuarios SET email = @email, rol = @rol WHERE id = @id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@rol", rol);
                    cmd.Parameters.AddWithValue("@id", idUsuario);

                    cmd.ExecuteNonQuery();
                }
            }
        }

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

        // Generar Reporte de Texto (Evidencia)
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
        // 8. MÉTODOS AUXILIARES Y USUARIO
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
                MessageBox.Show("Tu contraseña ha sido actualizada.");
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