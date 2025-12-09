using Microsoft.Data.SqlClient;
using SIJUPAY.Models;
using System.Data;
using System.Numerics;

namespace SIJUPAY.Logica
{
    public class Logica_Usuarios
    {
        public Usuario encontrarUsuario(string telefono, string contraseña)
        {
            Usuario objeto = new Usuario();
            using (SqlConnection conexion = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;AttachDBfilename=|DataDirectory|\\SijuPay.mdf;Integrated Security=True"))
            {
                string consulta = "select IdUsuario, Nombre, Apellido, Contrasena, Telefono, TipoUsuario from Usuario where Telefono = @ptelefono and Contrasena = @pcontrasena";
                SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.AddWithValue("@ptelefono", telefono);
                comando.Parameters.AddWithValue("@pcontrasena", contraseña);
                comando.CommandType = CommandType.Text;
                conexion.Open();
                using (SqlDataReader datos = comando.ExecuteReader())
                {
                    while (datos.Read())
                    {
                        objeto = new Usuario()
                        {
                            IdUsuario = (int)datos["IdUsuario"],
                            Nombre = datos["Nombre"].ToString(),
                            Apellido = datos["Apellido"].ToString(),
                            Contrasena = datos["Contrasena"].ToString(),
                            Telefono = datos["Telefono"].ToString(),
                            TipoUsuario = datos["TipoUsuario"].ToString()
                        };
                    }
                }
            }
            return objeto;
        }
        public bool registrarUsuario(Usuario objeto)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;AttachDBfilename=|DataDirectory|\\SijuPay.mdf;Integrated Security=True"))
            {
                string consulta = "INSERT INTO Usuario (Nombre, Apellido, Contrasena, Telefono, TipoUsuario) VALUES (@pnombre, @papellido, @pcontrasena, @ptelefono, @ptipousuario)";

                SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.AddWithValue("@pnombre", objeto.Nombre);
                comando.Parameters.AddWithValue("@papellido", objeto.Apellido);
                comando.Parameters.AddWithValue("@pcontrasena", objeto.Contrasena);
                comando.Parameters.AddWithValue("@ptelefono", objeto.Telefono);
                comando.Parameters.AddWithValue("@ptipousuario", "Cliente"); 
                comando.CommandType = CommandType.Text;

                try
                {
                    conexion.Open();
                    if (comando.ExecuteNonQuery() > 0)
                    {
                        respuesta = true;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                }
            }
            return respuesta;
        }

        public bool existeTelefono(string telefono)
        {
            bool existe = false;
            using (SqlConnection conexion = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;AttachDBfilename=|DataDirectory|\\SijuPay.mdf;Integrated Security=True"))
            {
                string consulta = "SELECT COUNT(*) FROM Usuario WHERE Telefono = @ptelefono";
                SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.AddWithValue("@ptelefono", telefono);
                comando.CommandType = CommandType.Text;

                try
                {
                    conexion.Open();
                    int count = (int)comando.ExecuteScalar();

                    if (count > 0)
                    {
                        existe = true;
                    }
                }
                catch (Exception ex)
                {
                    existe = false;
                }
            }
            return existe;
        }
        public Usuario obtenerUsuarioPorTelefono(string telefono)
        {
            Usuario objeto = new Usuario();
            using (SqlConnection conexion = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;AttachDBfilename=|DataDirectory|\\SijuPay.mdf;Integrated Security=True"))
            {
                string consulta = "SELECT IdUsuario, Nombre, Apellido, Telefono, TipoUsuario FROM Usuario WHERE Telefono = @ptelefono";
                SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.AddWithValue("@ptelefono", telefono);
                comando.CommandType = CommandType.Text;
                conexion.Open();

                using (SqlDataReader datos = comando.ExecuteReader())
                {
                    if (datos.Read())
                    {
                        objeto = new Usuario()
                        {
                            IdUsuario = (int)datos["IdUsuario"],
                            Nombre = datos["Nombre"].ToString(),
                            Apellido = datos["Apellido"].ToString(),
                            Telefono = datos["Telefono"].ToString(),
                            TipoUsuario = datos["TipoUsuario"].ToString()
                        };
                    }
                }
            }
            return objeto;
        }
    }
}
