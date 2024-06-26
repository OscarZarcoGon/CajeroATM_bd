namespace Cajero.ATM
{
    public class CuentaBancaria
    {
        public int ID { get; set; }
        public int Numero_Cuenta { get; set; }
        public decimal Saldo { get; set; }
        public decimal Limite { get; set; }
        public DateTime Fecha_Corte { get; set; }
        public bool Status { get; set; }
        public Cliente Propietario { get; set; }
        public Tarjeta TarjetaAsociada { get; set; }

        public CuentaBancaria() { }

        public CuentaBancaria(int numeroCuenta, decimal saldo, decimal limite, DateTime fechaCorte, bool status, Cliente propietario, Tarjeta tarjeta)
        {
            this.Numero_Cuenta = numeroCuenta;
            this.Saldo = saldo;
            this.Limite = limite;
            this.Fecha_Corte = fechaCorte;
            this.Status = status;
            this.Propietario = propietario;
            this.TarjetaAsociada = tarjeta;
        }
    }
}
