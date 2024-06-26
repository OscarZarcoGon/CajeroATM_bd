using System;
using System.Collections.Generic;
using System.Data.Odbc;

namespace Cajero.ATM
{
    public class Conexion
    {
        private string conexionString = "Dsn=ODBCoscarSQL;Uid=daniel;Pwd=dani123;";
        public OdbcConnection conexion;
        public List<Message> oMessages = new List<Message>();

        public Conexion()
        {
            conexion = new OdbcConnection(conexionString);
        }

        public void Conectar()
        {
            try
            {
                conexion.Open();
                oMessages.AddMessage(0, "Conexión exitosa a la bd", TypeMessages.success);
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, "Error" + ex.Message, TypeMessages.error);
            }
        }

        public void Desconectar()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                }
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, "Error" + ex.Message, TypeMessages.error);
            }
        }

        public void EjecutarComando(string comando)
        {
            try
            {
                Conectar();
                OdbcCommand cmd = new OdbcCommand(comando, conexion);
                cmd.ExecuteNonQuery();
                Desconectar();
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, "Error" + ex.Message, TypeMessages.error);
            }
        }

        public object EjecutarEscalar(string query)
        {
            object result = null;
            try
            {
                Conectar();
                OdbcCommand cmd = new OdbcCommand(query, conexion);
                result = cmd.ExecuteScalar();
                Desconectar();
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, "Error" + ex.Message, TypeMessages.error);
            }
            return result;
        }
    }
}
