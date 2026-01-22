using System;
using System.Collections.Generic;
using System.Text; // NECESARIO PARA EL REPORTE
using MySql.Data.MySqlClient;
using System.Windows;

namespace PracticaLogin
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=1234;";

        // =============================================================
        // 1. GENERAR REPORTE REAL (¡NUEVO!)
        // =============================================================
        public static string ObtenerReporteLogs()
        {
            var sb = new StringBuilder();

            // Cabecera del archivo de texto
            sb.AppendLine("=================================================================================");
            sb.AppendLine("                       REPORTE DE ACTIVIDAD - AKAY SYSTEM                        ");
            sb.AppendLine("=================================================================================");
            sb.AppendLine($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine("Generado por: Sistema Automático");
            sb.AppendLine("---------------------------------------------------------------------------------");
            sb.AppendLine("");

            // Encabezados de columna con espaciado fijo
            sb.AppendLine(string.Format("{0,-6} | {1,-20} | {2,-15} | {3,-10} | {4}",
                "FECHA", "HORA", "ADMIN", "ACCIÓN", "DETALLE"));
            sb.AppendLine(new string('-', 100));

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Consulta uniendo tablas para obtener el nombre del admin en vez de su ID
                    string query = @"
                        SELECT l.fecha_hora, u.username, l.accion, l.descripcion 
                        FROM log_actividad l
                        LEFT JOIN usuarios u ON l.id_admin = u.id
                        ORDER BY l.fecha_hora DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime fecha = Convert.ToDateTime(reader["fecha_hora"]);
                            string admin = reader["username"] != DBNull.Value ? reader["username"].ToString() : "Sistema/Desconocido";
                            string accion = reader["accion"].ToString();
                            string desc = reader["descripcion"].ToString();

                            // Escribimos la fila alineada
                            sb.AppendLine(string.Format("{0,-6} | {1,-20} | {2,-15} | {3,-10} | {4}",
                                fecha.ToString("dd/MM"),
                                fecha.ToString("HH:mm:ss"),
                                admin.Length > 14 ? admin.Substring(0, 14) : admin, // Recortar si es muy largo
                                accion,
                                desc));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("\n[ERROR AL LEER LOGS]: " + ex.Message);
            }

            sb.AppendLine("");
            sb.AppendLine("=================================================================================");
            sb.AppendLine("FIN DEL REPORTE");

            return sb.ToString();
        }

        // =============================================================
        // 2. LEER USUARIOS (CON FECHA ARREGLADA)
        // =============================================================
        public static List<Usuario> ObtenerUsuarios() => BuscarUsuarios("");

        public static List<Usuario> BuscarUsuarios(string filtro)
        {
            var lista = new List<Usuario>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM usuarios WHERE username LIKE @filtro";
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
                                GradoBaneo = reader["grado_baneo"] != DBNull.Value ? Convert.ToInt32(reader["grado_baneo"]) : 0,
                                // Lectura correcta de la fecha para que se vea en la tabla
                                FinBaneo = reader["fin_baneo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["fin_baneo"]) : null,
                                Suscripcion = reader["suscripcion"].ToString()
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // =============================================================
        // 3. LOGIN Y VALIDACIÓN
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
                            DateTime? finBaneo = reader["fin_baneo"] != DBNull.Value ? (DateTime?)reader["fin_baneo"] : null;
                            int idUsuario = Convert.ToInt32(reader["id"]);

                            if (!activo)
                            {
                                if (finBaneo.HasValue && finBaneo.Value <= DateTime.Now)
                                {
                                    reader.Close();
                                    LevantarCastigo(idUsuario, 0);
                                    return ValidateUser(user, pass, out errorMsg);
                                }
                                else
                                {
                                    errorMsg = "CUENTA SUSPENDIDA TEMPORALMENTE";
                                    return null;
                                }
                            }
                            return new Usuario
                            {
                                Id = idUsuario,
                                Username = reader["username"].ToString(),
                                Rol = reader["rol"].ToString(),
                                Activo = true,
                                GradoBaneo = 0,
                                Suscripcion = reader["suscripcion"].ToString()
                            };
                        }
                    }
                }
            }
            errorMsg = "Credenciales incorrectas.";
            return null;
        }

        // =============================================================
        // 4. ACCIONES ADMINISTRATIVAS (Inserts & Updates)
        // =============================================================
        public static void AplicarSancion(int idUsuario, int grado, int idAdmin)
        {
            DateTime fechaFin = DateTime.Now;
            switch (grado)
            {
                case 1: fechaFin = fechaFin.AddDays(1); break;
                case 2: fechaFin = fechaFin.AddDays(3); break;
                case 3: fechaFin = fechaFin.AddDays(7); break;
                case 4: fechaFin = fechaFin.AddMonths(1); break;
                case 5: fechaFin = new DateTime(9999, 12, 31); break;
                default: fechaFin = fechaFin.AddDays(1); break;
            }
            string f = fechaFin.ToString("yyyy-MM-dd HH:mm:ss");
            EjecutarSQL($"UPDATE usuarios SET activo=0, grado_baneo={grado}, fin_baneo='{f}' WHERE id={idUsuario}");
            RegistrarLog(idAdmin, "BANEO", $"Sanción Nivel {grado} aplicada.");
        }

        public static void RegistrarLog(int idA, string acc, string desc)
        {
            try { EjecutarSQL($"INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora) VALUES ({idA}, '{acc}', '{desc}', NOW())"); } catch { }
        }

        public static void LevantarCastigo(int id, int admin)
        {
            EjecutarSQL($"UPDATE usuarios SET activo=1, grado_baneo=0, fin_baneo=NULL WHERE id={id}");
            if (admin > 0) RegistrarLog(admin, "INDULTO", $"Castigo levantado a ID {id}");
        }

        public static void EliminarUsuarioTotal(int id, int admin)
        {
            EjecutarSQL($"DELETE FROM usuarios WHERE id={id}");
            RegistrarLog(admin, "ELIMINAR", $"Usuario ID {id} eliminado permanentemente.");
        }

        public static void AdminCrearUsuario(string u, string p, string e, string r, int admin)
        {
            EjecutarSQL($"INSERT INTO usuarios (username, password, email, rol, activo) VALUES ('{u}', '{p}', '{e}', '{r}', 1)");
            RegistrarLog(admin, "CREAR", $"Nuevo usuario creado: {u}");
        }

        public static void AdminCambiarPass(int id, string p, int admin)
        {
            EjecutarSQL($"UPDATE usuarios SET password='{p}' WHERE id={id}");
            RegistrarLog(admin, "PASSWORD", $"Contraseña cambiada para ID {id}");
        }

        public static void UpdatePassword(int id, string p) => EjecutarSQL($"UPDATE usuarios SET password='{p}' WHERE id={id}");

        public static void ActualizarUsuario(int id, string e, string r) => EjecutarSQL($"UPDATE usuarios SET email='{e}', rol='{r}' WHERE id={id}");

        // =============================================================
        // 5. APELACIONES Y OTROS
        // =============================================================
        public static void EnviarApelacion(string u, string m)
        {
            int id = GetUserId(u);
            EjecutarSQL($"INSERT INTO apelaciones (id_usuario, username, mensaje, fecha) VALUES ({id}, '{u}', '{m}', NOW())");
        }

        public static List<Apelacion> ObtenerApelaciones()
        {
            var l = new List<Apelacion>();
            using (var c = new MySqlConnection(connectionString))
            {
                c.Open();
                using (var r = new MySqlCommand("SELECT * FROM apelaciones ORDER BY fecha DESC", c).ExecuteReader())
                    while (r.Read()) l.Add(new Apelacion { Id = Convert.ToInt32(r["id"]), Username = r["username"].ToString(), Mensaje = r["mensaje"].ToString(), Fecha = Convert.ToDateTime(r["fecha"]) });
            }
            return l;
        }

        public static void EliminarApelacion(int id) => EjecutarSQL("DELETE FROM apelaciones WHERE id=" + id);

        public static bool RegisterUser(string n, string a, string u, string p, string e, string t, string c)
        {
            try { EjecutarSQL($"INSERT INTO usuarios (nombre, apellidos, username, password, email, telefono, cp) VALUES ('{n}','{a}','{u}','{p}','{e}','{t}','{c}')"); return true; } catch { return false; }
        }

        public static void InitializeDatabase() { }
        public static string GetSubscription(int id) => "FREE";
        public static void UpdateSubscription(string u, string s) => EjecutarSQL($"UPDATE usuarios SET suscripcion='{s}' WHERE username='{u}'");

        // =============================================================
        // AUXILIARES
        // =============================================================
        private static int GetUserId(string u)
        {
            using (var c = new MySqlConnection(connectionString))
            {
                c.Open();
                var res = new MySqlCommand($"SELECT id FROM usuarios WHERE username='{u}'", c).ExecuteScalar();
                return res != null ? Convert.ToInt32(res) : -1;
            }
        }
        private static void EjecutarSQL(string q)
        {
            using (var c = new MySqlConnection(connectionString)) { c.Open(); new MySqlCommand(q, c).ExecuteNonQuery(); }
        }
    }
}