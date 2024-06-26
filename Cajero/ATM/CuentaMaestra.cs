using System;
using System.Collections.Generic;
using System.Data.Odbc;

namespace Cajero.ATM
{
    public class CuentaMaestra
    {
        public int ID { get; set; }
        public DateTime Fecha_Transaccion { get; set; }
        public decimal Monto_Transaccion { get; set; }
        public string Descripcion { get; set; }
        public List<CuentaBancaria> Cuentas { get; set; }
        public List<Message> oMessages = new List<Message>();

        public CuentaMaestra()
        {
            Cuentas = new List<CuentaBancaria>();
        }

        public CuentaMaestra(int id, DateTime fechaTransaccion, decimal montoTransaccion)
        {
            this.ID = id;
            this.Fecha_Transaccion = fechaTransaccion;
            this.Monto_Transaccion = montoTransaccion;
            Cuentas = new List<CuentaBancaria>();
        }

        public CuentaBancaria BuscarCuentaPorTarjeta(long numeroTarjeta)
        {
            Conexion conexion = new Conexion();
            string consulta = $"SELECT cb.ID, cb.numero_cuenta, cb.saldo, cb.limite, cb.fecha_corte, cb.estado, cb.propietario_fk " +
                              $"FROM cuentaBancaria cb " +
                              $"INNER JOIN tarjeta t ON cb.tarjetaAsociada_fk = t.ID " +
                              $"WHERE t.numero_tarjeta = {numeroTarjeta}";

            conexion.Conectar();
            OdbcCommand comando = new OdbcCommand(consulta, conexion.conexion);
            OdbcDataReader reader = comando.ExecuteReader();
            CuentaBancaria cuenta = null;

            if (reader.Read())
            {
                cuenta = new CuentaBancaria
                {
                    ID = reader.GetInt32(0),
                    Numero_Cuenta = reader.GetInt32(1),
                    Saldo = reader.GetDecimal(2),
                    Limite = reader.GetDecimal(3),
                    Fecha_Corte = reader.GetDateTime(4),
                    Status = reader.GetBoolean(5),
                    Propietario = new Cliente { ID = reader.GetInt32(6) }
                };
            }
            reader.Close();
            conexion.Desconectar();

            return cuenta;
        }

        public bool RetirarDinero(long numeroTarjeta, decimal cantidad)
        {
            try
            {
                CuentaBancaria cuenta = BuscarCuentaPorTarjeta(numeroTarjeta);
                if (cuenta != null && cuenta.Saldo >= cantidad)
                {
                    cuenta.Saldo -= cantidad;

                    int tipoTransaccion = ObtenerIdTipoTransaccion("Retiro");

                    Conexion conexion = new Conexion();
                    string comando = $"UPDATE cuentaBancaria SET saldo = {cuenta.Saldo} WHERE ID = {cuenta.ID}";
                    conexion.EjecutarComando(comando);

                    GuardarTransaccion(cuenta.ID, cantidad, tipoTransaccion);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, ex.Message, TypeMessages.error);
                return false;
            }
        }

        public bool IngresarDinero(long numeroTarjeta, decimal cantidad)
        {
            try
            {
                CuentaBancaria cuenta = BuscarCuentaPorTarjeta(numeroTarjeta);
                if (cuenta != null)
                {
                    cuenta.Saldo += cantidad;

                    int tipoTransaccion = ObtenerIdTipoTransaccion("Depósito");

                    Conexion conexion = new Conexion();
                    string comando = $"UPDATE cuentaBancaria SET saldo = {cuenta.Saldo} WHERE ID = {cuenta.ID}";
                    conexion.EjecutarComando(comando);

                    GuardarTransaccion(cuenta.ID, cantidad, tipoTransaccion);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, ex.Message, TypeMessages.error);
                return false;
            }
        }

        public bool TransferirDinero(long numeroTarjetaOrigen, long numeroTarjetaDestino, decimal cantidad)
        {
            try
            {
                CuentaBancaria cuentaOrigen = BuscarCuentaPorTarjeta(numeroTarjetaOrigen);
                CuentaBancaria cuentaDestino = BuscarCuentaPorTarjeta(numeroTarjetaDestino);

                if (cuentaOrigen != null && cuentaDestino != null && cuentaOrigen.Saldo >= cantidad)
                {
                    cuentaOrigen.Saldo -= cantidad;
                    Conexion conexion = new Conexion();
                    string comandoRetiro = $"UPDATE cuentaBancaria SET saldo = {cuentaOrigen.Saldo} WHERE ID = {cuentaOrigen.ID}";
                    conexion.EjecutarComando(comandoRetiro);

                    cuentaDestino.Saldo += cantidad;
                    string comandoDeposito = $"UPDATE cuentaBancaria SET saldo = {cuentaDestino.Saldo} WHERE ID = {cuentaDestino.ID}";
                    conexion.EjecutarComando(comandoDeposito);

                    int tipoTransaccion = ObtenerIdTipoTransaccion("Transferencia");

                    GuardarTransaccion(cuentaOrigen.ID, cantidad, tipoTransaccion);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, ex.Message, TypeMessages.error);
                return false;
            }
        }

