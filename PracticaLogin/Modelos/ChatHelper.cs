using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient; // Necesario para la BBDD
using System.Data;            // Necesario para leer datos

namespace PracticaLogin
{
    public static class ChatHelper
    {
        // 1. OBTENER LISTA DE AMIGOS
        public static List<Amigo> ObtenerAmigos(int miIdUsuario)
        {
            List<Amigo> lista = new List<Amigo>();

            try
            {
                // Usamos la conexión pública del DatabaseHelper
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Consulta: Busca amigos ACEPTADOS y trae su estado
                    string query = @"
                        SELECT u.id, u.username, u.estado_presencia 
                        FROM amistades a
                        JOIN usuarios u ON (a.id_usuario1 = u.id OR a.id_usuario2 = u.id)
                        WHERE (a.id_usuario1 = @miId OR a.id_usuario2 = @miId)
                        AND u.id != @miId 
                        AND a.estado = 'ACEPTADA'";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@miId", miIdUsuario);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Protección por si la columna estado_presencia no existe en BBDD
                                string estadoDB = "Offline";
                                try
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal("estado_presencia")))
                                        estadoDB = reader.GetString("estado_presencia");
                                }
                                catch { }

                                Amigo amigo = new Amigo
                                {
                                    Id = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Estado = estadoDB
                                };
                                amigo.ActualizarColor(); // Pone el color verde/naranja/gris
                                lista.Add(amigo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de error (ej: BBDD apagada), devolvemos lista vacía para que no pete la app
                System.Diagnostics.Debug.WriteLine("Error ChatHelper: " + ex.Message);
            }
            return lista;
        }

        // 2. OBTENER MENSAJES
        public static List<MensajeChat> ObtenerConversacion(int miId, int idAmigo)
        {
            List<MensajeChat> chat = new List<MensajeChat>();

            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT id_remitente, mensaje, fecha_envio 
                        FROM chat_mensajes 
                        WHERE (id_remitente = @miId AND id_destinatario = @idAmigo) 
                           OR (id_remitente = @idAmigo AND id_destinatario = @miId)
                        ORDER BY fecha_envio ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@miId", miId);
                        cmd.Parameters.AddWithValue("@idAmigo", idAmigo);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int idRemitente = reader.GetInt32("id_remitente");
                                bool soyYo = (idRemitente == miId);

                                MensajeChat msg = new MensajeChat
                                {
                                    IdRemitente = idRemitente,
                                    Texto = reader.GetString("mensaje"),
                                    Fecha = reader.GetDateTime("fecha_envio"),

                                    // Configuración Visual
                                    Alineacion = soyYo ? "Right" : "Left",
                                    ColorFondo = soyYo ? "#005C4B" : "#202C33",
                                    ColorTexto = "White"
                                };
                                chat.Add(msg);
                            }
                        }
                    }
                }
            }
            catch { }
            return chat;
        }

        // 3. ENVIAR MENSAJE
        public static void EnviarMensaje(int idRemitente, int idDestinatario, string mensaje)
        {
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO chat_mensajes (id_remitente, id_destinatario, mensaje, fecha_envio) VALUES (@rem, @dest, @msg, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@rem", idRemitente);
                        cmd.Parameters.AddWithValue("@dest", idDestinatario);
                        cmd.Parameters.AddWithValue("@msg", mensaje);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
    }
}