using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace PracticaLogin
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=1234;";

        // CONEXIÓN PÚBLICA (Necesaria para ChatHelper)
        public static MySqlConnection GetConnection() => new MySqlConnection(connectionString);
        public static void InitializeDatabase() { } // Método vacío para evitar error de inicio

        // =============================================================
        // 1. GESTIÓN DE JUEGOS Y BIBLIOTECA
        // =============================================================
        public static List<Juego> ObtenerJuegos() => EjecutarConsultaJuegos("SELECT * FROM videojuegos");

        public static List<Juego> ObtenerDestacados() => EjecutarConsultaJuegos("SELECT * FROM videojuegos WHERE coste > 40 OR online = 1 LIMIT 5");

        public static List<Juego> ObtenerBiblioteca(int idUsuario) => EjecutarConsultaJuegos($"SELECT v.* FROM videojuegos v JOIN biblioteca b ON v.id = b.id_videojuego WHERE b.id_usuario = {idUsuario}");

        public static bool UsuarioTieneJuego(int idUsuario, int idJuego)
        {
            try { using (var c = GetConnection()) { c.Open(); using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM biblioteca WHERE id_usuario=@u AND id_videojuego=@g", c)) { cmd.Parameters.AddWithValue("@u", idUsuario); cmd.Parameters.AddWithValue("@g", idJuego); return Convert.ToInt32(cmd.ExecuteScalar()) > 0; } } } catch { return false; }
        }

        // MÉTODO CORREGIDO Y BLINDADO
        public static int EstadoPropiedadJuego(int idUsuario, int idJuego)
        {
            // Retorna: 0 (No tiene), 1 (Comprado), 2 (Descargado)
            try
            {
                using (var c = GetConnection())
                {
                    c.Open();
                    string q = "SELECT descargado FROM biblioteca WHERE id_usuario=@u AND id_videojuego=@g";

                    using (var cmd = new MySqlCommand(q, c))
                    {
                        cmd.Parameters.AddWithValue("@u", idUsuario);
                        cmd.Parameters.AddWithValue("@g", idJuego);

                        var res = cmd.ExecuteScalar();

                        // Si es nulo, es que no existe la compra
                        if (res == null || res == DBNull.Value) return 0;

                        // Conversión segura: MySQL a veces devuelve bool, sbyte o int
                        int valorDescargado = 0;
                        if (res is bool b) valorDescargado = b ? 1 : 0;
                        else valorDescargado = Convert.ToInt32(res);

                        // Lógica: Si descargado es 1, devolvemos estado 2. Si no, estado 1.
                        return (valorDescargado == 1) ? 2 : 1;
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de error, asumimos 0 pero podrías loguear 'ex.Message'
                return 0;
            }
        }

        public static void MarcarComoDescargado(int idUsuario, int idJuego)
        {
            // Si la columna no existe, fallará silenciosamente (catch en UI) o puedes añadirla a la BD
            try { EjecutarSQL($"UPDATE biblioteca SET descargado=1 WHERE id_usuario={idUsuario} AND id_videojuego={idJuego}"); } catch { }
        }

        public static void ComprarJuego(int idUsuario, int idJuego) => EjecutarSQL($"INSERT INTO biblioteca (id_usuario, id_videojuego) VALUES ({idUsuario}, {idJuego})");

        // --- CRUD JUEGOS (ADMIN) ---
        public static void CrearJuego(string titulo, decimal precio, double gb, string genero, int jugadores, bool online, string imagenPath, string imagenFondoPath)
        {
            if (string.IsNullOrEmpty(imagenPath)) imagenPath = "/Assets/logo.png";
            string sql = $@"INSERT INTO videojuegos (titulo, tematica, coste, espacio_gb, online, jugadores, imagen_url, imagen_fondo_url) 
                            VALUES ('{titulo}', '{genero}', {precio.ToString().Replace(",", ".")}, {gb.ToString().Replace(",", ".")}, {(online ? 1 : 0)}, {jugadores}, '{imagenPath}', '{imagenFondoPath}')";
            EjecutarSQL(sql);
        }

        public static void ActualizarJuego(int id, string titulo, decimal precio, double gb, string genero, int jugadores, bool online, string imagenPath, string imagenFondoPath)
        {
            string imgQuery = !string.IsNullOrEmpty(imagenPath) ? $", imagen_url='{imagenPath}'" : "";
            string imgFondoQuery = !string.IsNullOrEmpty(imagenFondoPath) ? $", imagen_fondo_url='{imagenFondoPath}'" : "";
            EjecutarSQL($@"UPDATE videojuegos SET titulo='{titulo}', tematica='{genero}', coste={precio.ToString().Replace(",", ".")}, 
                           espacio_gb={gb.ToString().Replace(",", ".")}, online={(online ? 1 : 0)}, jugadores={jugadores} {imgQuery} {imgFondoQuery} WHERE id={id}");
        }

        public static void EliminarJuego(int id) => EjecutarSQL($"DELETE FROM videojuegos WHERE id={id}");

        // --- LECTURA DE JUEGOS ---
        private static List<Juego> EjecutarConsultaJuegos(string query)
        {
            var l = new List<Juego>();
            try { using (var c = GetConnection()) { c.Open(); using (var r = new MySqlCommand(query, c).ExecuteReader()) while (r.Read()) l.Add(MapearJuego(r)); } } catch { }
            return l;
        }

        private static Juego MapearJuego(MySqlDataReader r)
        {
            bool tieneFondo = false;
            for (int i = 0; i < r.FieldCount; i++) if (r.GetName(i).Equals("imagen_fondo_url", StringComparison.InvariantCultureIgnoreCase)) tieneFondo = true;
            return new Juego
            {
                Id = Convert.ToInt32(r["id"]),
                Titulo = r["titulo"].ToString(),
                Genero = r["tematica"].ToString(),
                Precio = Convert.ToDecimal(r["coste"]),
                TamanoGb = Convert.ToDouble(r["espacio_gb"]),
                EsOnline = Convert.ToBoolean(r["online"]),
                NumJugadores = Convert.ToInt32(r["jugadores"]),
                EsDestacado = Convert.ToDecimal(r["coste"]) > 40,
                ImagenUrl = r["imagen_url"] != DBNull.Value ? r["imagen_url"].ToString() : "/Assets/logo.png",
                ImagenFondoUrl = (tieneFondo && r["imagen_fondo_url"] != DBNull.Value) ? r["imagen_fondo_url"].ToString() : ""
            };
        }

        // =============================================================
        // 2. GESTIÓN DE USUARIOS, SUSCRIPCIONES Y ADMIN
        // =============================================================

        public static bool RegisterUser(string n, string a, string u, string p, string e, string t, string c)
        {
            try { EjecutarSQL($"INSERT INTO usuarios (nombre,apellidos,username,password,email,telefono,cp) VALUES ('{n}','{a}','{u}','{p}','{e}','{t}','{c}')"); return true; } catch { return false; }
        }

        public static Usuario ValidateUser(string user, string pass, out string errorMsg)
        {
            errorMsg = "";
            using (var c = GetConnection())
            {
                c.Open();
                using (var r = new MySqlCommand($"SELECT * FROM usuarios WHERE username='{user}' AND password='{pass}'", c).ExecuteReader())
                {
                    if (r.Read())
                    {
                        if (!Convert.ToBoolean(r["activo"]))
                        {
                            DateTime? fin = r["fin_baneo"] != DBNull.Value ? (DateTime?)r["fin_baneo"] : null;
                            if (fin.HasValue && fin.Value <= DateTime.Now) { int uid = Convert.ToInt32(r["id"]); r.Close(); LevantarCastigo(uid, 0); return ValidateUser(user, pass, out errorMsg); }
                            errorMsg = "Bloqueado"; return null;
                        }
                        return new Usuario { Id = Convert.ToInt32(r["id"]), Username = r["username"].ToString(), Rol = r["rol"].ToString(), Activo = true, Suscripcion = r["suscripcion"].ToString() };
                    }
                }
            }
            errorMsg = "Error login"; return null;
        }

        public static List<Usuario> ObtenerUsuarios() => BuscarUsuarios("");
        public static List<Usuario> BuscarUsuarios(string f)
        {
            var l = new List<Usuario>();
            using (var c = GetConnection())
            {
                c.Open(); using (var r = new MySqlCommand($"SELECT * FROM usuarios WHERE username LIKE '%{f}%'", c).ExecuteReader()) while (r.Read()) l.Add(new Usuario
                {
                    Id = Convert.ToInt32(r["id"]),
                    Username = r["username"].ToString(),
                    Password = r["password"].ToString(),
                    Email = r["email"].ToString(),
                    Rol = r["rol"].ToString(),
                    Activo = Convert.ToBoolean(r["activo"]),
                    Suscripcion = r["suscripcion"].ToString(),
                    GradoBaneo = r["grado_baneo"] != DBNull.Value ? Convert.ToInt32(r["grado_baneo"]) : 0,
                    FinBaneo = r["fin_baneo"] != DBNull.Value ? (DateTime?)r["fin_baneo"] : null
                });
            }
            return l;
        }

        // MÉTODOS DE ADMIN Y SUSCRIPCIÓN QUE FALTABAN
        public static string GetSubscription(int id) { using (var c = GetConnection()) { c.Open(); var r = new MySqlCommand($"SELECT suscripcion FROM usuarios WHERE id={id}", c).ExecuteScalar(); return r != null ? r.ToString() : "FREE"; } }
        public static void UpdateSubscription(int id, string s) => EjecutarSQL($"UPDATE usuarios SET suscripcion='{s}' WHERE id={id}");
        public static void ActualizarUsuario(int id, string e, string r) => EjecutarSQL($"UPDATE usuarios SET email='{e}', rol='{r}' WHERE id={id}");
        public static void UpdatePassword(int id, string p) => EjecutarSQL($"UPDATE usuarios SET password='{p}' WHERE id={id}");
        public static void AdminCambiarPass(int id, string p, int admin) { UpdatePassword(id, p); RegistrarLog(admin, "PASS", $"Pass ID {id}"); }
        public static void AdminCrearUsuario(string u, string p, string e, string r, int admin) { EjecutarSQL($"INSERT INTO usuarios (username,password,email,rol,activo) VALUES ('{u}','{p}','{e}','{r}',1)"); RegistrarLog(admin, "CREAR", $"Usuario {u}"); }
        public static void EliminarUsuarioTotal(int id, int admin) { EjecutarSQL($"DELETE FROM usuarios WHERE id={id}"); RegistrarLog(admin, "ELIMINAR", $"Borrado ID {id}"); }

        public static void AplicarSancion(int id, int grado, int admin)
        {
            DateTime fin = DateTime.Now; switch (grado) { case 1: fin = fin.AddDays(1); break; case 2: fin = fin.AddDays(3); break; case 3: fin = fin.AddDays(7); break; case 4: fin = fin.AddMonths(1); break; case 5: fin = new DateTime(9999, 12, 31); break; }
            EjecutarSQL($"UPDATE usuarios SET activo=0, grado_baneo={grado}, fin_baneo='{fin:yyyy-MM-dd HH:mm:ss}' WHERE id={id}"); RegistrarLog(admin, "BANEO", $"Sanción {grado}");
        }
        public static void LevantarCastigo(int id, int admin) { EjecutarSQL($"UPDATE usuarios SET activo=1, grado_baneo=0, fin_baneo=NULL WHERE id={id}"); if (admin > 0) RegistrarLog(admin, "INDULTO", $"Desbaneo {id}"); }

        public static void RegistrarLog(int id, string a, string d) => EjecutarSQL($"INSERT INTO log_actividad (id_admin, accion, descripcion) VALUES ({id}, '{a}', '{d}')");
        public static string ObtenerReporteLogs()
        {
            var sb = new StringBuilder(); sb.AppendLine("LOGS"); try { using (var c = GetConnection()) { c.Open(); using (var r = new MySqlCommand("SELECT * FROM log_actividad ORDER BY fecha_hora DESC", c).ExecuteReader()) while (r.Read()) sb.AppendLine($"{r["fecha_hora"]} | {r["accion"]} | {r["descripcion"]}"); } } catch { }
            return sb.ToString();
        }

        // --- SOPORTE Y APELACIONES ---
        public static void EnviarTicketSoporte(int u, string m, string msg) => EjecutarSQL($"INSERT INTO soporte_tickets (id_usuario, motivo, mensaje) VALUES ({u}, '{m}', '{msg}')");
        public static List<TicketSoporte> ObtenerTodosLosTickets()
        {
            var l = new List<TicketSoporte>(); try { using (var c = GetConnection()) { c.Open(); using (var r = new MySqlCommand("SELECT s.*, u.username FROM soporte_tickets s JOIN usuarios u ON s.id_usuario=u.id ORDER BY s.fecha DESC", c).ExecuteReader()) while (r.Read()) l.Add(new TicketSoporte { Id = Convert.ToInt32(r["id"]), Usuario = r["username"].ToString(), Motivo = r["motivo"].ToString(), Mensaje = r["mensaje"].ToString(), Estado = r["estado"].ToString(), Fecha = Convert.ToDateTime(r["fecha"]) }); } } catch { }
            return l;
        }
        public static void CerrarTicket(int id) => EjecutarSQL($"UPDATE soporte_tickets SET estado='RESUELTO' WHERE id={id}");

        public static void EnviarApelacion(string u, string m) { try { int id = GetUserId(u); if (id > 0) EjecutarSQL($"INSERT INTO apelaciones (id_usuario,username,mensaje) VALUES ({id},'{u}','{m}')"); } catch { } }
        public static List<Apelacion> ObtenerApelaciones() { var l = new List<Apelacion>(); try { using (var c = GetConnection()) { c.Open(); using (var r = new MySqlCommand("SELECT * FROM apelaciones", c).ExecuteReader()) while (r.Read()) l.Add(new Apelacion { Id = Convert.ToInt32(r["id"]), Username = r["username"].ToString(), Mensaje = r["mensaje"].ToString(), Fecha = Convert.ToDateTime(r["fecha"]) }); } } catch { } return l; }
        public static void EliminarApelacion(int id) => EjecutarSQL($"DELETE FROM apelaciones WHERE id={id}");

        // --- AUXILIARES ---
        private static int GetUserId(string u) { using (var c = GetConnection()) { c.Open(); var r = new MySqlCommand($"SELECT id FROM usuarios WHERE username='{u}'", c).ExecuteScalar(); return r != null ? Convert.ToInt32(r) : -1; } }
        private static void EjecutarSQL(string q) { using (var c = GetConnection()) { c.Open(); new MySqlCommand(q, c).ExecuteNonQuery(); } }
    }
}