        public CuentaBancaria ConsultarSaldo(long numeroTarjeta)
        {
            return BuscarCuentaPorTarjeta(numeroTarjeta);
        }
        public bool CambiarNIP(long numeroTarjeta, int nuevoNIP)
        {
            Conexion conexion = new Conexion();
            string comando = $"UPDATE tarjeta SET NIP = {nuevoNIP} WHERE numero_tarjeta = {numeroTarjeta}";
            conexion.EjecutarComando(comando);
            return true;
        }

        //SIGUE ESTANDO MAL EL MÉTODO RegistrarCuenta, TENGO QUE BUSCAR UNA SOLUCIÓN, que no lleve parámetros 
        public bool RegistrarCuenta(CuentaBancaria nuevaCuenta)
        {
            try
            {
                Conexion conexion = new Conexion();
                conexion.Conectar();

                string tarjeta = $"INSERT INTO tarjeta(numero_tarjeta, nip, fecha_vencimiento, estado) VALUES({nuevaCuenta.TarjetaAsociada.Numero_Tarjeta}, {nuevaCuenta.TarjetaAsociada.NIP}, '2025-06-15', 1); SELECT SCOPE_IDENTITY();";
                int tarjetaID = Convert.ToInt32(conexion.EjecutarEscalar(tarjeta));

                string cliente = $"INSERT INTO clientes(numero_cliente, nombre, fecha_nacimiento, telefono) VALUES({nuevaCuenta.Propietario.NumeroCliente}, '{nuevaCuenta.Propietario.Nombre}', '{nuevaCuenta.Propietario.FechaNacimiento:yyyy-MM-dd}', {nuevaCuenta.Propietario.Telefono}); SELECT SCOPE_IDENTITY();";
                int clienteID = Convert.ToInt32(conexion.EjecutarEscalar(cliente));

                string cuentaBancaria = $"INSERT INTO cuentaBancaria (numero_cuenta, saldo, limite, fecha_corte, estado, propietario_fk, tarjetaAsociada_fk) VALUES({nuevaCuenta.Numero_Cuenta}, {nuevaCuenta.Saldo}, {nuevaCuenta.Limite}, '{nuevaCuenta.Fecha_Corte:yyyy-MM-dd}', 1, {clienteID}, {tarjetaID})";
                conexion.EjecutarComando(cuentaBancaria);

                conexion.Desconectar();

                return true;
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, ex.Message, TypeMessages.error);
                return false;
            }
        }







        private int ObtenerIdTipoTransaccion(string tipo)
        {
            Conexion conexion = new Conexion();
            string consulta = $"SELECT ID FROM TipoTransaccion WHERE Tipo = '{tipo}'";
            conexion.Conectar();
            OdbcCommand comando = new OdbcCommand(consulta, conexion.conexion);
            int idTipoTransaccion = (int)comando.ExecuteScalar();
            conexion.Desconectar();
            return idTipoTransaccion;
        }

        private void GuardarTransaccion(int cuentaId, decimal monto, int tipoTransaccion)
        {
            try
            {
                if (monto >= 0)
                {
                    Conexion conexion = new Conexion();
                    string comando = $"INSERT INTO cuentaMaestra (fecha_transaccion, monto_transaccion, tipo_transaccion_fk, cuenta_fk) VALUES ('{DateTime.Now.ToString("yyyy-MM-dd")}', {monto}, '{tipoTransaccion}', {cuentaId})";
                    conexion.EjecutarComando(comando);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, ex.Message, TypeMessages.error);
                return;
            }
        }

        public bool ValidarTarjeta(long numeroTarjeta, int nip)
        {
            try
            {
                Conexion conexion = new Conexion();
                string consulta = $"SELECT numero_tarjeta, NIP FROM tarjeta WHERE numero_tarjeta = {numeroTarjeta} AND NIP = {nip}";

                conexion.Conectar();
                OdbcCommand comando = new OdbcCommand(consulta, conexion.conexion);
                OdbcDataReader reader = comando.ExecuteReader();

                bool tarjetaValida = reader.HasRows;

                reader.Close();
                conexion.Desconectar();

                return tarjetaValida;
            }
            catch (Exception ex)
            {
                oMessages.AddMessage(999, ex.Message, TypeMessages.error);
                return false;
            }
        }
    }
}
