using Microsoft.Data.SqlClient;
using Probar_hacer_login.Models;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace Probar_hacer_login.Datos
{
    public class UsuarioDatos
    {

        // Metodo para registrar un nuevo usuario en la base de datos
        public bool Registrar(UsuarioModels oUsuario)
        {
            bool rpta;

            try
            {
                var cn = new Conexion();

                using (var conexion = new SqlConnection(cn.getCadenaSQL()))
                {
                    conexion.Open();

                    SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", conexion);

                    string contraseñaHasheada = BCrypt.Net.BCrypt.HashPassword(oUsuario.Contraseña);

                    cmd.Parameters.AddWithValue("Email", oUsuario.Email);
                    cmd.Parameters.AddWithValue("Contraseña", contraseñaHasheada);
                    cmd.Parameters.AddWithValue("Documento", oUsuario.Documento);
                    cmd.Parameters.AddWithValue("Nombre", oUsuario.Nombre);
                    cmd.Parameters.AddWithValue("Apellido", oUsuario.Apellido);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                rpta = true;
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
                rpta = false;
            }
            return rpta;
        }


        public UsuarioModels ValidarCredenciales(string email, string contraseña)
        {
            UsuarioModels oUsuario = null;

            try
            {
                var cn = new Conexion();

                using (var conexion = new SqlConnection(cn.getCadenaSQL()))
                {
                    conexion.Open();

                    SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", conexion);
                    cmd.Parameters.AddWithValue("Email", email);

                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            oUsuario = new UsuarioModels()
                            {
                                Email = dr["Email"].ToString(),
                                Contraseña = dr["Contraseña"].ToString(), // Esto es el hash
                                Documento = dr["Documento"].ToString(),
                                Nombre = dr["Nombre"].ToString(),
                                Apellido = dr["Apellido"].ToString()
                            };

                            bool esCorrecta = BCrypt.Net.BCrypt.Verify(contraseña, oUsuario.Contraseña);
                            if (!esCorrecta)
                            {
                                oUsuario = null;
                            }
                        }

                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                oUsuario = null;
            }
            return oUsuario;
        }


        public UsuarioModels ObtenerPorEmail(string email)
        {
            UsuarioModels oUsuario = null;
            try
            {
                var cn = new Conexion();
                using (var conexion = new SqlConnection(cn.getCadenaSQL()))
                {
                    conexion.Open();
                    string query = "SELECT Email, Contraseña, Documento , Nombre, Apellido FROM USUARIO WHERE Email = @Email";
                    using (var cmd = new SqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                oUsuario = new UsuarioModels()
                                {
                                     Email = dr["Email"].ToString(),
                                     Contraseña = dr["Contraseña"].ToString(),
                                     Documento = dr["Documento"].ToString(),
                                     Nombre = dr["Nombre"].ToString(),
                                     Apellido = dr["Apellido"].ToString()
                                };
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return oUsuario;
        }



    }
}
