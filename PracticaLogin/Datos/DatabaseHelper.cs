using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows;

namespace PracticaLogin
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=1234;";

        // =============================================================
        // 1. GESTIÓN DE JUEGOS (DOBLE IMAGEN: PORTADA + BANNER)
        // =============================================================

        public static List<Juego> ObtenerJuegos()
        {
            return EjecutarConsultaJuegos("SELECT * FROM videojuegos");
        }

        public static List<Juego> ObtenerDestacados()
        {
            // Filtro simple para "Destacados"
            return EjecutarConsultaJuegos("SELECT * FROM videojuegos WHERE coste > 40 OR online = 1 LIMIT 5");
        }

        public static List<Juego> ObtenerMisJuegos(int idUsuario)
        {
            // JOIN para obtener biblioteca del usuario
            string query = $"SELECT v.* FROM videojuegos v JOIN biblioteca b ON v.id = b.id_videojuego WHERE b.id_usuario = {idUsuario}";
            return EjecutarConsultaJuegos(query);
        }

        public static bool UsuarioTieneJuego(int idUsuario, int idJuego)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM biblioteca WHERE id_usuario=@uid AND id_videojuego=@gid";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", idUsuario);
                        cmd.Parameters.AddWithValue("@gid", idJuego);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch { return false; }
        }

        // --- NUEVO: ESTADO DE DESCARGA ---
        public static int EstadoPropiedadJuego(int idUsuario, int idJuego)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Requiere: ALTER TABLE biblioteca ADD COLUMN descargado TINYINT DEFAULT 0;
                    string query = "SELECT descargado FROM biblioteca WHERE id_usuario=@uid AND id_videojuego=@gid";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", idUsuario);
                        cmd.Parameters.AddWithValue("@gid", idJuego);
                        var res = cmd.ExecuteScalar();
                        if (res == null) return 0; // No comprado
                        return Convert.ToInt32(res) == 1 ? 2 : 1; // 2=Descargado, 1=Comprado
                    }
                }
            }
            catch { return 0; }
        }

        public static void MarcarComoDescargado(int idUsuario, int idJuego) =>
            EjecutarSQL($"UPDATE biblioteca SET descargado=1 WHERE id_usuario={idUsuario} AND id_videojuego={idJuego}");

        // --- CRUD ADMIN (SOPORTE PARA 2 IMÁGENES) ---

        // Método CREAR actualizado: Recibe imagenPath (Vertical) y imagenFondoPath (Horizontal)
        public static void CrearJuego(string titulo, decimal precio, double gb, string genero, int jugadores, bool online, string imagenPath, string imagenFondoPath)
        {
            // Si no se envían imágenes, ponemos placeholders
            if (string.IsNullOrEmpty(imagenPath)) imagenPath = "/Assets/logo.png";
            if (string.IsNullOrEmpty(imagenFondoPath)) imagenFondoPath = imagenPath; // Si no hay fondo, usa la portada

            string sql = $@"INSERT INTO videojuegos (titulo, tematica, coste, espacio_gb, online, jugadores, imagen_url, imagen_fondo_url) 
                            VALUES ('{titulo}', '{genero}', {precio.ToString().Replace(",", ".")}, 
                            {gb.ToString().Replace(",", ".")}, {(online ? 1 : 0)}, {jugadores}, '{imagenPath}', '{imagenFondoPath}')";

            EjecutarSQL(sql);
        }

        // Método ACTUALIZAR actualizado
        public static void ActualizarJuego(int id, string titulo, decimal precio, double gb, string genero, int jugadores, bool online, string imagenPath, string imagenFondoPath)
        {
            // Construimos la query dinámica por si solo cambia una imagen
            string imgQuery = !string.IsNullOrEmpty(imagenPath) ? $", imagen_url='{imagenPath}'" : "";
            string imgFondoQuery = !string.IsNullOrEmpty(imagenFondoPath) ? $", imagen_fondo_url='{imagenFondoPath}'" : "";

            string sql = $@"UPDATE videojuegos SET 
                            titulo='{titulo}', 
                            tematica='{genero}', 
                            coste={precio.ToString().Replace(",", ".")}, 
                            espacio_gb={gb.ToString().Replace(",", ".")}, 
                            online={(online ? 1 : 0)}, 
                            jugadores={jugadores} 
                            {imgQuery} 
                            {imgFondoQuery}
                            WHERE id={id}";

            EjecutarSQL(sql);
        }

        public static void EliminarJuego(int id) => EjecutarSQL($"DELETE FROM videojuegos WHERE id={id}");
        public static void ComprarJuego(int idUsuario, int idJuego)
        {
            // Insertamos el juego en la biblioteca marcando descargado como 0 (falso)
            string sql = $"INSERT INTO biblioteca (id_usuario, id_videojuego, descargado) VALUES ({idUsuario}, {idJuego}, 0)";
            EjecutarSQL(sql);
        }

        // --- LECTURA DE DATOS (MAPEO) ---
        private static List<Juego> EjecutarConsultaJuegos(string query)
        {
            var lista = new List<Juego>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) lista.Add(MapearJuego(reader));
                    }
                }
            }
            catch { }
            return lista;
        }

        private static Juego MapearJuego(MySqlDataReader reader)
        {
            return new Juego
            {
                Id = Convert.ToInt32(reader["id"]),
                Titulo = reader["titulo"].ToString(),
                Genero = reader["tematica"].ToString(),
                Precio = Convert.ToDecimal(reader["coste"]),
                TamanoGb = Convert.ToDouble(reader["espacio_gb"]),
                EsOnline = Convert.ToBoolean(reader["online"]),
                NumJugadores = Convert.ToInt32(reader["jugadores"]),

                // Leemos la portada vertical
                ImagenUrl = reader["imagen_url"] != DBNull.Value ? reader["imagen_url"].ToString() : "/Assets/logo.png",

                // Leemos el fondo horizontal (NUEVO) - Con comprobación de seguridad por si la columna no existe
                ImagenFondoUrl = HasColumn(reader, "imagen_fondo_url") && reader["imagen_fondo_url"] != DBNull.Value
                                 ? reader["imagen_fondo_url"].ToString()
                                 : ""
            };
        }

        private static bool HasColumn(MySqlDataReader r, string columnName)
        {
            for (int i = 0; i < r.FieldCount; i++) if (r.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }

        // =============================================================
        // 2. USUARIOS, LOGIN Y SUSCRIPCIONES (SIN CAMBIOS)
        // =============================================================
        public static void InitializeDatabase() { }
        public static string GetSubscription(int id) { using (var c = new MySqlConnection(connectionString)) { c.Open(); var res = new MySqlCommand($"SELECT suscripcion FROM usuarios WHERE id={id}", c).ExecuteScalar(); return res != null ? res.ToString() : "FREE"; } }
        public static void UpdateSubscription(int id, string s) => EjecutarSQL($"UPDATE usuarios SET suscripcion='{s}' WHERE id={id}");
        public static void UpdatePassword(int id, string p) => EjecutarSQL($"UPDATE usuarios SET password='{p}' WHERE id={id}");

        public static Usuario ValidateUser(string user, string pass, out string errorMsg)
        {
            errorMsg = "";
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM usuarios WHERE username=@u AND password=@p", conn))
                {
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.Parameters.AddWithValue("@p", pass);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!Convert.ToBoolean(reader["activo"]))
                            {
                                // Comprobación de baneo expirado
                                DateTime? fin = reader["fin_baneo"] != DBNull.Value ? (DateTime?)reader["fin_baneo"] : null;
                                if (fin.HasValue && fin.Value <= DateTime.Now)
                                {
                                    int uid = Convert.ToInt32(reader["id"]);
                                    reader.Close();
                                    LevantarCastigo(uid, 0);
                                    return ValidateUser(user, pass, out errorMsg);
                                }
                                errorMsg = "Bloqueado"; return null;
                            }
                            return new Usuario { Id = Convert.ToInt32(reader["id"]), Username = reader["username"].ToString(), Rol = reader["rol"].ToString(), Activo = true, Suscripcion = reader["suscripcion"].ToString() };
                        }
                    }
                }
            }
            errorMsg = "Error login";
            return null;
        }

        // =============================================================
        // 3. ADMIN: SANCIONES Y LOGS (SIN CAMBIOS)
        // =============================================================
        public static void RegistrarLog(int id, string a, string d) => EjecutarSQL($"INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora) VALUES ({id}, '{a}', '{d}', NOW())");

        public static void AplicarSancion(int idUsuario, int grado, int idAdmin)
        {
            DateTime fechaFin = DateTime.Now;
            switch (grado) { case 1: fechaFin = fechaFin.AddDays(1); break; case 2: fechaFin = fechaFin.AddDays(3); break; case 3: fechaFin = fechaFin.AddDays(7); break; case 4: fechaFin = fechaFin.AddMonths(1); break; case 5: fechaFin = new DateTime(9999, 12, 31); break; }
            EjecutarSQL($"UPDATE usuarios SET activo=0, grado_baneo={grado}, fin_baneo='{fechaFin:yyyy-MM-dd HH:mm:ss}' WHERE id={idUsuario}");
            RegistrarLog(idAdmin, "BANEO", $"Sanción Nivel {grado}");
        }

        public static void LevantarCastigo(int id, int admin) { EjecutarSQL($"UPDATE usuarios SET activo=1, grado_baneo=0, fin_baneo=NULL WHERE id={id}"); if (admin > 0) RegistrarLog(admin, "INDULTO", $"Desbaneo ID {id}"); }
        public static void EliminarUsuarioTotal(int id, int admin) { EjecutarSQL($"DELETE FROM usuarios WHERE id={id}"); RegistrarLog(admin, "ELIMINAR", $"Borrado ID {id}"); }
        public static void AdminCrearUsuario(string u, string p, string e, string r, int admin) { EjecutarSQL($"INSERT INTO usuarios (username,password,email,rol,activo) VALUES ('{u}','{p}','{e}','{r}',1)"); RegistrarLog(admin, "CREAR", $"Usuario {u}"); }
        public static void AdminCambiarPass(int id, string p, int admin) { EjecutarSQL($"UPDATE usuarios SET password='{p}' WHERE id={id}"); RegistrarLog(admin, "PASS", $"Pass ID {id}"); }
        public static void ActualizarUsuario(int id, string e, string r) => EjecutarSQL($"UPDATE usuarios SET email='{e}', rol='{r}' WHERE id={id}");

        // --- REPORTES Y APELACIONES ---
        public static string ObtenerReporteLogs()
        {
            var sb = new StringBuilder(); sb.AppendLine("LOGS DE SISTEMA");
            try { using (var c = new MySqlConnection(connectionString)) { c.Open(); using (var r = new MySqlCommand("SELECT l.fecha_hora, u.username, l.accion, l.descripcion FROM log_actividad l LEFT JOIN usuarios u ON l.id_admin = u.id ORDER BY l.fecha_hora DESC", c).ExecuteReader()) while (r.Read()) sb.AppendLine($"{r["fecha_hora"]} | {r["username"]} | {r["accion"]} | {r["descripcion"]}"); } } catch { }
            return sb.ToString();
        }

        // --- SOPORTE TÉCNICO ---
        public static void EnviarTicketSoporte(int idUsuario, string motivo, string mensaje)
        {
            string sql = $"INSERT INTO soporte (id_usuario, motivo, mensaje) VALUES ({idUsuario}, '{motivo}', '{mensaje}')";
            EjecutarSQL(sql);
        }

        // --- GESTIÓN DE SOPORTE (PARA ADMIN) ---

        public static List<TicketSoporte> ObtenerTodosLosTickets()
        {
            var lista = new List<TicketSoporte>();
            try
            {
                using (var c = new MySqlConnection(connectionString))
                {
                    c.Open();
                    // Traemos el nombre del usuario que mandó el ticket con un JOIN
                    string q = "SELECT s.*, u.username FROM soporte s JOIN usuarios u ON s.id_usuario = u.id ORDER BY s.fecha DESC";
                    using (var r = new MySqlCommand(q, c).ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new TicketSoporte
                            {
                                Id = Convert.ToInt32(r["id"]),
                                Usuario = r["username"].ToString(),
                                Motivo = r["motivo"].ToString(),
                                Mensaje = r["mensaje"].ToString(),
                                Fecha = Convert.ToDateTime(r["fecha"]),
                                Estado = r["estado"].ToString()
                            });
                        }
                    }
                }
            }
            catch { }
            return lista;
        }



        public static void CerrarTicket(int idTicket)
        {
            EjecutarSQL($"UPDATE soporte SET estado='RESUELTO' WHERE id={idTicket}");
        }

        public static void EnviarApelacion(string u, string m) { int id = GetUserId(u); if (id != -1) EjecutarSQL($"INSERT INTO apelaciones (id_usuario,username,mensaje,fecha) VALUES ({id},'{u}','{m}',NOW())"); }
        public static List<Apelacion> ObtenerApelaciones() { var l = new List<Apelacion>(); using (var c = new MySqlConnection(connectionString)) { c.Open(); using (var r = new MySqlCommand("SELECT * FROM apelaciones ORDER BY fecha DESC", c).ExecuteReader()) while (r.Read()) l.Add(new Apelacion { Id = Convert.ToInt32(r["id"]), Username = r["username"].ToString(), Mensaje = r["mensaje"].ToString(), Fecha = Convert.ToDateTime(r["fecha"]) }); } return l; }
        public static void EliminarApelacion(int id) => EjecutarSQL("DELETE FROM apelaciones WHERE id=" + id);

        public static bool RegisterUser(string n, string a, string u, string p, string e, string t, string c) { try { EjecutarSQL($"INSERT INTO usuarios (nombre,apellidos,username,password,email,telefono,cp) VALUES ('{n}','{a}','{u}','{p}','{e}','{t}','{c}')"); return true; } catch { return false; } }
        public static List<Usuario> ObtenerUsuarios() => BuscarUsuarios("");
        public static List<Usuario> BuscarUsuarios(string f)
        {
            var l = new List<Usuario>();
            using (var c = new MySqlConnection(connectionString))
            {
                c.Open();
                using (var r = new MySqlCommand("SELECT * FROM usuarios WHERE username LIKE @f", c))
                {
                    r.Parameters.AddWithValue("@f", "%" + f + "%");
                    using (var rd = r.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            l.Add(new Usuario
                            {
                                Id = Convert.ToInt32(rd["id"]),
                                Username = rd["username"].ToString(),
                                // AQUI AÑADIMOS LA CONTRASEÑA
                                Password = rd["password"].ToString(),
                                Email = rd["email"].ToString(),
                                Rol = rd["rol"].ToString(),
                                Activo = Convert.ToBoolean(rd["activo"]),
                                Suscripcion = rd["suscripcion"].ToString(),
                                // AQUI AÑADIMOS LOS DATOS DE BANEO
                                GradoBaneo = rd["grado_baneo"] != DBNull.Value ? Convert.ToInt32(rd["grado_baneo"]) : 0,
                                FinBaneo = rd["fin_baneo"] != DBNull.Value ? (DateTime?)rd["fin_baneo"] : null
                            });
                        }
                    }
                }
            }
            return l;
        }

        private static int GetUserId(string u) { using (var c = new MySqlConnection(connectionString)) { c.Open(); var r = new MySqlCommand($"SELECT id FROM usuarios WHERE username='{u}'", c).ExecuteScalar(); return r != null ? Convert.ToInt32(r) : -1; } }
        private static void EjecutarSQL(string q) { using (var c = new MySqlConnection(connectionString)) { c.Open(); new MySqlCommand(q, c).ExecuteNonQuery(); } }
    }
}