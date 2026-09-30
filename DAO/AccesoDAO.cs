using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAO
{
    public class AccesoDAO
    {
        private readonly Conexion _conexion;

        public AccesoDAO(Conexion conexion)
        {
            _conexion = conexion;
        }
        public bool ValidarAcceso(int IdPerfil, string ControllerName, string ActionName, string Base)
        {
            bool exito = false;
            using (SqlConnection cn =  _conexion.CrearConexion(Base))
            {
                try
                {
                    cn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SMC_VALIDAR_ACCESO", cn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@IdPerfil", IdPerfil);
                    da.SelectCommand.Parameters.AddWithValue("@ControllerName", ControllerName);
                    da.SelectCommand.Parameters.AddWithValue("@ActionName", ActionName);
                    SqlDataReader drd = da.SelectCommand.ExecuteReader();
                    while (drd.Read())
                    {
                        string menu = drd["idMenu"].ToString();
                        exito = true;
                    }
                    drd.Close();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return exito;
        }
    }
}
